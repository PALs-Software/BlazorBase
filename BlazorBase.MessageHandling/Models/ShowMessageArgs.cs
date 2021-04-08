using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Models
{
    public class ShowMessageArgs
    {
        public ShowMessageArgs() { }
        public ShowMessageArgs(string title, string message, MessageType messageType, bool isHandled)
        {
            Title = title;
            Message = message;
            MessageType = messageType;
            IsHandled = isHandled;
        }

        public string Title { get; set; }
        public string Message { get; set; }
        public MessageType MessageType { get; set; }
        public bool IsHandled { get; set; }
    }
}
