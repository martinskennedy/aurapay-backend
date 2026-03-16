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
        /// Sincroniza um novo usuário autenticado pelo Supabase.
        /// </summary>
        /// <remarks>
        /// **Fluxo de Autenticação:**
        /// 1. O usuário deve se autenticar primeiro no **Supabase Auth**.
        /// 2. O `access_token` retornado pelo Supabase deve ser inserido no botão **Authorize** (Bearer).
        /// 3. Este endpoint extrai o `ExternalId` (claim 'sub') do token para criar o perfil local e uma conta bancária com bônus inicial.
        /// 
        /// *Nota: Se o usuário já estiver sincronizado, retornará erro 400.*        
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