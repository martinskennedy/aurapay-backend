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
        private readonly IUserService _userService;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(
            IAccountService accountService,
            IUserService userService,
            ILogger<AccountsController> logger)
        {
            _accountService = accountService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            // 1. Extraímos o ExternalId (sub do Supabase) do Token
            var externalId = GetExternalId();

            _logger.LogInformation("Consulta de saldo solicitada pelo ExternalId: {ExternalId}", externalId);

            // 2. Buscamos o NOSSO UserId interno usando o ExternalId
            var user = await _userService.GetByExternalIdAsync(externalId);

            if (user == null)
            {
                _logger.LogWarning("Tentativa de acesso a saldo por usuário não sincronizado: {ExternalId}", externalId);
                return NotFound(new { message = "Usuário não encontrado no sistema local." });
            }

            // 3. Buscamos o saldo usando o ID interno (Proteção de Domínio)
            var account = await _accountService.GetBalanceAsync(user.Id);

            if (account == null)
                return NotFound(new { message = "Conta bancária não encontrada." });

            return Ok(account);
        }
    }
}
