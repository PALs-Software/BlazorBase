using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.Components
{
    public class ConfirmDialogEventArgs : EventArgs
    {
        public ConfirmDialogResult ConfirmDialogResult { get; set; }
        public object Sender { get; set; }

        public ConfirmDialogEventArgs(ConfirmDialogResult confirmDialogResult, object sender)
        {
            ConfirmDialogResult = confirmDialogResult;
            Sender = sender;
        }
    }
}
