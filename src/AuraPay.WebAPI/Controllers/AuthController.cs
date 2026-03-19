using AuraPay.Application.DTOs;
using AuraPay.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AuraPay.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ITokenService tokenService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _logger = logger;
        }
        /// <summary>
        /// Realiza a autenticação do usuário no sistema AuraPay.
        /// </summary>
        /// <remarks>
        /// Valida e-mail e senha no banco local e retorna um JWT assinado internamente.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            _logger.LogInformation("Tentativa de login para o e-mail: {Email}", request.Email);

            // 1. Valida as credenciais usando o seu UserService
            var user = await _userService.ValidateUserAsync(request.Email, request.Password);

            if (user == null)
            {
                _logger.LogWarning("Falha na autenticação: E-mail ou senha incorretos para {Email}", request.Email);
                return Unauthorized(new { message = "E-mail ou senha inválidos." });
            }

            // 2. Gera Token JWT
            // Passa o objeto UserDto para o TokenService extrair o ID e o Nome para os Claims
            var token = _tokenService.GenerateToken(user);

            _logger.LogInformation("Usuário {UserId} autenticado com sucesso.", user.Id);

            return Ok(new
            {
                accessToken = token,
                user = user
            });
        }
    }
}
