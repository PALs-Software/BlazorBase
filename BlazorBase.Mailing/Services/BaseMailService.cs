using BlazorBase.Mailing.Models;
using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Runtime.Versioning;

namespace BlazorBase.Mailing.Services;

[SupportedOSPlatform("windows")]
public class BaseMailService<TTemplateLocalizer>(IBlazorBaseMailingOptions options, IServiceProvider serviceProvider, BaseErrorHandler errorHandler,
                IStringLocalizer<BaseMailService> localizer,
                IStringLocalizer<TTemplateLocalizer> templateLocalizer, ILogger<BaseMailService> logger) : BaseMailService(options, serviceProvider, errorHandler, localizer, logger)
{
    #region Injects
    protected readonly IStringLocalizer<TTemplateLocalizer> TemplateLocalizer = templateLocalizer;

    #endregion

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
    /// Send an email to the receivers with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receivers">The mail addresses of the receivers</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public bool SendMail(List<string> receivers, Enum mailTemplate, object[] subjectArguments, object[] bodyArguments, params string[] attachmentPathes)
    {
        return SendMail(receivers, mailTemplate.ToString(), subjectArguments, bodyArguments, attachmentPathes);
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
    /// Send an email to the receiver with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receiver">The mail address of the receiver</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public bool SendMail(string receiver, Enum mailTemplate, object[] subjectArguments, object[] bodyArguments, params string[] attachmentPathes)
    {
        return SendMail(receiver, mailTemplate.ToString(), subjectArguments, bodyArguments, attachmentPathes);
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
    /// Send an email to the receiver with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receiver">The mail address of the receiver</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public bool SendMail(string receiver, string mailTemplate, object[] subjectArguments, object[] bodyArguments, params string[] attachmentPathes)
    {
        return SendMail(new List<string>() { receiver }, mailTemplate.ToString(), subjectArguments, bodyArguments, attachmentPathes);
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

    /// <summary>
    /// Send an email to the receivers with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receivers">The mail addresses of the receivers</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public bool SendMail(List<string> receivers, string mailTemplate, object[] subjectArguments, object[] bodyArguments, params string[] attachmentPathes)
    {
        var defaultSubjectArguments = new object[] { MailingOptions.WebsiteName };
        var defaultBodyArguments = new object[] { MailingOptions.WebsiteName };
        subjectArguments = defaultSubjectArguments.Concat(subjectArguments).ToArray();
        bodyArguments = defaultBodyArguments.Concat(bodyArguments).ToArray();

        var subject = TemplateLocalizer[$"{mailTemplate}_Subject", subjectArguments].ToString();
        var body = TemplateLocalizer[$"{mailTemplate}_Body", bodyArguments].ToString();
        return SendMail(receivers, subject, body, attachmentPathes);
    }
}

[SupportedOSPlatform("windows")]
public class BaseMailService(IBlazorBaseMailingOptions options, IServiceProvider serviceProvider, BaseErrorHandler errorHandler,
                   IStringLocalizer<BaseMailService> localizer, ILogger<BaseMailService> logger)
{
    #region Injects

    protected readonly IBlazorBaseMailingOptions MailingOptions = options;
    protected readonly IServiceProvider ServiceProvider = serviceProvider;
    protected readonly BaseErrorHandler ErrorHandler = errorHandler;
    protected readonly IStringLocalizer<BaseMailService> Localizer = localizer;
    protected readonly ILogger<BaseMailService> Logger = logger;

    #endregion

    #region Properties

    public bool DisplayErrorMessagesToUser { get; set; } = true;

    public string? LastErrorMessage { get; protected set; } = null;

    #endregion

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
    /// Send an email to the receiver
    /// </summary>
    /// <param name="receiver">The mail address of the receiver</param>
    /// <param name="subject">The subject text of the mail</param>
    /// <param name="body">The body text of the mail</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public bool SendMail(string receiver, string subject, string body, params string[] attachmentPathes)
    {
        return SendMail(new List<string>() { receiver }, subject, body, attachmentPathes);
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
        LastErrorMessage = null;

        try
        {
            var preparedMail = PrepareMail(receivers, subject, body, attachmentPathes);
            await preparedMail.Client.SendMailAsync(preparedMail.MailMessage);
        }
        catch (Exception e)
        {
            HandleMailError(receivers, subject, e);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Send an email to the receivers
    /// </summary>
    /// <param name="receivers">The mail addresses of the receivers</param>
    /// <param name="subject">The subject text of the mail</param>
    /// <param name="body">The body text of the mail</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public bool SendMail(List<string> receivers, string subject, string body, params string[] attachmentPathes)
    {
        LastErrorMessage = null;

        try
        {
            var preparedMail = PrepareMail(receivers, subject, body, attachmentPathes);
            preparedMail.Client.Send(preparedMail.MailMessage);
        }
        catch (Exception e)
        {
            HandleMailError(receivers, subject, e);
            return false;
        }

        return true;
    }

    protected virtual (SmtpClient Client, MailMessage MailMessage) PrepareMail(List<string> receivers, string subject, string body, params string[] attachmentPathes)
    {
#if DEBUG
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
#endif

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

        return (client, mailMessage);
    }

    protected virtual void HandleMailError(List<string> receivers, string subject, Exception e)
    {
        var errorMessage = ErrorHandler.PrepareExceptionErrorMessage(e);
        LastErrorMessage = errorMessage;
        Logger.LogError("Error sending email to {Receivers} with subject {Subject}:\n\n{ErrorMessage}", String.Join(";", receivers), subject, errorMessage);

        if (DisplayErrorMessagesToUser)
        {
            var messageHandler = ServiceProvider.GetRequiredService<IMessageHandler>();
            messageHandler.ShowMessage(Localizer["Error sending email to {0} with subject {1}", String.Join(";", receivers), subject], errorMessage, MessageType.Error);
        }
    }

}
