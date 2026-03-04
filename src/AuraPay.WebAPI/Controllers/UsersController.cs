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