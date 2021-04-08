using BlazorBase.MessageHandling.Components;
using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlazorBase.MessageHandling.Services.MessageHandler;

namespace BlazorBase.MessageHandling.Interfaces
{
    public interface IMessageHandler
    {
        public event ShowMessageEventHandler OnShowMessage;
        public void ShowMessage(string title, string message, MessageType messageType = MessageType.Information);
        public void ShowMessage(ShowMessageArgs args);
    }
}
