using BlazorBase.MessageHandling.Components;
using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Models;
using Blazorise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlazorBase.MessageHandling.Models.ShowDateSpanDialogArgs;
using static BlazorBase.MessageHandling.Services.MessageHandler;

namespace BlazorBase.MessageHandling.Interfaces
{
    public interface IMessageHandler
    {
        public delegate void ShowMessageEventHandler(ShowMessageArgs args);
        public event ShowMessageEventHandler OnShowMessage;
        public void ShowMessage(
            string title,
            string message,
            MessageType messageType = MessageType.Information,
            Func<ModalClosingEventArgs, Task> onClosing = null,
            object icon = null,
            string closeButtonText = null,
            Color closeButtonColor = Color.Secondary,
            ModalSize modalSize = ModalSize.Large);
        public void ShowMessage(ShowMessageArgs args);

        public delegate void ShowConfirmDialogHandler(ShowConfirmDialogArgs args);
        public event ShowConfirmDialogHandler OnShowConfirmDialog;
        public void ShowConfirmDialog(
            string title,
            string message,
            MessageType messageType = MessageType.Information,
            Func<ModalClosingEventArgs, ConfirmDialogResult, Task> onClosing = null,
            object icon = null,
            string confirmButtonText = null,
            Color confirmButtonColor = Color.Primary,
            string abortButtonText = null,
            Color abortButtonColor = Color.Secondary,
            ModalSize modalSize = ModalSize.Large);
        public void ShowConfirmDialog(ShowConfirmDialogArgs args);

        public delegate void ShowDateSpanDialogHandler(ShowDateSpanDialogArgs args);
        public event ShowDateSpanDialogHandler OnShowDateSpanDialog;
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
            ModalSize modalSize = ModalSize.Large);
        public void ShowDateSpanDialog(ShowDateSpanDialogArgs args);
    }
}
