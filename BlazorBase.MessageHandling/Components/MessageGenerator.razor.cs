using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.MessageHandling.Models;
using BlazorBase.Modules;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Components
{
    public partial class MessageGenerator
    {
        #region Properties

        protected ConcurrentDictionary<Guid, ModalInfo> ModalInfos { get; set; } = new ConcurrentDictionary<Guid, ModalInfo>();
        #endregion

        #region Injects
        [Inject] protected ErrorHandler ErrorHandler { get; set; }
        [Inject]private IMessageHandler MessageHandler { get; set; }
        [Inject]private IStringLocalizer<MessageGenerator> Localizer { get; set; }
        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                MessageHandler.OnShowMessage += MessageHandler_OnShowMessage;
                MessageHandler.OnShowConfirmDialog += MessageHandler_OnShowConfirmDialog;
            });
        }
        #endregion

        #region Message Handler
        protected void MessageHandler_OnShowMessage(ShowMessageArgs args)
        {
            ShowMessage(args);
        }

        private void MessageHandler_OnShowConfirmDialog(ShowConfirmDialogArgs args)
        {
            ShowConfirmDialog(args);
        }
        #endregion

        #region Methods

        public void ShowMessage(ShowMessageArgs args)
        {
            if (args.IsHandled)
                return;

            InvokeAsync(() =>
            {
                args.IsHandled = true;

                if (args is ShowConfirmDialogArgs confirmDialogArgs)
                {
                    if (confirmDialogArgs.ConfirmButtonText == null)
                        confirmDialogArgs.ConfirmButtonText = Localizer["Confirm"];

                    if (args.CloseButtonText == null)
                        args.CloseButtonText = Localizer["Abort"];
                }

                if (args.CloseButtonText == null)
                    args.CloseButtonText = Localizer["Ok"];

                if (args.Icon == null)
                    args.SetIconByMessageType();

                while (!ModalInfos.TryAdd(Guid.NewGuid(), new ModalInfo(args))) ;

                StateHasChanged();
            });
        }

        public void ShowMessage(string title, string message,
                                MessageType messageType = MessageType.Information,
                                Func<ModalClosingEventArgs, Task> onClosing = null,
                                object icon = null,
                                string closeButtonText = null,
                                Color closeButtonColor = Color.Secondary,
                                ModalSize modalSize = ModalSize.Large)
        {
            ShowMessage(new ShowMessageArgs(title, message, messageType, onClosing, icon, closeButtonText, closeButtonColor, modalSize));
        }

        public void ShowConfirmDialog(string title, string message,
                                  MessageType messageType = MessageType.Information,
                                  Func<ModalClosingEventArgs, ConfirmDialogResult, Task> onClosing = null,
                                  object icon = null,
                                  string confirmButtonText = null,
                                  Color confirmButtonColor = Color.Primary,
                                  string abortButtonText = null,
                                  Color abortButtonTextColor = Color.Secondary,
                                  ModalSize modalSize = ModalSize.Large)
        {
            ShowMessage(new ShowConfirmDialogArgs(title, message, messageType, onClosing, icon, confirmButtonText, confirmButtonColor, abortButtonText, abortButtonTextColor, modalSize));
        }
        public void ShowConfirmDialog(ShowConfirmDialogArgs args)
        {
            ShowMessage(args);
        }
        #endregion

        #region Modal
        protected void OnModalClosed(Guid id)
        {
            ModalInfos.TryRemove(id, out ModalInfo _);
        }

        protected async Task OnModalClosing(ModalInfo modalInfo, ModalClosingEventArgs args)
        {
            try
            {
                if (modalInfo.Args is ShowConfirmDialogArgs confirmDialogArgs)
                    await (confirmDialogArgs.OnClosing?.Invoke(args, modalInfo.ConfirmDialogResult ?? ConfirmDialogResult.Aborted) ?? Task.CompletedTask);
                else
                    await (modalInfo.Args.OnClosing?.Invoke(args) ?? Task.CompletedTask);
            }
            catch (Exception e)
            {
                _ = Task.Run(() => {
                    ShowMessage(Localizer["Error"], ErrorHandler.PrepareExceptionErrorMessage(e), MessageType.Error);
                });
            }
        }

        protected void OnConfirmButtonClicked(ModalInfo modalInfo)
        {
            modalInfo.ConfirmDialogResult = ConfirmDialogResult.Confirmed;
            modalInfo.Modal.Hide();
        }

        protected void OnAbortButtonClicked(ModalInfo modalInfo)
        {
            modalInfo.ConfirmDialogResult = ConfirmDialogResult.Aborted;
            modalInfo.Modal.Hide();
        }

        #endregion
    }
}
