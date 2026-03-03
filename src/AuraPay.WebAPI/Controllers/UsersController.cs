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

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateUserRequestDto request)
        {
            _logger.LogInformation("Tentativa de registro: {Email}", request.Email);

            var user = await _userService.RegisterUserAsync(request);
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