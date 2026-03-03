using AuraPay.Application.DTOs;
using AuraPay.Application.Interfaces;
using AuraPay.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuraPay.WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TransactionsController : BaseController
    {
        private readonly ITransactionService _transactionService;
        private readonly IUserService _userService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService transactionService, IUserService userService, ITransactionRepository transactionRepository, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _userService = userService;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequestDto request)
        {
            // SEGURANÇA: Pegamos o ExternalId do TOKEN, buscamos o nosso User interno
            // e passamos o ID interno para o serviço.
            var externalId = GetExternalId();

            _logger.LogInformation("Requisição de transferência recebida. ExternalId: {ExternalId}, Valor: {Amount}, Para: {Target}",
            externalId, request.Amount, request.DestinationAccountNumber);

            var user = await _userService.GetByExternalIdAsync(externalId);

            if (user == null)
            {
                _logger.LogWarning("Tentativa de transferência por usuário não sincronizado no banco local: {ExternalId}", externalId);
                return Unauthorized("Usuário não sincronizado.");
            }

            var success = await _transactionService.TransferAsync(user.Id, request);

            if (!success)
            {
                _logger.LogError("Falha crítica: O serviço retornou falso para a transferência do usuário {UserId}", user.Id);
                return BadRequest("Não foi possível processar a transferência.");
            }

            _logger.LogInformation("Transferência processada com sucesso para o usuário {UserId}. Origem: {ExternalId}",
            user.Id, externalId);

            return Ok(new { message = "Transferência realizada com sucesso!" });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var externalId = GetExternalId();
            _logger.LogInformation("Buscando extrato para: {ExternalId}", externalId);

            var user = await _userService.GetByExternalIdAsync(externalId);
            if (user == null) return Unauthorized();

            var transactions = await _transactionRepository.GetByAccountIdAsync(user.Id);

            return Ok(transactions);
        }
    }
}
