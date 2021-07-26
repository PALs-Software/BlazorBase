using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.MessageHandling.Models;
using Blazorise;
using System;
using System.Threading.Tasks;
using static BlazorBase.MessageHandling.Interfaces.IMessageHandler;
using static BlazorBase.MessageHandling.Models.ShowDateSpanDialogArgs;

namespace BlazorBase.MessageHandling.Services
{
    public class MessageHandler : IMessageHandler
    {
        #region Events

        public event ShowMessageEventHandler OnShowMessage;

        public event ShowConfirmDialogHandler OnShowConfirmDialog;

        public event ShowDateSpanDialogHandler OnShowDateSpanDialog;
        #endregion

        #region ShowMessage
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

        public void ShowMessage(ShowMessageArgs args)
        {
            OnShowMessage?.Invoke(args);
        }
        #endregion

        #region ShowConfirmDialog
        public void ShowConfirmDialog(string title, string message,
                                MessageType messageType = MessageType.Information,
                                Func<ModalClosingEventArgs, ConfirmDialogResult, Task> onClosing = null,
                                object icon = null,
                                string confirmButtonText = null,
                                Color confirmButtonColor = Color.Primary,
                                string abortButtonText = null,
                                Color abortButtonColor = Color.Secondary,
                                ModalSize modalSize = ModalSize.Large)
        {
            ShowConfirmDialog(new ShowConfirmDialogArgs(title, message, messageType, onClosing, icon, confirmButtonText, confirmButtonColor, abortButtonText, abortButtonColor, modalSize));
        }

        public void ShowConfirmDialog(ShowConfirmDialogArgs args)
        {
            OnShowConfirmDialog?.Invoke(args);
        }
        #endregion

        #region ShowDateSpanDialog
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
            OnShowDateSpanDialog?.Invoke(args);
        }
        #endregion

    }
}
