using AuraPay.Application.DTOs;
using AuraPay.Application.Interfaces;
using AuraPay.Application.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuraPay.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CardsController : BaseController
    {
        private readonly ICardService _cardService;
        private readonly IUserService _userService;

        public CardsController(ICardService cardService, IUserService userService)
        {
            _cardService = cardService;
            _userService = userService;
        }

        /// <summary>
        /// Solicita a criação de um novo cartão virtual.
        /// </summary>
        /// <param name="request">Dados para emissão do cartão (ex: nome impresso).</param>
        /// <response code="200">Cartão criado com sucesso.</response>
        /// <response code="401">Usuário não autenticado ou sincronizado.</response>
        [HttpPost("virtual")]
        public async Task<IActionResult> CreateVirtualCard([FromBody] CreateCardRequest request)
        {
            var externalId = GetExternalId();
            var user = await _userService.GetByExternalIdAsync(externalId);

            if (user == null) return Unauthorized();

            var card = await _cardService.CreateVirtualCardAsync(user.Id, request.HolderName);
            return Ok(card);
        }

        /// <summary>
        /// Lista todos os cartões vinculados à conta do usuário.
        /// </summary>
        /// <remarks>
        /// Esta rota retorna os dados mascarados dos cartões (apenas os últimos 4 dígitos).
        /// </remarks>
        /// <response code="200">Lista de cartões recuperada com sucesso.</response>
        [HttpGet("my-cards")]
        public async Task<IActionResult> GetMyCards()
        {
            var externalId = GetExternalId();
            var user = await _userService.GetByExternalIdAsync(externalId);

            if (user == null) return Unauthorized();

            // Chamando o método da interface conforme você pontuou
            var cards = await _cardService.GetMyCardsAsync(user.Id);
            return Ok(cards);
        }

        /// <summary>
        /// Revela os dados sensíveis de um cartão (Número completo, CVV e Validade).
        /// </summary>
        /// <remarks>
        /// **Atenção:** Esta rota deve ser chamada apenas sob demanda para evitar exposição desnecessária.
        /// </remarks>
        /// <param name="cardId">ID único do cartão.</param>
        /// <response code="200">Dados sensíveis retornados com sucesso.</response>
        /// <response code="403">O usuário logado não tem permissão para visualizar este cartão.</response>
        /// <response code="404">Cartão não encontrado.</response>
        [HttpGet("{cardId}/reveal")]
        public async Task<IActionResult> RevealCardData(Guid cardId)
        {
            var externalId = GetExternalId();
            var user = await _userService.GetByExternalIdAsync(externalId);

            if (user == null) return Unauthorized();

            try
            {
                var sensitiveData = await _cardService.GetSensitiveDataAsync(cardId, user.Id);
                if (sensitiveData == null) return NotFound(new { message = "Cartão não encontrado." });

                return Ok(sensitiveData);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Ativa ou Desativa temporariamente um cartão.
        /// </summary>
        /// <param name="cardId">ID único do cartão.</param>
        /// <response code="200">Status alterado com sucesso.</response>
        /// <response code="400">Falha ao tentar alterar o status (ex: cartão cancelado permanentemente).</response>
        [HttpPatch("{cardId}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(Guid cardId)
        {
            var externalId = GetExternalId();
            var user = await _userService.GetByExternalIdAsync(externalId);

            if (user == null) return Unauthorized();

            var result = await _cardService.ToggleCardStatusAsync(cardId, user.Id);

            if (!result) return BadRequest(new { message = "Não foi possível alterar o status do cartão." });

            return Ok(new { message = "Status do cartão atualizado com sucesso." });
        }
    }
}
