using FinancialMonitorApi.Models;

namespace FinancialMonitorApi.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid transactionId);

    Task<IEnumerable<Transaction>> GetAllAsync();

    Task AddAsync(Transaction transaction);
}