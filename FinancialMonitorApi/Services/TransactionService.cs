using FinancialMonitorApi.DTOs;
using FinancialMonitorApi.Models;
using FinancialMonitorApi.Repositories.Interfaces;
using FinancialMonitorApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FinancialMonitorApi.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repository;
    private readonly IHubContext<TransactionsHub> _hubContext;

    public TransactionService(ITransactionRepository repository, IHubContext<TransactionsHub> hubContext)
    {
        _repository = repository;
        _hubContext = hubContext;
    }

    public async Task<Transaction> CreateAsync(CreateTransactionDto dto)
    {
        if (dto.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.Currency) ||
            dto.Currency.Length != 3 ||
            dto.Currency.Any(c => !char.IsAsciiLetter(c)))
        {
            throw new ArgumentException("Currency must contain exactly 3 letters.");
        }

        if (!Enum.IsDefined(dto.Status))
            throw new ArgumentException("Invalid transaction status.");

        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Amount = dto.Amount,
            Currency = dto.Currency.ToUpperInvariant(),
            Status = dto.Status,
            Timestamp = DateTime.UtcNow
        };

        await _repository.AddAsync(transaction);
        
        Console.WriteLine(
            $"Sending transaction status: {transaction.Status}"
        );

        await _hubContext.Clients.All.SendAsync(
            "NewTransaction",
            transaction
        );

        return transaction;
    }
    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Transaction?> GetByIdAsync(Guid transactionId)
    {
        return await _repository.GetByIdAsync(transactionId);
    }
}