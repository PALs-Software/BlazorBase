using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.MessageHandling.Models;
using Blazorise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Services
{
    public class MessageHandler : IMessageHandler
    {
        #region Events
        public delegate void ShowMessageEventHandler(ShowMessageArgs args);
        public event ShowMessageEventHandler OnShowMessage;

        public delegate void ShowConfirmDialogHandler(ShowConfirmDialogArgs args);
        public event ShowConfirmDialogHandler OnShowConfirmDialog;
        #endregion

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
            ShowConfirmDialog(new ShowConfirmDialogArgs(title, message, messageType, onClosing, icon, confirmButtonText, confirmButtonColor, abortButtonText, abortButtonTextColor, modalSize));
        }

        public void ShowConfirmDialog(ShowConfirmDialogArgs args)
        {
            OnShowConfirmDialog?.Invoke(args);
        }

    }
}
