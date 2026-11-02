
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinancialMonitorApi.Data;
using FinancialMonitorApi.DTOs;
using FinancialMonitorApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FinancialMonitorApi.Tests.Integration;

public class TransactionsApiTests
{
    [Fact]
    public async Task PostTransaction_ThenGetById_ReturnsCreatedTransaction()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<AppDbContext>();
                    services.RemoveAll<DbContextOptions<AppDbContext>>();
                    services.RemoveAll<DbContextOptions>();

                    var databaseName = Guid.NewGuid().ToString();

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase(databaseName));
                });
            });

        var client = factory.CreateClient();

        var jsonOptions = new JsonSerializerOptions(
            JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter(
                    namingPolicy: null,
                    allowIntegerValues: false)
            }
        };

        var dto = new CreateTransactionDto
        {
            Amount = 150.50m,
            Currency = "ILS",
            Status = TransactionStatus.Completed
        };

        // Act - Create
        var postResponse = await client.PostAsJsonAsync(
            "/api/transactions",
            dto,
            jsonOptions);

        // Assert - Create
        var responseBody = await postResponse.Content.ReadAsStringAsync();

        Assert.True(
            postResponse.StatusCode == HttpStatusCode.Created,
            $"Expected Created but got {postResponse.StatusCode}. Response: {responseBody}"
        );

        var createdTransaction =
            await postResponse.Content.ReadFromJsonAsync<Transaction>(jsonOptions);

        Assert.NotNull(createdTransaction);

        // Act - Get
        var getResponse = await client.GetAsync(
            $"/api/transactions/{createdTransaction.TransactionId}");

        // Assert - Get
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var retrievedTransaction =
            await getResponse.Content.ReadFromJsonAsync<Transaction>(jsonOptions);

        Assert.NotNull(retrievedTransaction);

        Assert.Equal(
            createdTransaction.TransactionId,
            retrievedTransaction.TransactionId);

        Assert.Equal(150.50m, retrievedTransaction.Amount);
        Assert.Equal("ILS", retrievedTransaction.Currency);
        Assert.Equal(TransactionStatus.Completed, retrievedTransaction.Status);
    }
}
