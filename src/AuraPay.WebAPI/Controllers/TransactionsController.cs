using AuraPay.Application.DTOs;
using AuraPay.Application.Interfaces;
using AuraPay.Domain.Interfaces;
using AuraPay.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuraPay.WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : BaseController
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        /// <summary>
        /// Realiza uma transferência de valores entre contas.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/transactions/transfer
        ///     {
        ///        "destinationAccountNumber": "12345-6",
        ///        "amount": 150.00
        ///     }
        /// 
        /// </remarks>
        /// <param name="request">Objeto contendo os dados da conta de destino e o valor.</param>
        /// <returns>Mensagem de confirmação da transação.</returns>
        /// <response code="200">Transferência realizada com sucesso.</response>
        /// <response code="400">Dados inválidos, saldo insuficiente ou tentativa de auto-transferência.</response>
        /// <response code="401">Usuário não autenticado ou não sincronizado no sistema.</response>
        /// <response code="404">Conta de destino não encontrada.</response>
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequestDto request)
        {
            var userId = GetUserId();

            _logger.LogInformation("Transferência solicitada pelo UserId: {UserId}, Valor: {Amount}, Para: {Target}",
            userId, request.Amount, request.DestinationAccountNumber);

            var success = await _transactionService.TransferAsync(userId, request);

            if (!success)
            {
                _logger.LogError("Falha ao processar transferência para o usuário {UserId}", userId);
                return BadRequest(new { message = "Não foi possível completar a transferência." });
            }

            return Ok(new { message = "Transferência realizada com sucesso!" });
        }

        /// <summary>
        /// Recupera o histórico de transações da conta do usuário autenticado.
        /// </summary>
        /// <remarks>
        /// Retorna uma lista de todas as entradas e saídas (débitos e créditos) 
        /// ordenadas da mais recente para a mais antiga.
        /// </remarks>
        /// <returns>Uma lista de transações formatadas para exibição.</returns>
        /// <response code="200">Retorna a lista de transações com sucesso.</response>
        /// <response code="401">Usuário não autenticado ou token inválido.</response>
        /// <response code="404">Conta bancária não localizada para o usuário.</response>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = GetUserId();

            _logger.LogInformation("Buscando extrato para: UserId: {UserId}", userId);

            var history = await _transactionService.GetHistoryAsync(userId);

            return Ok(history);
        }
    }
}
