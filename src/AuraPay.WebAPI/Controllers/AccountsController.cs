using AuraPay.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuraPay.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Bloqueia tudo neste controller para usuários não autenticados
    public class AccountsController : BaseController
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém o saldo atual e os dados básicos da conta do usuário autenticado.
        /// </summary>
        /// <remarks>
        /// Retorna o saldo disponível em tempo real e o número da conta formatado. 
        /// O ID do usuário é extraído automaticamente do Token JWT.
        /// </remarks>
        /// <returns>Dados da conta e saldo disponível.</returns>
        /// <response code="200">Saldo retornado com sucesso.</response>
        /// <response code="401">Usuário não autenticado.</response>
        /// <response code="404">Usuário não sincronizado ou conta não localizada no sistema local.</response>
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            // 1. Extrai o UserId diretamente dos Claims do Token JWT
            var userId = GetUserId();

            _logger.LogInformation("Consulta de saldo solicitada pelo UserId: {UserId}", userId);

            // 2. Busca o saldo
            var account = await _accountService.GetBalanceAsync(userId);

            if (account == null)
            {
                _logger.LogWarning("Conta bancária não encontrada para o UserId: {UserId}", userId);
                return NotFound(new { message = "Conta bancária não encontrada." });
            }

            return Ok(account);
        }
    }
}
