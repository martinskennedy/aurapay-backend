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

        [HttpPost("virtual")]
        public async Task<IActionResult> CreateVirtualCard([FromBody] CreateCardRequest request)
        {
            var externalId = GetExternalId();
            var user = await _userService.GetByExternalIdAsync(externalId);

            if (user == null) return Unauthorized();

            var card = await _cardService.CreateVirtualCardAsync(user.Id, request.HolderName);
            return Ok(card);
        }

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
