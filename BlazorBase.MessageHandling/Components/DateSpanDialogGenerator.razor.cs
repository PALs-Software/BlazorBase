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
using static BlazorBase.MessageHandling.Models.ShowDateSpanDialogArgs;

namespace BlazorBase.MessageHandling.Components
{
    public partial class DateSpanDialogGenerator
    {
        #region Properties

        protected ConcurrentDictionary<Guid, ModalInfo> ModalInfos { get; set; } = new ConcurrentDictionary<Guid, ModalInfo>();
        #endregion

        #region Injects
        [Inject] protected ErrorHandler ErrorHandler { get; set; }
        [Inject] protected IMessageHandler MessageHandler { get; set; }
        [Inject] protected IStringLocalizer<MessageGenerator> MessageGeneratorLocalizer { get; set; }
        [Inject] protected IStringLocalizer<DateSpanDialogGenerator> Localizer { get; set; }
        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                MessageHandler.OnShowDateSpanDialog += MessageHandler_OnShowDateSpanDialog;
            });
        }

        private void MessageHandler_OnShowDateSpanDialog(ShowDateSpanDialogArgs args)
        {
            ShowDateSpanDialog(args);
        }
        #endregion

        #region Methods
        public void ShowDateSpanDialog(
            string title,
            string message,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            DateInputMode dateInputMode = DateInputMode.Date,
            string fromDateCaption = null,
            string toDateCaption = null,
            MessageType messageType = MessageType.Information,
            Func<ModalClosingEventArgs, ConfirmDialogResult, DateSpanDialogResult, Task> onClosing = null,
            object icon = null,
            string confirmButtonText = null,
            Color confirmButtonColor = Color.Primary,
            string abortButtonText = null,
            Color abortButtonColor = Color.Secondary,
            ModalSize modalSize = ModalSize.Large)
        {
            ShowDateSpanDialog(
                new ShowDateSpanDialogArgs(
                    title,
                    message,
                    fromDate,
                    toDate,
                    dateInputMode,
                    fromDateCaption,
                    toDateCaption,
                    messageType,
                    onClosing,
                    icon,
                    confirmButtonText,
                    confirmButtonColor,
                    abortButtonText,
                    abortButtonColor,
                    modalSize
                )
            );
        }

        public void ShowDateSpanDialog(ShowDateSpanDialogArgs args)
        {
            if (args.IsHandled)
                return;

            InvokeAsync(() =>
            {
                args.IsHandled = true;

                if (args.ConfirmButtonText == null)
                    args.ConfirmButtonText = MessageGeneratorLocalizer["Confirm"];
                if (args.CloseButtonText == null)
                    args.CloseButtonText = MessageGeneratorLocalizer["Abort"];
                if (args.FromDateCaption == null)
                    args.FromDateCaption = Localizer["From:"];
                if (args.ToDateCaption == null)
                    args.ToDateCaption = Localizer["To:"];

                if (args.Icon == null)
                    args.SetIconByMessageType();

                while (!ModalInfos.TryAdd(Guid.NewGuid(), new ModalInfo(args))) ;

                StateHasChanged();
            });
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
                var dateSpanArgs = modalInfo.Args as ShowDateSpanDialogArgs;

                await (
                    dateSpanArgs.OnClosing?.Invoke(
                        args,
                        modalInfo.ConfirmDialogResult ?? ConfirmDialogResult.Aborted,
                        new DateSpanDialogResult(dateSpanArgs.FromDate, dateSpanArgs.ToDate)
                    ) ?? Task.CompletedTask
                );
            }
            catch (Exception e)
            {
                _ = Task.Run(() =>
                {
                    MessageHandler.ShowMessage(MessageGeneratorLocalizer["Error"], ErrorHandler.PrepareExceptionErrorMessage(e), MessageType.Error);
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
