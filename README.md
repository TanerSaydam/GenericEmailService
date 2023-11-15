# Dependency

This library was created by .Net 8.0

## Install

```bash
dotnet add package GenericEmailService
```

## Class
```CSharp
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
```

## Methods
This library have two methods
- Net
- MailKit


## Configuration
```CSharp
EmailConfigurations configurations = new(
                                        Stmp: "smtp.example.com",
                                        Password: "password",
                                        Port: 587,
                                        SSL: true,
                                        Html: true);
```

## Use Net
```CSharp
EmailModel<Attachment> model = new(
                                  Configurations: configurations,
                                  FromEmail: "mymail@gmail.com",
                                  ToEmails: ["sendmail1@gmail.com","sendmail2@gmail.com"],
                                  Subject: "Subjects",
                                  Body: "<b>Body</b>",
                                  Attachments: Attachments);    
await EmailService.SendEmailWithSmtpClientAsync(model);
```

## Use MailKit
```CSharp
EmailModel<Stream> model = new(
                                  Configurations: configurations,
                                  FromEmail: "mymail@gmail.com",
                                  ToEmails: ["sendmail1@gmail.com","sendmail2@gmail.com"],
                                  Subject: "Subjects",
                                  Body: "<b>Body</b>",
                                  Attachments: Attachments);    
string result = await EmailService.SendEmailWithMailKitAsync(model);
```

## Methods
```Csharp
    using MimeKit;
using MimeKit.Text;
using System.Net;
using System.Net.Mail;

namespace GenericEmailService;

public static class EmailService
{        
    public static async Task SendEmailWithNetAsync(EmailModel<Attachment> model)
    {            
        using (MailMessage mail = new MailMessage())
        {                
            mail.From = new MailAddress(model.FromEmail);
            foreach (var email in model.ToEmails)
            {
                mail.To.Add(email);
            }
            mail.Subject = model.Subject;
            mail.Body = model.Body;
            mail.IsBodyHtml = model.Configurations.Html;
            if(model.Attachments != null)
            {
                foreach (var attachment in model.Attachments)
                {
                    mail.Attachments.Add(attachment);
                }
            }                
            using (SmtpClient smtp = new SmtpClient(model.Configurations.Smtp))
            {
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(model.FromEmail, model.Configurations.Password);
                smtp.EnableSsl = model.Configurations.SSL;
                smtp.Port = model.Configurations.Port;
                await smtp.SendMailAsync(mail);
            }
        }
    }

    public static async Task<string> SendEmailWithMailKitAsync(EmailModel<Stream> model)
    {
        string response = "";       

        using (var mail = new MimeMessage())
        {
            mail.From.Add(new MailboxAddress(model.FromEmail, model.FromEmail));
            foreach (var email in model.ToEmails)
            {
                mail.To.Add(new MailboxAddress(email, email));
            }
            mail.Subject = model.Subject;

            // Check if we have attachments to send
            if (model.Attachments != null && model.Attachments.Count > 0)
            {
                // Create a multipart/mixed container to hold both the text and the attachments
                var multipart = new Multipart("mixed");
                multipart.Add(new TextPart(TextFormat.Html) { Text = model.Body });

                foreach (var attachmentStream in model.Attachments)
                {
                    var attachment = new MimePart("application", "octet-stream")
                    {
                        Content = new MimeContent(attachmentStream),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = "filename.ext" // You should get the filename from somewhere or modify your SendEmailModel to contain filenames for attachments.
                    };
                    multipart.Add(attachment);
                }

                mail.Body = multipart;
            }
            else
            {
                mail.Body = new TextPart(TextFormat.Html)
                {
                    Text = model.Body
                };
            }

            using (var smtp = new MailKit.Net.Smtp.SmtpClient())
            {
                smtp.Connect(model.Configurations.Smtp, model.Configurations.Port, model.Configurations.SSL);
                smtp.Authenticate(model.FromEmail, model.Configurations.Password);
                response = await smtp.SendAsync(mail);
                smtp.Disconnect(true);
            }

            return response;
        }
    }
}

```