using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.SortableItem;
using BlazorBase.Files.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorBase.AudioRecorder.Models;

public class BaseAudioRecord : BaseModel, IBaseAudioRecord, ISortableItem
{
    #region Properties

    [Key]
    public Guid Id { get; set; }

    [Required]
    [DisplayKey]
    [Visible(DisplayOrder = 200, HideInGUITypes = new GUIType[] { GUIType.Card, GUIType.ListPart })]
    public string FileName { get; set; } = null!;

    [Editable(false)]
    [Visible(DisplayOrder = 300, HideInGUITypes = new GUIType[] { GUIType.Card, GUIType.ListPart })]
    public string? MimeFileType { get; set; }

    [Editable(false)]
    [Visible(DisplayOrder = 400, HideInGUITypes = new GUIType[] { GUIType.ListPart })]
    public string? Hash { get; set; }

    [NotMapped]
    public Guid? TempAudioFileId { get; set; }

    public Guid? AudioFileId { get; set; }

    [ForeignKey(nameof(AudioFileId))]
    public virtual IBaseFile? AudioFile { get; set; }

    public int SortIndex { get; set; }

    /// <summary>
    /// This property is only needed to show the file in the general base file list and card.
    /// </summary>
    [NotMapped]
    [Visible(DisplayOrder = 100)]
    [CustomClassAndStyle(Style = "min-width:200px", Locations = [CustomizationLocation.ListHeaderCell], ValidInGUITypes = [GUIType.List])]
    public virtual IBaseAudioRecord DisplayAudioRecord { get { return this; } }

    #endregion

    #region CRUD

    public override async Task OnCreateNewEntryInstance(OnCreateNewEntryInstanceArgs args)
    {
        Id = await args.EventServices.DbContext.GetNewPrimaryKeyAsync(GetType());

        (await args.EventServices.DbContext.EntryAsync(this)).State = EntityState.Added; //Needed for some Reason, because ef not detect that when a record is a navigation property and is newly added, it must add the record before it add or update the entity itself
    }

    public override Task OnAfterCardSaveChanges(OnAfterCardSaveChangesArgs args)
    {
        if (AudioFile == null || TempAudioFileId == null || TempAudioFileId == Guid.Empty)
            return Task.CompletedTask;

        TempAudioFileId = AudioFile.TempFileId;
        return Task.CompletedTask;
    }

    public override async Task OnBeforeDbContextDeleteEntry(OnBeforeDbContextDeleteEntryArgs args)
    {        
        await args.EventServices.DbContext.LoadReferenceAsync(this, entry => entry.AudioFile);
        
        if (AudioFile != null)
        {
            args.AdditionalEntriesDeleted.Add(AudioFile);
            await args.EventServices.DbContext.RemoveAsync(AudioFile);
        }
    }

    public override async Task OnAfterRemoveEntry(OnAfterRemoveEntryArgs args)
    {
        var entry = await args.EventServices.DbContext.EntryAsync(this);
        if (entry.State != EntityState.Detached) // Only relevant if removed from another entity as list property
            entry.State = entry.State == EntityState.Added ? EntityState.Detached : EntityState.Deleted;

        await base.OnAfterRemoveEntry(args);
    }

    #endregion

    #region Methods

    public virtual string? GetFileLink()
    {
        if (AudioFileId == null || FileName == null)
            return null;

        if (TempAudioFileId == null || TempAudioFileId == Guid.Empty)
            return $"/{BlazorBaseFileOptions.Instance.ControllerRoute}/GetFile/{AudioFileId}/{Uri.EscapeDataString(FileName)}?hash={Hash}"; //Append Hash for basic browser file cache refresh notification
        else
            return $"/{BlazorBaseFileOptions.Instance.ControllerRoute}/GetTemporaryFile/{TempAudioFileId}/{Uri.EscapeDataString(FileName)}?hash={Hash}";
    }

    #endregion
}
