using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.Components
{
    public partial class ConfirmDialog
    {
        public enum ConfirmDialogResult
        {
            Confirmed,
            Aborted
        }

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

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public string ConfirmButtonText { get; set; } = "Ok";

        [Parameter]
        public string AbortButtonText { get; set; } = "Abbrechen";

        [Parameter]
        public Color ConfirmButtonColor { get; set; } = Color.Primary;

        [Parameter]
        public Color AbortButtonColor { get; set; } = Color.Secondary;

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public EventCallback<ConfirmDialogEventArgs> OnConfirmDialogClosed { get; set; }

        [Parameter]
        public object Sender { get; set; }

        private Modal Modal = default!;

        public async Task Show()
        {
            await InvokeAsync(() =>
            {
                Modal.Show();
            });
        }

        public async Task Show(object sender)
        {
            Sender = sender;
            await Show();
        }

        protected async Task SaveModal()
        {
            Modal.Hide();
            await OnConfirmDialogClosed.InvokeAsync(new ConfirmDialogEventArgs(ConfirmDialogResult.Confirmed, Sender));
        }

        protected async Task AbortModal()
        {
            Modal.Hide();
            await OnConfirmDialogClosed.InvokeAsync(new ConfirmDialogEventArgs(ConfirmDialogResult.Aborted, Sender));
        }
    }
}
