using FinancialMonitorApi.Models;

namespace FinancialMonitorApi.DTOs;

public class CreateTransactionDto
{
    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public TransactionStatus Status { get; set; }
}