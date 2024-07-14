using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Server.Services;
using BlazorBase.Server.User.Models;
using BlazorBase.User.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Collections.Concurrent;

namespace BlazorBase.Server.User.Services;

public class BaseUserCircuitHandlerService : BaseCircuitHandlerService
{
    #region Injects
    protected IBaseUserService BaseUserService { get; }
    protected IBaseDbContext DbContext { get; }
    #endregion

    #region Properties

    public static IReadOnlyDictionary<Guid, List<string>> UserSessions { get { return LocalUserSessions; } }
    public Guid? CurrentUserId { get; protected set; }

    #endregion

    #region Members

    protected static ConcurrentDictionary<Guid, List<string>> LocalUserSessions { get; set; } = new();
    protected static readonly object SessionLock = new();

    #endregion

    #region Init

    public BaseUserCircuitHandlerService(IBaseUserService baseUserService, IBaseDbContext dbContext)
    {
        BaseUserService = baseUserService;
        DbContext = dbContext;
    }

    #endregion

    public static bool UserIsOnline(Guid userId)
    {
        return LocalUserSessions.ContainsKey(userId);
    }

    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var user = await BaseUserService.GetCurrentUserAsync(DbContext, asNoTracking: false);
        CurrentUserId = user?.Id;
        if (user is IBaseUserSessionData extendedUser)
        {
            extendedUser.LastSessionCreatedOn = DateTime.Now;
            await DbContext.SaveChangesAsync();
        }

        if (user?.Id != null)
        {
            lock (SessionLock)
            {
                if (!LocalUserSessions.ContainsKey(user.Id))
                    LocalUserSessions[user.Id] = [];
                LocalUserSessions[user.Id].Add(circuit.Id);
            }
        }

        await base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (CurrentUserId != null)
        {
            lock (SessionLock)
            {
                if (LocalUserSessions.ContainsKey(CurrentUserId.Value))
                {
                    LocalUserSessions[CurrentUserId.Value].Remove(circuit.Id);

                    if (LocalUserSessions[CurrentUserId.Value].Count == 0)
                        LocalUserSessions.TryRemove(CurrentUserId.Value, out _);
                }
            }
        }

        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}
