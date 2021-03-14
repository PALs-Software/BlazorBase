using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BlazorBase.Components
{
    public partial class ConfirmDialog
    {
        [Parameter]
        public string Title { get; set; }

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

        [Inject]
        private IStringLocalizer<ConfirmDialog> Localizer { get; set; }

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
