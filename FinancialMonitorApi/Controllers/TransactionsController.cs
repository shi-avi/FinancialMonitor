using FinancialMonitorApi.DTOs;
using FinancialMonitorApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialMonitorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionDto dto)
    {
        try
        {
            var transaction = await _transactionService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = transaction.TransactionId },
                transaction
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var transactions = await _transactionService.GetAllAsync();

        return Ok(transactions);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var transaction = await _transactionService.GetByIdAsync(id);

        if (transaction == null)
            return NotFound();

        return Ok(transaction);
    }
}