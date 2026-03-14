using JobHunter.Domain.Models;
using Microsoft.Extensions.Logging;

namespace JobHunter.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendQuoteEmailAsync(Lead job, Quote quote)
    {
        // Mock implementation of sending an email
        _logger.LogInformation("===========================================");
        _logger.LogInformation("MOCK EMAIL SENT");
        _logger.LogInformation($"To: {job.Business?.Name ?? "Unknown"} Contact");
        _logger.LogInformation($"Subject: Quote for {job.Title} (ID: {job.Id})");
        _logger.LogInformation($"Price: {quote.Price:C}");
        _logger.LogInformation($"Lead Time: {quote.LeadTime.TotalDays} days");
        _logger.LogInformation($"Message: {quote.Message}");
        _logger.LogInformation("===========================================");

        return Task.CompletedTask;
    }
}
