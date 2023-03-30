using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.MessageHandling.Models;
using BlazorBase.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Components
{
    public partial class LoadingMessageGenerator
    {
        #region Properties
        protected ConcurrentDictionary<Guid, ShowLoadingMessageArgs> LoadingMessages { get; set; } = new ConcurrentDictionary<Guid, ShowLoadingMessageArgs>();
        #endregion

        #region Injects
        [Inject] protected BaseErrorHandler ErrorHandler { get; set; }
        [Inject] protected IMessageHandler MessageHandler { get; set; }
        [Inject] protected IStringLocalizer<MessageGenerator> Localizer { get; set; }
        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                MessageHandler.OnShowLoadingMessage += MessageHandler_OnShowLoadingMessage;
                MessageHandler.OnUpdateLoadingMessage += MessageHandler_OnUpdateLoadingMessage;
                MessageHandler.OnCloseLoadingMessage += MessageHandler_OnCloseLoadingMessage;
                MessageHandler.OnShowLoadingProgressMessage += MessageHandler_OnShowLoadingProgressMessage;
                MessageHandler.OnUpdateLoadingProgressMessage += MessageHandler_OnUpdateLoadingProgressMessage;
                MessageHandler.OnCloseLoadingProgressMessage += MessageHandler_OnCloseLoadingProgressMessage;
            });
        }
        #endregion

        #region Message Handler
        protected Guid MessageHandler_OnShowLoadingMessage(ShowLoadingMessageArgs args)
        {
            return ShowLoadingMessage(args);
        }

        private bool MessageHandler_OnUpdateLoadingMessage(ShowLoadingMessageArgs args)
        {
            return UpdateLoadingMessage(args);
        }

        private bool MessageHandler_OnCloseLoadingMessage(Guid id)
        {
            return CloseLoadingMessage(id);
        }

        private Guid MessageHandler_OnShowLoadingProgressMessage(ShowLoadingProgressMessageArgs args)
        {
            return ShowLoadingMessage(args);
        }
        private bool MessageHandler_OnUpdateLoadingProgressMessage(ShowLoadingProgressMessageArgs args)
        {
            return UpdateLoadingMessage(args);
        }

        private bool MessageHandler_OnCloseLoadingProgressMessage(Guid id)
        {
            return CloseLoadingMessage(id);
        }
        #endregion

        #region Methods

        #region Show
        protected Guid ShowLoadingMessage(ShowLoadingMessageArgs args)
        {
            if (args.IsHandled)
                return args.Id;

            args.IsHandled = true;
            while (!LoadingMessages.TryAdd(args.Id = Guid.NewGuid(), args)) ;

            InvokeAsync(() => { StateHasChanged(); });

            return args.Id;
        }

        public void ShowLoadingMessage(string message,
                                       RenderFragment loadingChildContent = null)
        {
            ShowLoadingMessage(new ShowLoadingMessageArgs(message, loadingChildContent));
        }

        public void ShowLoadingProgressMessage(string message,
                                               int currentProgress = 0,
                                               string progressText = null,
                                               bool showProgressInText = true,
                                               RenderFragment loadingChildContent = null)
        {
            ShowLoadingMessage(new ShowLoadingProgressMessageArgs(message, currentProgress, progressText, showProgressInText, loadingChildContent));
        }
        #endregion

        #region Update
        protected bool UpdateLoadingMessage(ShowLoadingMessageArgs args)
        {
            if (!LoadingMessages.TryGetValue(args.Id, out ShowLoadingMessageArgs? loadingMessage))
                return false;

            loadingMessage.Message = args.Message;
            loadingMessage.LoadingChildContent = args.LoadingChildContent;

            if (loadingMessage is not ShowLoadingProgressMessageArgs loadingProgressArgs || args is not ShowLoadingProgressMessageArgs progressArgs)
                return true;

            loadingProgressArgs.ProgressText = progressArgs.ProgressText;
            loadingProgressArgs.CurrentProgress = progressArgs.CurrentProgress;
            loadingProgressArgs.ShowProgressInText = progressArgs.ShowProgressInText;

            InvokeAsync(() => { StateHasChanged(); });
            return true;
        }

        public void UpdateLoadingMessage(Guid id,
                                         string message,
                                         RenderFragment loadingChildContent = null)
        {
            UpdateLoadingMessage(new ShowLoadingMessageArgs(message, loadingChildContent) { Id = id });
        }

        public void UpdateLoadingProgressMessage(Guid id,
                                                 string message,
                                                 int currentProgress = 0,
                                                 string progressText = null,
                                                 bool showProgressInText = true,
                                                 RenderFragment loadingChildContent = null)
        {
            ShowLoadingMessage(new ShowLoadingProgressMessageArgs(message, currentProgress, progressText, showProgressInText, loadingChildContent) { Id = id });
        }
        #endregion

        #region Close
        public bool CloseLoadingMessage(Guid id)
        {
            var success = LoadingMessages.TryRemove(id, out ShowLoadingMessageArgs _);
            InvokeAsync(() => { StateHasChanged(); });
            return success;
        }

        public void CloseLoadingProgressMessage(Guid id)
        {
            CloseLoadingMessage(id);
        }
        #endregion

        #endregion
    }
}
