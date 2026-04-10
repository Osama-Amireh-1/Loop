using Loop.Application.Abstractions.Communication;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Loop.Infrastructure.Communication;

internal sealed class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        string host = configuration["Email:SmtpHost"] ?? throw new InvalidOperationException("Email:SmtpHost is not configured.");
        int port = int.TryParse(configuration["Email:SmtpPort"], out int parsedPort) ? parsedPort : 587;
        string fromAddress = configuration["Email:FromAddress"] ?? throw new InvalidOperationException("Email:FromAddress is not configured.");
        string? fromName = configuration["Email:FromName"];
        string? username = configuration["Email:Username"];
        string? password = configuration["Email:Password"];
        bool enableSsl = bool.TryParse(configuration["Email:EnableSsl"], out bool parsedEnableSsl) && parsedEnableSsl;

        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(to);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        await client.SendMailAsync(message, cancellationToken);
    }
}

