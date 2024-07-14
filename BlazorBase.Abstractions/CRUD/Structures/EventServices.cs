using BlazorBase.Abstractions.CRUD.Interfaces;
using Microsoft.Extensions.Localization;

namespace BlazorBase.Abstractions.CRUD.Structures;

/// <summary>
/// Collection of services which can be used in events
/// </summary>
/// <param name="ServiceProvider"></param>
/// <param name="DbContext"></param>
/// <param name="Localizer"></param>
public record EventServices(IServiceProvider ServiceProvider, IBaseDbContext DbContext, IStringLocalizer Localizer)
{
}
