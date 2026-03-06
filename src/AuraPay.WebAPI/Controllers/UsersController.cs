using AuraPay.Application.DTOs;
using AuraPay.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuraPay.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        //[HttpPost("register-teste")]
        //[AllowAnonymous] // Libera sem token apenas para esse teste
        //public async Task<IActionResult> RegisterTeste([FromBody] CreateUserRequestDto request, [FromQuery] Guid idManual)
        //{
        //    // Usamos o ID que você copiou do Supabase Auth (aquele 2cc84684...)
        //    var user = await _userService.RegisterUserAsync(request, idManual);
        //    return Ok(user);
        //}

        /// <summary>
        /// Cria um novo usuário.
        /// </summary>
        /// <remarks>
        /// Ele extrai o ID do provedor de identidade do Token e cria o registro local, incluindo a conta bancária inicial.
        /// </remarks>
        /// <param name="request">Dados básicos do perfil (Nome, Documento, etc).</param>
        /// <response code="200">Usuário sincronizado e conta criada com sucesso.</response>
        /// <response code="400">Dados de entrada inválidos ou usuário já sincronizado.</response>
        /// <response code="401">Token de autenticação ausente ou inválido.</response>
        [HttpPost("register")]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] CreateUserRequestDto request)
        {
            // Pegamos o ID real vindo do Token do Supabase
            var externalId = GetExternalId();
            _logger.LogInformation("Sincronizando usuário do Supabase: {ExternalId}", externalId);

            // Passamos o ID do token para o serviço
            var user = await _userService.RegisterUserAsync(request, externalId);
            return Ok(user);
        }

        /// <summary>
        /// Obtém os dados do perfil do usuário atualmente autenticado.
        /// </summary>
        /// <response code="200">Dados do perfil recuperados com sucesso.</response>
        /// <response code="401">Token inválido.</response>
        /// <response code="404">Usuário autenticado mas ainda não sincronizado (chame o /register).</response>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var externalId = GetExternalId();
            _logger.LogInformation("Buscando perfil para ExternalId: {ExternalId}", externalId);

            var user = await _userService.GetByExternalIdAsync(externalId);

            if (user == null)
            {
                _logger.LogWarning("Usuário autenticado no Supabase mas não encontrado no AuraPay: {ExternalId}", externalId);
                return NotFound(new { message = "Usuário não sincronizado." });
            }

            return Ok(user);
        }
    }
}