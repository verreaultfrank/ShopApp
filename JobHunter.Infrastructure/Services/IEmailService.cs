using JobHunter.Domain.Models;

namespace JobHunter.Infrastructure.Services;

public interface IEmailService
{
    Task SendQuoteEmailAsync(Lead job, Quote quote);
}
