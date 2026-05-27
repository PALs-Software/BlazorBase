using BlazorBase.Mailing.Enums;
using BlazorBase.Mailing.Models;
using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;

namespace BlazorBase.Mailing.Services;

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
    /// Send an email asynchronously to the receivers with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receivers">The mail addresses of the receivers</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="priority">The priority of the email</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public Task<bool> SendMailAsync(List<string> receivers, Enum mailTemplate, object[] subjectArguments, object[] bodyArguments, MailPriority priority, params string[] attachmentPathes)
    {
        return SendMailAsync(receivers, mailTemplate.ToString(), subjectArguments, bodyArguments, priority, attachmentPathes);
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
    /// Send an email to the receivers with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receivers">The mail addresses of the receivers</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="priority">The priority of the email</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public bool SendMail(List<string> receivers, Enum mailTemplate, object[] subjectArguments, object[] bodyArguments, MailPriority priority, params string[] attachmentPathes)
    {
        return SendMail(receivers, mailTemplate.ToString(), subjectArguments, bodyArguments, priority, attachmentPathes);
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
    /// <param name="priority">The priority of the email</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public Task<bool> SendMailAsync(string receiver, Enum mailTemplate, object[] subjectArguments, object[] bodyArguments, MailPriority priority, params string[] attachmentPathes)
    {
        return SendMailAsync(receiver, mailTemplate.ToString(), subjectArguments, bodyArguments, priority, attachmentPathes);
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
    /// Send an email to the receiver with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receiver">The mail address of the receiver</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="priority">The priority of the email</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public bool SendMail(string receiver, Enum mailTemplate, object[] subjectArguments, object[] bodyArguments, MailPriority priority, params string[] attachmentPathes)
    {
        return SendMail(receiver, mailTemplate.ToString(), subjectArguments, bodyArguments, priority, attachmentPathes);
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
    /// Send an email asynchronously to the receiver with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receiver">The mail address of the receiver</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="priority">The priority of the email</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public Task<bool> SendMailAsync(string receiver, string mailTemplate, object[] subjectArguments, object[] bodyArguments, MailPriority priority, params string[] attachmentPathes)
    {
        return SendMailAsync(new List<string>() { receiver }, mailTemplate.ToString(), subjectArguments, bodyArguments, priority, attachmentPathes);
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
    /// Send an email to the receiver with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receiver">The mail address of the receiver</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="priority">The priority of the email</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public bool SendMail(string receiver, string mailTemplate, object[] subjectArguments, object[] bodyArguments, MailPriority priority, params string[] attachmentPathes)
    {
        return SendMail(new List<string>() { receiver }, mailTemplate.ToString(), subjectArguments, bodyArguments, priority, attachmentPathes);
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
    /// Send an email asynchronously to the receivers with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receivers">The mail addresses of the receivers</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="priority">The priority of the email</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public Task<bool> SendMailAsync(List<string> receivers, string mailTemplate, object[] subjectArguments, object[] bodyArguments, MailPriority priority, params string[] attachmentPathes)
    {
        var defaultSubjectArguments = new object[] { MailingOptions.WebsiteName };
        var defaultBodyArguments = new object[] { MailingOptions.WebsiteName };
        subjectArguments = defaultSubjectArguments.Concat(subjectArguments).ToArray();
        bodyArguments = defaultBodyArguments.Concat(bodyArguments).ToArray();

        var subject = TemplateLocalizer[$"{mailTemplate}_Subject", subjectArguments].ToString();
        var body = TemplateLocalizer[$"{mailTemplate}_Body", bodyArguments].ToString();
        return SendMailAsync(receivers, subject, body, priority, attachmentPathes);
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

    /// <summary>
    /// Send an email to the receivers with a specific template and arguments. Note that the first argument is always the website name as defined in the BlazorBaseOptions class <see cref="BlazorBase.Models.BlazorBaseOptions"/>
    /// </summary>
    /// <param name="receivers">The mail addresses of the receivers</param>
    /// <param name="mailTemplate">The used mail template. The template localizer will look after the keys "{mailTemplate}_Subject" and "{mailTemplate}_Body"</param>
    /// <param name="subjectArguments">The arguments for the localizer to replace in the subject template</param>
    /// <param name="bodyArguments">The arguments for the localizter to replace in the body template</param>
    /// <param name="priority">The priority of the email</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public bool SendMail(List<string> receivers, string mailTemplate, object[] subjectArguments, object[] bodyArguments, MailPriority priority, params string[] attachmentPathes)
    {
        var defaultSubjectArguments = new object[] { MailingOptions.WebsiteName };
        var defaultBodyArguments = new object[] { MailingOptions.WebsiteName };
        subjectArguments = defaultSubjectArguments.Concat(subjectArguments).ToArray();
        bodyArguments = defaultBodyArguments.Concat(bodyArguments).ToArray();

        var subject = TemplateLocalizer[$"{mailTemplate}_Subject", subjectArguments].ToString();
        var body = TemplateLocalizer[$"{mailTemplate}_Body", bodyArguments].ToString();
        return SendMail(receivers, subject, body, priority, attachmentPathes);
    }
}

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
    /// Send an email asynchronously to the receiver
    /// </summary>
    /// <param name="receiver">The mail address of the receiver</param>
    /// <param name="subject">The subject text of the mail</param>
    /// <param name="body">The body text of the mail</param>
    /// <param name="priority">The priority of the email</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public Task<bool> SendMailAsync(string receiver, string subject, string body, MailPriority priority, params string[] attachmentPathes)
    {
        return SendMailAsync(new List<string>() { receiver }, subject, body, priority, attachmentPathes);
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
    /// Send an email to the receiver
    /// </summary>
    /// <param name="receiver">The mail address of the receiver</param>
    /// <param name="subject">The subject text of the mail</param>
    /// <param name="body">The body text of the mail</param>
    /// <param name="priority">The priority of the email</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public bool SendMail(string receiver, string subject, string body, MailPriority priority, params string[] attachmentPathes)
    {
        return SendMail(new List<string>() { receiver }, subject, body, priority, attachmentPathes);
    }

    /// <summary>
    /// Send an email asynchronously to the receivers
    /// </summary>
    /// <param name="receivers">The mail addresses of the receivers</param>
    /// <param name="subject">The subject text of the mail</param>
    /// <param name="body">The body text of the mail</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public Task<bool> SendMailAsync(List<string> receivers, string subject, string body, params string[] attachmentPathes)
    {
        return SendMailAsync(receivers, subject, body, MailPriority.Normal, attachmentPathes);
    }

    /// <summary>
    /// Send an email asynchronously to the receivers
    /// </summary>
    /// <param name="receivers">The mail addresses of the receivers</param>
    /// <param name="subject">The subject text of the mail</param>
    /// <param name="body">The body text of the mail</param>
    /// <param name="priority">The priority of the email</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public async Task<bool> SendMailAsync(List<string> receivers, string subject, string body, MailPriority priority, params string[] attachmentPathes)
    {
        LastErrorMessage = null;

        try
        {
            using var client = new SmtpClient();
            ConfigureClientCertificateValidation(client);

            var message = BuildMessage(receivers, subject, body, priority, attachmentPathes);
            var secureSocketOptions = MapEncryption(MailingOptions.Encryption);

            await client.ConnectAsync(MailingOptions.Server, MailingOptions.Port, secureSocketOptions);
            await AuthenticateAsync(client);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
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
        return SendMail(receivers, subject, body, MailPriority.Normal, attachmentPathes);
    }

    /// <summary>
    /// Send an email to the receivers
    /// </summary>
    /// <param name="receivers">The mail addresses of the receivers</param>
    /// <param name="subject">The subject text of the mail</param>
    /// <param name="body">The body text of the mail</param>
    /// <param name="priority">The priority of the email</param>
    /// <param name="attachmentPathes">File paths that are appended and sent with the email</param>
    /// <returns>The task object representing the asynchronous operation</returns>
    public bool SendMail(List<string> receivers, string subject, string body, MailPriority priority, params string[] attachmentPathes)
    {
        LastErrorMessage = null;

        try
        {
            using var client = new SmtpClient();
            ConfigureClientCertificateValidation(client);

            var message = BuildMessage(receivers, subject, body, priority, attachmentPathes);
            var secureSocketOptions = MapEncryption(MailingOptions.Encryption);

            client.Connect(MailingOptions.Server, MailingOptions.Port, secureSocketOptions);
            Authenticate(client);
            client.Send(message);
            client.Disconnect(true);
        }
        catch (Exception e)
        {
            HandleMailError(receivers, subject, e);
            return false;
        }

        return true;
    }

    protected virtual MimeMessage BuildMessage(List<string> receivers, string subject, string body, MailPriority priority, params string[] attachmentPathes)
    {
        var message = new MimeMessage
        {
            Subject = subject,
            Priority = MapPriority(priority)
        };

        message.From.Add(MailboxAddress.Parse(MailingOptions.SenderAddress));

        foreach (var item in receivers)
            if (!String.IsNullOrEmpty(item))
                message.To.Add(MailboxAddress.Parse(item));

        var bodyBuilder = new BodyBuilder { HtmlBody = body };

        foreach (var path in attachmentPathes)
            bodyBuilder.Attachments.Add(path);

        message.Body = bodyBuilder.ToMessageBody();
        return message;
    }

    protected virtual Task AuthenticateAsync(SmtpClient client)
    {
        if (MailingOptions.UseDefaultCredentials)
            return Task.CompletedTask;

        var credentials = BuildCredentials();

        if (!String.IsNullOrEmpty(credentials.Domain))
            return client.AuthenticateAsync(new SaslMechanismNtlm(credentials));

        return client.AuthenticateAsync(credentials.UserName, credentials.Password);
    }

    protected virtual void Authenticate(SmtpClient client)
    {
        if (MailingOptions.UseDefaultCredentials)
            return;

        var credentials = BuildCredentials();

        if (!String.IsNullOrEmpty(credentials.Domain))
        {
            client.Authenticate(new SaslMechanismNtlm(credentials));
            return;
        }

        client.Authenticate(credentials.UserName, credentials.Password);
    }

    protected virtual NetworkCredential BuildCredentials()
    {
        var password = SecureStringToPlainText(MailingOptions.HostPassword);
        return new NetworkCredential(MailingOptions.Host, password, MailingOptions.Domain ?? String.Empty);
    }

    protected virtual void ConfigureClientCertificateValidation(SmtpClient client)
    {
#if DEBUG
        client.ServerCertificateValidationCallback = (_, _, _, _) => true;
#endif
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

    private static MessagePriority MapPriority(MailPriority priority) => priority switch
    {
        MailPriority.Low => MessagePriority.NonUrgent,
        MailPriority.High => MessagePriority.Urgent,
        _ => MessagePriority.Normal
    };

    private static SecureSocketOptions MapEncryption(MailEncryption encryption) => encryption switch
    {
        MailEncryption.StartTls => SecureSocketOptions.StartTls,
        MailEncryption.Ssl => SecureSocketOptions.SslOnConnect,
        _ => SecureSocketOptions.None
    };

    private static string SecureStringToPlainText(SecureString secureString)
    {
        if (secureString == null || secureString.Length == 0)
            return String.Empty;

        var pointer = Marshal.SecureStringToGlobalAllocUnicode(secureString);

        try
        {
            return Marshal.PtrToStringUni(pointer) ?? String.Empty;
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(pointer);
        }
    }
}
