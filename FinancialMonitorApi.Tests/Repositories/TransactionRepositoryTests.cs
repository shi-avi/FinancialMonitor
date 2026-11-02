
using FinancialMonitorApi.Data;
using FinancialMonitorApi.Models;
using FinancialMonitorApi.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FinancialMonitorApi.Tests.Repositories;

public class TransactionRepositoryTests
{
    [Fact]
    public async Task AddAsync_WhenTransactionIsValid_SavesTransactionToDatabase()
    {
        // Arrange
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var repository = new TransactionRepository(context);

        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Amount = 150.50m,
            Currency = "ILS",
            Status = TransactionStatus.Completed,
            Timestamp = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(transaction);

        var savedTransaction =
            await repository.GetByIdAsync(transaction.TransactionId);

        // Assert
        Assert.NotNull(savedTransaction);
        Assert.Equal(transaction.TransactionId, savedTransaction.TransactionId);
        Assert.Equal(150.50m, savedTransaction.Amount);
        Assert.Equal("ILS", savedTransaction.Currency);
        Assert.Equal(TransactionStatus.Completed, savedTransaction.Status);
    }

    [Fact]
    public async Task GetAllAsync_WhenTransactionsExist_ReturnsAllTransactions()
    {
        // Arrange
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var repository = new TransactionRepository(context);

        var transaction1 = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "ILS",
            Status = TransactionStatus.Completed,
            Timestamp = DateTime.UtcNow
        };

        var transaction2 = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Amount = 250m,
            Currency = "USD",
            Status = TransactionStatus.Pending,
            Timestamp = DateTime.UtcNow
        };

        await repository.AddAsync(transaction1);
        await repository.AddAsync(transaction2);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, t => t.TransactionId == transaction1.TransactionId);
        Assert.Contains(result, t => t.TransactionId == transaction2.TransactionId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTransactionDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var repository = new TransactionRepository(context);
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistingId);

        // Assert
        Assert.Null(result);
    }

    
    [Fact]
    public async Task GetAllAsync_WhenDatabaseIsEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var repository = new TransactionRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task AddAsync_WhenMultipleTransactionsAreAddedConcurrently_SavesAllTransactions()
    {
        // Arrange
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
        }

        var transactions = Enumerable.Range(1, 10)
            .Select(i => new Transaction
            {
                TransactionId = Guid.NewGuid(),
                Amount = i * 10m,
                Currency = "ILS",
                Status = TransactionStatus.Completed,
                Timestamp = DateTime.UtcNow
            })
            .ToList();

        // Act
        var tasks = transactions.Select(async transaction =>
        {
            await using var context = new AppDbContext(options);
            var repository = new TransactionRepository(context);
            await repository.AddAsync(transaction);
        });

        await Task.WhenAll(tasks);

        // Assert
        await using var verifyContext = new AppDbContext(options);

        var savedTransactions = await verifyContext.Transactions.ToListAsync();

        Assert.Equal(10, savedTransactions.Count);
    }


}
