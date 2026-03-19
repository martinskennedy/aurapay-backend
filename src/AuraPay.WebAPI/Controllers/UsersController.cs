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

        /// <summary>
        /// Registra um novo usuário no sistema AuraPay.
        /// </summary>
        /// <remarks>
        /// Este endpoint cria o perfil local, gera o Hash da senha e cria uma conta bancária com bônus.
        /// </remarks>
        /// <param name="request">Dados do perfil e senha.</param>
        /// <response code="200">Usuário registrado com sucesso.</response>
        /// <response code="400">E-mail ou Documento já cadastrados.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateUserRequestDto request)
        {
            _logger.LogInformation("Novo registro solicitado para o e-mail: {Email}", request.Email);

            var user = await _userService.RegisterUserAsync(request, request.Password);

            return Ok(user);
        }

        /// <summary>
        /// Obtém os dados do perfil do usuário atualmente autenticado.
        /// </summary>
        /// <response code="200">Dados do perfil recuperados com sucesso.</response>
        /// <response code="401">Token inválido.</response>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetUserId();

            _logger.LogInformation("Buscando perfil para o UserId: {UserId}", userId);

            var user = await _userService.GetByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("UserId não foi encontrado no banco: {UserId}", userId);
                return NotFound(new { message = "Usuário não encontrado." });
            }

            return Ok(user);
        }
    }
}