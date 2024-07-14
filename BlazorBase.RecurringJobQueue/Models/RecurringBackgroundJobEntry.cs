using BlazorBase.Abstractions.CRUD.Attributes;
using BlazorBase.Abstractions.CRUD.Enums;
using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace BlazorBase.RecurringBackgroundJobQueue.Models;

[Route("/RecurringBackgroundJobs")]
[Authorize(Policy = nameof(RecurringBackgroundJobEntry))]
public partial class RecurringBackgroundJobEntry : BaseModel
{
    [Key]
    [Required]
    [Visible(DisplayOrder = 100)]
    public string Name { get; set; } = null!;
    
    [Required]
    [Visible(DisplayOrder = 200)]
    [PresentationDataType(PresentationDataType.DateTime)]
    public DateTime LastRuntime { get; set; } = DateTime.MinValue;
    
    [Required]
    [Visible(DisplayOrder = 200)]
    [PresentationDataType(PresentationDataType.DateTime)]
    public DateTime NextRuntime { get; set; } = DateTime.MinValue;

    [Visible(DisplayGroup = "Log", DisplayGroupOrder = 200, DisplayOrder = 100, HideInGUITypes = [GUIType.List])]
    public string? Log { get; set; } = null;

    [Visible(DisplayGroup = "Last Errors", DisplayGroupOrder = 300, DisplayOrder = 100)]
    public string? LastErrors { get; set; } = null;
}
