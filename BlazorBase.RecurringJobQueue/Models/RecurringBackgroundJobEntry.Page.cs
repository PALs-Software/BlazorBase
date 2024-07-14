using BlazorBase.Abstractions.CRUD.Enums;
using BlazorBase.Abstractions.CRUD.Structures;
using BlazorBase.MessageHandling.Interfaces;
using Blazorise.Icons.FontAwesome;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBase.RecurringBackgroundJobQueue.Models;

public partial class RecurringBackgroundJobEntry
{
    public override bool UserCanAddEntries => false;
    public override bool UserCanDeleteEntries => false;
    public override bool UserCanEditEntries => false;
    public override bool UserCanOpenCardReadOnly => true;

    public override Task<List<PageActionGroup>?> GeneratePageActionGroupsAsync(EventServices eventServices)
    {
        return Task.FromResult<List<PageActionGroup>?>(
        [
            new()
            {
                Caption = PageActionGroup.DefaultGroups.Process,
                Image = FontAwesomeIcons.Bolt,
                VisibleInGUITypes = [GUIType.List, GUIType.Card],
                PageActions =
                [
                    new()
                    {
                        Caption = "Run Job Manually",
                        ToolTip = "Runs the recurring background job manually.",
                        Image = FontAwesomeIcons.PlayCircle,
                        Action = async (source, eventServices, selectedRecord) =>
                        {
                            if (selectedRecord == null)
                                throw new Exception(eventServices.Localizer["No recurring background job is selected"]);

                            var backgroundJobName = selectedRecord is RecurringBackgroundJobEntry backgroundJobEntry ? backgroundJobEntry.Name : (string?)((object[])selectedRecord)[0] ?? String.Empty;
                            var jobQueue = eventServices.ServiceProvider.GetRequiredService<Services.RecurringBackgroundJobQueue>();
                            await jobQueue.ExecuteBackgroundJobManuallyAsync(backgroundJobName);

                            var messageHandler = eventServices.ServiceProvider.GetRequiredService<IMessageHandler>();
                            messageHandler.ShowMessage(eventServices.Localizer["Background job \"{0}\" executed", backgroundJobName],
                                eventServices.Localizer["The background job \"{0}\" ran successfully.", backgroundJobName]);

                            if (selectedRecord is RecurringBackgroundJobEntry job)
                                job.ReloadEntityFromDatabase();
                        }
                    }
                ]
            }
        ]);
    }
}
