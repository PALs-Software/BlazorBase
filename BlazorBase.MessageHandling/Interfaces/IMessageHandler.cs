using BlazorBase.MessageHandling.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Interfaces
{
    public interface IMessageHandler
    {
        void ShowMessage(string title, string message, MessageType messageType = MessageType.Information);
    }
}
