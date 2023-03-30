using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using Blazorise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Models
{
    public class ShowDateSpanDialogArgs : ShowConfirmDialogArgs
    {
        public ShowDateSpanDialogArgs() { }
        public ShowDateSpanDialogArgs(
            string title,
            string message,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            DateInputMode dateInputMode = DateInputMode.Date,
            bool useAsSingleDatePicker = false,
            string? fromDateCaption = null,
            string? toDateCaption = null,
            MessageType messageType = MessageType.Information,
            Func<ModalClosingEventArgs, ConfirmDialogResult, DateSpanDialogResult, Task>? onClosing = null,
            object? icon = null,
            string? confirmButtonText = null,
            Color confirmButtonColor = Color.Primary,
            string? abortButtonText = null,
            Color abortButtonColor = Color.Secondary,
            ModalSize modalSize = ModalSize.Large) : base(title, message, messageType, null, icon, confirmButtonText, confirmButtonColor, abortButtonText, abortButtonColor, modalSize)
        {
            OnClosing = onClosing;
            FromDate = fromDate;
            ToDate = toDate;
            DateInputMode = dateInputMode;
            UseAsSingleDatePicker = useAsSingleDatePicker;
            FromDateCaption = fromDateCaption;
            ToDateCaption = toDateCaption;
        }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateInputMode DateInputMode { get; set; }
        public bool UseAsSingleDatePicker { get; set; }
        public string? FromDateCaption { get; set; }
        public string? ToDateCaption { get; set; }

        public record DateSpanDialogResult(DateTime? FromDate, DateTime? ToDate);
        public new Func<ModalClosingEventArgs, ConfirmDialogResult, DateSpanDialogResult, Task> OnClosing { get; set; }
    }
}
