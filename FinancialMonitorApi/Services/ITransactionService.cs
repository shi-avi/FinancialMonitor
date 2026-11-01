using FinancialMonitorApi.DTOs;
using FinancialMonitorApi.Models;

namespace FinancialMonitorApi.Services;

public interface ITransactionService
{
    Task<Transaction> CreateAsync(CreateTransactionDto dto);
    
    Task<IEnumerable<Transaction>> GetAllAsync();

    Task<Transaction?> GetByIdAsync(Guid transactionId);

}