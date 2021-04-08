using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.MessageHandling.Models;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Components
{
    public partial class MessageGenerator
    {
        #region Properties
     
        protected Dictionary<Guid, ModalInfo> ModalInfos { get; set; } = new Dictionary<Guid, ModalInfo>();
        #endregion

        #region Injects
        [Inject]
        private IMessageHandler MessageHandler { get; set; }

        [Inject]
        private IStringLocalizer<MessageGenerator> Localizer { get; set; }
        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                MessageHandler.OnShowMessage += MessageHandler_OnShowMessage;
            });
        }
        #endregion

        #region Message Handler
        protected void MessageHandler_OnShowMessage(ShowMessageArgs args)
        {
            ShowMessage(args);
        }
        #endregion

        #region Methods
        public void ShowMessage(string title, string message, MessageType messageType = MessageType.Information)
        {
            ShowMessage(new ShowMessageArgs(title, message, messageType, false));
        }

        public void ShowMessage(ShowMessageArgs args)
        {
            if (args.IsHandled)
                return;

            InvokeAsync(() =>
            {
                args.IsHandled = true;
                ModalInfos.Add(Guid.NewGuid(), new ModalInfo(args));

                StateHasChanged();
            });
        }


        #endregion

        #region Modal
        protected void OnModalClosed(Guid id)
        {
            ModalInfos.Remove(id);
        }

        #endregion

        #region View Models
        public class ModalInfo
        {
            public ModalInfo(ShowMessageArgs args)
            {
                Args = args;
            }
            public ShowMessageArgs Args { get; set; }
            public Modal Modal { get; set; }
        }
        #endregion
    }


}
