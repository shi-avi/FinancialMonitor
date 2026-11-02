using Xunit;
using Moq;
using FinancialMonitorApi.Services;
using FinancialMonitorApi.Repositories.Interfaces;
using FinancialMonitorApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using FinancialMonitorApi.DTOs;
using System.Threading;

namespace FinancialMonitorApi.Tests.Services;

public class TransactionServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenAmountIsZero_ThrowsArgumentException()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var hubMock = new Mock<IHubContext<TransactionsHub>>();

        var service = new TransactionService(
            repositoryMock.Object,
            hubMock.Object
        );

        var dto = new CreateTransactionDto
        {
            Amount = 0,
            Currency = "USD",
            Status = FinancialMonitorApi.Models.TransactionStatus.Pending
        };

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(dto)
        );
    }

    [Fact]
    public async Task CreateAsync_WhenDataIsValid_SavesTransaction()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();

        clientsMock
            .Setup(c => c.All)
            .Returns(clientProxyMock.Object);

        var hubMock = new Mock<IHubContext<TransactionsHub>>();

        hubMock
            .Setup(h => h.Clients)
            .Returns(clientsMock.Object);

        clientProxyMock
            .Setup(c => c.SendCoreAsync(
                "NewTransaction",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);

        var service = new TransactionService(
            repositoryMock.Object,
            hubMock.Object
        );

        var dto = new CreateTransactionDto
        {
            Amount = 100,
            Currency = "USD",
            Status = FinancialMonitorApi.Models.TransactionStatus.Pending
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.Equal(100, result.Amount);
        Assert.Equal("USD", result.Currency);

        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<FinancialMonitorApi.Models.Transaction>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAsync_WhenCurrencyIsInvalid_ThrowsArgumentException()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var hubMock = new Mock<IHubContext<TransactionsHub>>();

        var service = new TransactionService(
            repositoryMock.Object,
            hubMock.Object
        );

        var dto = new CreateTransactionDto
        {
            Amount = 100,
            Currency = "US",
            Status = FinancialMonitorApi.Models.TransactionStatus.Pending
        };

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(dto)
        );

        // Verify: לא בוצעה שמירה
        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<FinancialMonitorApi.Models.Transaction>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAsync_WhenStatusIsInvalid_ThrowsArgumentException()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var hubMock = new Mock<IHubContext<TransactionsHub>>();

        var service = new TransactionService(
            repositoryMock.Object,
            hubMock.Object
        );

        var dto = new CreateTransactionDto
        {
            Amount = 100,
            Currency = "USD",
            Status = (FinancialMonitorApi.Models.TransactionStatus)999
        };

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(dto)
        );

        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<FinancialMonitorApi.Models.Transaction>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAsync_WhenCurrencyIsLowercase_ConvertsToUppercase()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();

        clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);

        var hubMock = new Mock<IHubContext<TransactionsHub>>();
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        clientProxyMock
            .Setup(c => c.SendCoreAsync(
                "NewTransaction",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);

        var service = new TransactionService(repositoryMock.Object, hubMock.Object);

        var dto = new CreateTransactionDto
        {
            Amount = 100,
            Currency = "usd",
            Status = FinancialMonitorApi.Models.TransactionStatus.Pending
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task CreateAsync_WhenDataIsValid_BroadcastsTransaction()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();

        clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);

        var hubMock = new Mock<IHubContext<TransactionsHub>>();
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        clientProxyMock
            .Setup(c => c.SendCoreAsync(
                "NewTransaction",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);

        var service = new TransactionService(repositoryMock.Object, hubMock.Object);

        var dto = new CreateTransactionDto
        {
            Amount = 100,
            Currency = "USD",
            Status = FinancialMonitorApi.Models.TransactionStatus.Pending
        };

        // Act
        await service.CreateAsync(dto);

        // Assert
        clientProxyMock.Verify(
            c => c.SendCoreAsync(
                "NewTransaction",
                It.Is<object[]>(args =>
                    args.Length == 1 &&
                    args[0] is FinancialMonitorApi.Models.Transaction
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
    [Fact]
    public async Task CreateAsync_SavesBeforeBroadcasting()
    {
        // Arrange
        var sequence = new MockSequence();

        var repositoryMock = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();

        clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);

        var hubMock = new Mock<IHubContext<TransactionsHub>>();
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        repositoryMock
            .InSequence(sequence)
            .Setup(r => r.AddAsync(It.IsAny<FinancialMonitorApi.Models.Transaction>()))
            .Returns(Task.CompletedTask);

        clientProxyMock
            .InSequence(sequence)
            .Setup(c => c.SendCoreAsync(
                "NewTransaction",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);

        var service = new TransactionService(repositoryMock.Object, hubMock.Object);

        var dto = new CreateTransactionDto
        {
            Amount = 100,
            Currency = "USD",
            Status = FinancialMonitorApi.Models.TransactionStatus.Pending
        };

        // Act
        await service.CreateAsync(dto);

        // Assert
        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<FinancialMonitorApi.Models.Transaction>()),
            Times.Once
        );

        clientProxyMock.Verify(
            c => c.SendCoreAsync(
                "NewTransaction",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
    [Fact]
public async Task CreateAsync_WhenRepositoryFails_DoesNotBroadcast()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var hubMock = new Mock<IHubContext<TransactionsHub>>();

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<FinancialMonitorApi.Models.Transaction>()))
            .ThrowsAsync(new Exception("Database error"));

        var service = new TransactionService(
            repositoryMock.Object,
            hubMock.Object
        );

        var dto = new CreateTransactionDto
        {
            Amount = 100,
            Currency = "USD",
            Status = FinancialMonitorApi.Models.TransactionStatus.Pending
        };

        // Act
        await Assert.ThrowsAsync<Exception>(
            () => service.CreateAsync(dto)
        );

        // Assert
        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<FinancialMonitorApi.Models.Transaction>()),
            Times.Once
        );

        hubMock.Verify(
            h => h.Clients,
            Times.Never
        );
    }
    [Fact]
    public async Task CreateAsync_WhenAmountIsNegative_ThrowsArgumentException()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var hubMock = new Mock<IHubContext<TransactionsHub>>();

        var service = new TransactionService(
            repositoryMock.Object,
            hubMock.Object
        );

        var dto = new CreateTransactionDto
        {
            Amount = -50,
            Currency = "USD",
            Status = FinancialMonitorApi.Models.TransactionStatus.Pending
        };

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(dto)
        );

        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<FinancialMonitorApi.Models.Transaction>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAsync_WhenCurrencyIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var hubMock = new Mock<IHubContext<TransactionsHub>>();

        var service = new TransactionService(
            repositoryMock.Object,
            hubMock.Object
        );

        var dto = new CreateTransactionDto
        {
            Amount = 100,
            Currency = "",
            Status = FinancialMonitorApi.Models.TransactionStatus.Pending
        };

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(dto)
        );

        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<FinancialMonitorApi.Models.Transaction>()),
            Times.Never
        );
    }
    [Fact]
    public async Task CreateAsync_WhenCurrencyHasMoreThanThreeLetters_ThrowsArgumentException()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var hubMock = new Mock<IHubContext<TransactionsHub>>();

        var service = new TransactionService(
            repositoryMock.Object,
            hubMock.Object
        );

        var dto = new CreateTransactionDto
        {
            Amount = 100,
            Currency = "USDD",
            Status = FinancialMonitorApi.Models.TransactionStatus.Pending
        };

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(dto)
        );

        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<FinancialMonitorApi.Models.Transaction>()),
            Times.Never
        );
    }
    [Fact]
    public async Task GetByIdAsync_WhenTransactionExists_ReturnsTransaction()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        var transaction = new FinancialMonitorApi.Models.Transaction
        {
            TransactionId = transactionId,
            Amount = 250,
            Currency = "USD",
            Status = FinancialMonitorApi.Models.TransactionStatus.Completed,
            Timestamp = DateTime.UtcNow
        };

        var repositoryMock = new Mock<ITransactionRepository>();

        repositoryMock
            .Setup(r => r.GetByIdAsync(transactionId))
            .ReturnsAsync(transaction);

        var hubMock = new Mock<IHubContext<TransactionsHub>>();

        var service = new TransactionService(
            repositoryMock.Object,
            hubMock.Object
        );

        // Act
        var result = await service.GetByIdAsync(transactionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transactionId, result.TransactionId);
        Assert.Equal(250, result.Amount);
    }
}