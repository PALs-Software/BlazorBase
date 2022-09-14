using BlazorBase.Mailing.Models;
using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.Models;
using BlazorBase.Modules;
using BlazorBase.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Net.Mail;
using System.Runtime.Versioning;

namespace BlazorBase.Mailing.Services;

[SupportedOSPlatform("windows")]
public class BaseMailService<TTemplateLocalizer> : BaseMailService
{
    #region Injects
    protected readonly IStringLocalizer<TTemplateLocalizer> TemplateLocalizer;
    #endregion

    public BaseMailService(BlazorBaseMailingOptions options, IMessageHandler messageHandler, BaseErrorHandler errorHandler,
                    IStringLocalizer<BaseMailService> localizer, IHttpContextAccessor httpContextAccessor,
                    IStringLocalizer<TTemplateLocalizer> templateLocalizer) : base(options, messageHandler, errorHandler, localizer, httpContextAccessor)
    {
        TemplateLocalizer = templateLocalizer;
    }

    /// <summary>
    /// Send an email asynchronously to the receivers with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receivers">The mail addresses of the receivers</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public Task<bool> SendMailAsync(List<string> receivers, Enum mailTemplate, object[] subjectArguments, object[] bodyArguments, params string[] attachmentPathes)
    {
        return SendMailAsync(receivers, mailTemplate.ToString(), subjectArguments, bodyArguments, attachmentPathes);
    }

    /// <summary>
    /// Send an email asynchronously to the receiver with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receiver">The mail address of the receiver</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public Task<bool> SendMailAsync(string receiver, Enum mailTemplate, object[] subjectArguments, object[] bodyArguments, params string[] attachmentPathes)
    {
        return SendMailAsync(receiver, mailTemplate.ToString(), subjectArguments, bodyArguments, attachmentPathes);
    }

    /// <summary>
    /// Send an email asynchronously to the receiver with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receiver">The mail address of the receiver</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public Task<bool> SendMailAsync(string receiver, string mailTemplate, object[] subjectArguments, object[] bodyArguments, params string[] attachmentPathes)
    {
        return SendMailAsync(new List<string>() { receiver }, mailTemplate.ToString(), subjectArguments, bodyArguments, attachmentPathes);
    }

    /// <summary>
    /// Send an email asynchronously to the receivers with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receivers">The mail addresses of the receivers</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public Task<bool> SendMailAsync(List<string> receivers, string mailTemplate, object[] subjectArguments, object[] bodyArguments, params string[] attachmentPathes)
    {
        var defaultSubjectArguments = new object[] { MailingOptions.WebsiteName };
        var defaultBodyArguments = new object[] { MailingOptions.WebsiteName };
        subjectArguments = defaultSubjectArguments.Concat(subjectArguments).ToArray();
        bodyArguments = defaultBodyArguments.Concat(bodyArguments).ToArray();

        var subject = TemplateLocalizer[$"{mailTemplate}_Subject", subjectArguments].ToString();
        var body = TemplateLocalizer[$"{mailTemplate}_Body", bodyArguments].ToString();
        return SendMailAsync(receivers, subject, body, attachmentPathes);
    }
}

[SupportedOSPlatform("windows")]
public class BaseMailService
{
    #region Injects
    protected readonly IBlazorBaseMailingOptions MailingOptions;
    protected readonly IMessageHandler MessageHandler;
    protected readonly BaseErrorHandler ErrorHandler;
    protected readonly IStringLocalizer<BaseMailService> Localizer;
    protected readonly IHttpContextAccessor HttpContextAccessor;
    #endregion

    public BaseMailService(IBlazorBaseMailingOptions options, IMessageHandler messageHandler, BaseErrorHandler errorHandler,
                       IStringLocalizer<BaseMailService> localizer, IHttpContextAccessor httpContextAccessor)
    {
        MailingOptions = options;
        MessageHandler = messageHandler;
        ErrorHandler = errorHandler;
        Localizer = localizer;
        HttpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Send an email asynchronously to the receiver
    /// </summary>
    /// <param name="receiver">The mail address of the receiver</param>
    /// <param name="subject">The subject text of the mail</param>
    /// <param name="body">The body text of the mail</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public Task<bool> SendMailAsync(string receiver, string subject, string body, params string[] attachmentPathes)
    {
        return SendMailAsync(new List<string>() { receiver }, subject, body, attachmentPathes);
    }

    /// <summary>
    /// Send an email asynchronously to the receivers
    /// </summary>
    /// <param name="receivers">The mail addresses of the receivers</param>
    /// <param name="subject">The subject text of the mail</param>
    /// <param name="body">The body text of the mail</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public async Task<bool> SendMailAsync(List<string> receivers, string subject, string body, params string[] attachmentPathes)
    {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        var client = new SmtpClient()
        {
            Host = MailingOptions.Server,
            UseDefaultCredentials = MailingOptions.UseDefaultCredentials,
            Port = MailingOptions.Port,
            EnableSsl = MailingOptions.EnableSSL,
        };
        if (!MailingOptions.UseDefaultCredentials)
            client.Credentials = new NetworkCredential(MailingOptions.Host, MailingOptions.HostPassword, MailingOptions.Domain);

        var mailMessage = new MailMessage
        {
            From = new MailAddress(MailingOptions.SenderAddress),
            Body = body,
            Subject = subject,
            IsBodyHtml = true
        };

        foreach (var item in attachmentPathes)
            mailMessage.Attachments.Add(new Attachment(item));

        foreach (var item in receivers)
            if (!String.IsNullOrEmpty(item))
                mailMessage.To.Add(item);

        try
        {
            await client.SendMailAsync(mailMessage);
        }
        catch (Exception e)
        {
            var errorMessage = ErrorHandler.PrepareExceptionErrorMessage(e);
            MessageHandler.ShowMessage(Localizer["Error sending email to {0} with subject {1}", String.Join(";", receivers), subject], errorMessage, MessageType.Error);
            return false;
        }

        return true;
    }
}
