﻿using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.Files.Models;
using BlazorBase.MessageHandling.Interfaces;
using Blazorise.RichTextEdit;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Http;
using BlazorBase.RichTextEditor.Models;
using System.Net;
using BlazorBase.Files.Services;

namespace BlazorBase.RichTextEditor.Components;

public partial class BaseRichTextEditor
{
    #region Parameters
    [Parameter] public RichTextEditTheme Theme { get; set; } = RichTextEditTheme.Snow;
    [Parameter] public bool ReadOnly { get; set; } = false;
    [Parameter] public bool SubmitOnEnter { get; set; } = false;
    [Parameter] public Blazorise.Placement ToolbarPosition { get; set; } = Blazorise.Placement.Top;
    [Parameter] public IBaseDbContext DbContext { get; set; } = null!;
    [Parameter] public IBaseModel ConnectedModel { get; set; } = null!;
    [Parameter] public string? BaseImageFileNames { get; set; } = String.Empty;
    [Parameter] public bool HideSaveButton { get; set; } = false;

    [Parameter] public RenderFragment? AdditionalButtons { get; set; }
    [Parameter] public RenderFragment? EditorContent { get; set; }

    [Parameter] public string? BackgroundColor { get; set; }
    [Parameter] public string? Class { get; set; }

    #region Events
    public record OnSaveArgs(IBaseModel ConnectedModel, string Content) { public object? AdditionalInformations { get; set; } }
    [Parameter] public EventCallback<OnSaveArgs> OnSave { get; set; }

    public record OnBeforeSaveArgs(IBaseModel ConnectedModel) { public object? AdditionalInformations { get; set; } }
    [Parameter] public EventCallback<OnBeforeSaveArgs> OnBeforeSave { get; set; }

    public record OnBeforeModifyHtmlContentArgs(IBaseModel ConnectedModel, HtmlDocument HtmlDocument, bool IsHandled) { public object? AdditionalInformations { get; set; } }
    [Parameter] public EventCallback<OnBeforeModifyHtmlContentArgs> OnBeforeModifyHtmlContent { get; set; }

    public record OnAfterModifyHtmlContentArgs(IBaseModel ConnectedModel, HtmlDocument HtmlDocument) { public object? AdditionalInformations { get; set; } }
    [Parameter] public EventCallback<OnAfterModifyHtmlContentArgs> OnAfterModifyHtmlContent { get; set; }

    public record OnContentChangedArgs(IBaseModel ConnectedModel);
    [Parameter] public EventCallback<OnContentChangedArgs> OnContentChanged { get; set; }
    #endregion
    #endregion

    #region Injects
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;
    [Inject] protected IServiceProvider ServiceProvider { get; set; } = null!;
    [Inject] protected IStringLocalizer<BaseRichTextEditor> Localizer { get; set; } = null!;
    [Inject] protected IMessageHandler MessageHandler { get; set; } = null!;
    [Inject] protected IBlazorBaseRichTextEditorOptions Options { get; set; } = null!;
    [Inject] protected IBlazorBaseFileOptions FileOptions { get; set; } = null!;
    [Inject] protected IImageService ImageService { get; set; } = null!;
    [Inject] protected IBaseFileService FileService { get; set; } = null!;
    #endregion

    #region Properties
    protected BaseRichTextEdit? RichTextEdit { get; set; }
    #endregion

    #region Member
    protected string RootClass = String.Empty;
    protected string Style = String.Empty;
    #endregion

    #region Init
    protected override Task OnInitializedAsync()
    {
        if (ReadOnly)
            RootClass = "readonly";

        if (!String.IsNullOrEmpty(BackgroundColor))
            Style = $"background-color: {BackgroundColor}";

        return base.OnInitializedAsync();
    }
    #endregion

    #region Public Methods
    public async Task<string?> GetContentAsync(bool withoutPostProcessing = false)
    {
        if (RichTextEdit == null)
            return null;

        var contentAsHtml = await RichTextEdit.GetHtmlDirectlyAsync();

        if (withoutPostProcessing)
            return contentAsHtml;

        var content = await ModifyHtmlContentAsync(contentAsHtml);

        return content;
    }
    #endregion

    #region Actions
    protected async Task SaveAsync()
    {
        if (RichTextEdit == null)
            return;

        var args = new OnBeforeSaveArgs(ConnectedModel);
        await OnBeforeSave.InvokeAsync(args);

        var contentAsHtml = await RichTextEdit.GetHtmlDirectlyAsync();

        var content = await ModifyHtmlContentAsync(contentAsHtml, args);

        await OnSave.InvokeAsync(new OnSaveArgs(ConnectedModel, content) { AdditionalInformations = args.AdditionalInformations });
    }

    protected async Task OnContentChangedAsync()
    {
        await OnContentChanged.InvokeAsync(new OnContentChangedArgs(ConnectedModel));
    }
    #endregion

    #region HtmlRework
    protected async Task<string> ModifyHtmlContentAsync(string contentAsHtml, OnBeforeSaveArgs? args = null)
    {
        var document = new HtmlDocument();
        document.LoadHtml(contentAsHtml);

        var onBeforeModifyHtmlContentArgs = new OnBeforeModifyHtmlContentArgs(ConnectedModel, document, false) { AdditionalInformations = args?.AdditionalInformations };
        await OnBeforeModifyHtmlContent.InvokeAsync(onBeforeModifyHtmlContentArgs);
        if (onBeforeModifyHtmlContentArgs.IsHandled)
            return document.DocumentNode.OuterHtml;

        if (Options.ChangeImagesInEditorToLocalFiles)
            await ChangeImagesToLocalFilesAsync(document);

        var onAfterModifyHtmlContentArgs = new OnAfterModifyHtmlContentArgs(ConnectedModel, document) { AdditionalInformations = args?.AdditionalInformations };
        await OnAfterModifyHtmlContent.InvokeAsync();

        if (args != null)
            args.AdditionalInformations = onAfterModifyHtmlContentArgs.AdditionalInformations;

        return document.DocumentNode.OuterHtml;
    }

    protected async Task ChangeImagesToLocalFilesAsync(HtmlDocument document)
    {
        var eventServices = GetEventServices();
        var images = document.DocumentNode.Descendants("img");

        var internalImageCount = images.Count(htmlNode =>
        {
            string src = htmlNode.GetAttributeValue("src", String.Empty);
            return src.StartsWith(NavigationManager.BaseUri) || src.StartsWith($"/{FileOptions.ControllerRoute}/GetFile");
        });

        var imageFileName = $"{BaseImageFileNames ?? String.Empty}";
        if (ConnectedModel != null)
            imageFileName += $"{ConnectedModel.GetUnproxiedType().Name}_{String.Join("_", ConnectedModel.GetPrimaryKeys() ?? [])}";

        ChangeInternalImagesWithBaseUrlToLocalFiles(images);
        internalImageCount = await ChangeBase64ImagesToLocalFilesAsync(images, eventServices, imageFileName, internalImageCount);
        internalImageCount = await ChangeExternalImagesToLocalFilesAsync(images, eventServices, imageFileName, internalImageCount);
    }

    protected virtual async Task<int> ChangeBase64ImagesToLocalFilesAsync(IEnumerable<HtmlNode> images, EventServices eventServices, string imageFileName, int internalImageCount)
    {
        var base64DataImages = images.Where(htmlNode =>
        {
            string src = htmlNode.GetAttributeValue("src", String.Empty);
            return !String.IsNullOrWhiteSpace(src) && src.StartsWith("data:image");
        }).ToList();

        foreach (var image in base64DataImages)
        {
            string srcValue = image.GetAttributeValue("src", String.Empty);
            var mimeType = srcValue[5..srcValue.IndexOf(';')];
            var baseType = $".{mimeType.Split("/")[1]}";
            var base64ImageData = srcValue[(srcValue.IndexOf(',') + 1)..];
            byte[] imageData = Convert.FromBase64String(base64ImageData);

            internalImageCount++;
            var imageName = $"{imageFileName}_{internalImageCount:000000}";

            if (Options.ResizeBigImagesToMaxImageSize)
                imageData = await ImageService.ResizeImageToMaxSizeAsync(imageData, Options.MaxImageSize);

            var file = await FileService.CreateFileAsync(eventServices, imageName, baseType, mimeType, imageData);
            await DbContext.AddAsync(file);

            image.SetAttributeValue("src", file.GetFileLink(ignoreTemporaryLink: true) ?? String.Empty);
        }

        return internalImageCount;
    }

    protected virtual async Task<int> ChangeExternalImagesToLocalFilesAsync(IEnumerable<HtmlNode> images, EventServices eventServices, string imageFileName, int internalImageCount)
    {
        var externalImages = images.Where(htmlNode =>
        {
            string src = htmlNode.GetAttributeValue("src", String.Empty);
            return !String.IsNullOrWhiteSpace(src) && !src.StartsWith("data:image") && !src.StartsWith("/api/BaseFile/GetFile") && !src.StartsWith(NavigationManager.BaseUri);
        }).ToList();

        foreach (var image in externalImages)
        {
            string srcValue = image.GetAttributeValue("src", String.Empty);
            srcValue = WebUtility.HtmlDecode(srcValue);
            byte[]? imageData = null;
            try
            {
                using HttpClient client = CreateHttpClientForImageDownload();
                imageData = await client.GetByteArrayAsync(srcValue);
            }
            catch (Exception)
            {
                MessageHandler.ShowMessage(Localizer["Error by saving the editor"], Localizer["The external image {0} could not be downloaded and converted to an internal file. Please check the image. For now the image conversion is skipped for this image.", srcValue], MessageHandling.Enum.MessageType.Error);
                continue;
            }

            string? mimeType = null;
            try
            {
                using var memoryStream = new MemoryStream(imageData);
#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
                var imageInformations = Image.FromStream(memoryStream);
                mimeType = $"image/{imageInformations.RawFormat.ToString().ToLower()}";
                imageInformations.Dispose();
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
            }
            catch (Exception)
            {
                mimeType = "image/png";
            }

            var baseType = $".{mimeType.Split("/")[1]}";

            internalImageCount++;
            var imageName = $"{imageFileName}_{internalImageCount:D6}";
            if (Options.ResizeBigImagesToMaxImageSize)
                imageData = await ImageService.ResizeImageToMaxSizeAsync(imageData, Options.MaxImageSize);

            var file = await FileService.CreateFileAsync(eventServices, imageName, baseType, mimeType, imageData);
            await DbContext.AddAsync(file);

            image.SetAttributeValue("src", file.GetFileLink(ignoreTemporaryLink: true) ?? String.Empty);
        }

        return internalImageCount;
    }

    protected void ChangeInternalImagesWithBaseUrlToLocalFiles(IEnumerable<HtmlNode> images)
    {
        var internalImagesWithBaseUrl = images.Where(htmlNode =>
        {
            string src = htmlNode.GetAttributeValue("src", String.Empty);
            return src.StartsWith(NavigationManager.BaseUri);
        }).ToList();

        foreach (var image in internalImagesWithBaseUrl)
        {
            string srcValue = image.GetAttributeValue("src", String.Empty);
            srcValue = $"/{srcValue.Replace(NavigationManager.BaseUri, String.Empty)}";
            image.SetAttributeValue("src", srcValue);
        }
    }

    protected virtual HttpClient CreateHttpClientForImageDownload()
    {
        return new HttpClient();
    }
    #endregion

    #region MISC
    protected EventServices GetEventServices()
    {
        return new EventServices(ServiceProvider, DbContext, Localizer);
    }
    #endregion
}
