namespace GenericEmailService;

public sealed record EmailConfigurations(
    string Smtp,
    string Password,
    int Port,
    bool SSL = true,
    bool Html = true);

public sealed record EmailModel<TAttachment>(
    EmailConfigurations Configurations,
    string FromEmail,
    List<string> ToEmails,
    string Subject,
    string Body,
    List<TAttachment>? Attachments = null);

//using NetEmailModel = EmailModel<Attachment>;
//using MailKitEmailModel = EmailModel<Stream>;