using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace BlazorBase.DataUpgrade;

[Route("/DataUpgradeEntries")]
[Authorize(Policy = nameof(DataUpgradeEntry))]
public class DataUpgradeEntry : BaseModel
{
    [Key]
    [StringLength(100)]
    [Visible(DisplayOrder = 100, SortDirection = SortDirection.Descending)]
    public string Id { get; set; } = default!;

    [Editable(false)]
    [Visible(DisplayOrder = 200)]
    public string? Description { get; set; }

    [Editable(false)]
    [Visible(DisplayGroup = "Log", DisplayGroupOrder = 200, DisplayOrder = 100, HideInGUITypes = new GUIType[] { GUIType.List })]
    public string? Log { get; set; }
}
