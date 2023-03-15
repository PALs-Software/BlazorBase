﻿using BlazorBase.Files.Models;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BlazorBase.Files.Components
{
    public partial class BaseFileModal
    {
        #region Parameters
        [Parameter] public BaseFile BaseFile { get; set; } = null;
        [Parameter] public bool ShowFileButton { get; set; } = true;
        [Parameter] public bool ShowDownloadFileButton { get; set; } = true;
        #endregion

        #region Injects
        [Inject] protected IStringLocalizer<BaseFileModal> Localizer { get; set; }

        #endregion

        #region Members
        protected Modal Modal = default!;
        #endregion

        public void Show()
        {
            Modal?.Show();
        }
    }
}
