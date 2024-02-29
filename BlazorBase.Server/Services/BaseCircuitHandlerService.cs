using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Collections.Concurrent;

namespace BlazorBase.Server.Services;

public class BaseCircuitHandlerService : CircuitHandler
{
    #region Properties
    public static event EventHandler? OnCircuitsChanged;
    public string CurrentCircuitId { get; protected set; } = null!;
    #endregion

    #region Properties
    public static IReadOnlyDictionary<string, Circuit> Circuits { get { return LocalCircuits; } }
    #endregion

    #region Members
    protected static ConcurrentDictionary<string, Circuit> LocalCircuits { get; set; } = new();
    #endregion

    protected virtual void CallCircuitsChangedEvent() => OnCircuitsChanged?.Invoke(this, EventArgs.Empty);

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        CurrentCircuitId = circuit.Id;
        LocalCircuits[circuit.Id] = circuit;
        CallCircuitsChangedEvent();

        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        LocalCircuits.TryRemove(circuit.Id, out _);
        CallCircuitsChangedEvent();

        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        return base.OnConnectionDownAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }
}
