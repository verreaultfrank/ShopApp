using JobHunter.Domain.Models;

namespace JobHunter.Application.Interfaces;

public interface IEmailService
{
    Task SendQuoteEmailAsync(JobOpportunity job, Quote quote);
}
