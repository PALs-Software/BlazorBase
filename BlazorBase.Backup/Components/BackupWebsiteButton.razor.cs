using BlazorBase.Backup.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BlazorBase.Backup.Components
{
    public partial class BackupWebsiteButton
    {
        #region Injects

        [Inject] protected BackupWebsiteService BackupWebsiteService { get; set; } = default!;
        [Inject] protected IStringLocalizer<BackupWebsiteButton> Localizer { get; set; } = default!;

        #endregion

        protected virtual Task CreateAndDownloadWebsiteBackupAsync()
        {
            return BackupWebsiteService.CreateAndDownloadWebsiteBackupAsync();
        }
    }
}
