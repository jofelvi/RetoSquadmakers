using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Infrastructure.ExternalServices;

public class EmailNotificationProvider : INotificationProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationProvider> _logger;

    public string Type => "Email";

    public EmailNotificationProvider(IConfiguration configuration, ILogger<EmailNotificationProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool CanHandle(string notificationType)
    {
        return string.Equals(notificationType, Type, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<NotificationResult> SendAsync(NotificationMessage message)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = smtpSettings["Host"] ?? "smtp.gmail.com";
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            
            // Check if SMTP credentials are configured
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogInformation("SMTP credentials not configured. Simulating email send to {Recipient} with subject: {Subject}", 
                    message.Recipient, message.Subject);
                
                // Simulate network delay
                await Task.Delay(200);
                
                // Return success with simulated ID
                var simulatedId = $"simulated_email_{DateTime.UtcNow.Ticks}";
                return NotificationResult.Success(simulatedId);
            }
            
            var fromEmail = smtpSettings["FromEmail"] ?? username;
            var fromName = smtpSettings["FromName"] ?? "RetoSquadmakers";

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = message.Subject,
                Body = message.Content,
                IsBodyHtml = IsHtmlContent(message.Content)
            };

            mailMessage.To.Add(message.Recipient);

            // Add metadata as headers if present
            if (message.Metadata != null)
            {
                foreach (var metadata in message.Metadata)
                {
                    if (metadata.Value != null)
                    {
                        mailMessage.Headers.Add($"X-Custom-{metadata.Key}", metadata.Value.ToString());
                    }
                }
            }

            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent successfully to {Recipient} with subject: {Subject}", 
                message.Recipient, message.Subject);

            return NotificationResult.Success($"email_{DateTime.UtcNow.Ticks}");
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending email to {Recipient}: {Error}", 
                message.Recipient, ex.Message);
            return NotificationResult.Failure($"SMTP Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email to {Recipient}: {Error}", 
                message.Recipient, ex.Message);
            return NotificationResult.Failure($"Error: {ex.Message}");
        }
    }

    private static bool IsHtmlContent(string content)
    {
        return content.Contains("<html>", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("<body>", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("<div>", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("<p>", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("<br>", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("</", StringComparison.OrdinalIgnoreCase);
    }
}