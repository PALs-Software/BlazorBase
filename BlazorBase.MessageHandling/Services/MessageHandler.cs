using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.MessageHandling.Models;
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
        #endregion

        public void ShowMessage(string title, string message, MessageType messageType = MessageType.Information)
        {
            ShowMessage(new ShowMessageArgs(title, message, messageType, false));
        }

        public void ShowMessage(ShowMessageArgs args)
        {
            OnShowMessage?.Invoke(args);
        }

    }
}
