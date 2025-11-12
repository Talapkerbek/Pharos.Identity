using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace Pharos.Identity.Infra.Email;
public class ResendEmailSender(IResend resend, IConfiguration configuration, ILogger<ResendEmailSender> logger) : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        logger.LogInformation("Sending email to {Email} with subject {Subject}", email, subject);
        
        var message = new EmailMessage
        {
            From = configuration["Resend:Email"]!,
            To = { email },
            Subject = subject,
            HtmlBody = htmlMessage
        };

        await resend.EmailSendAsync(message);
        
        logger.LogInformation("Success! Email sent to {Email} with subject {Subject}", email, subject);
    }
}