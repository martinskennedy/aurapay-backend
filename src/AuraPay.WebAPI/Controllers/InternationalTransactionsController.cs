using AuraPay.Application.DTOs;
using AuraPay.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuraPay.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InternationalTransactionsController : BaseController
    {
        private readonly IInternationalTransactionService _internationalService;
        private readonly ILogger<InternationalTransactionsController> _logger;

        public InternationalTransactionsController(
            IInternationalTransactionService internationalService,
            ILogger<InternationalTransactionsController> logger)
        {
            _internationalService = internationalService;
            _logger = logger;
        }

        /// <summary>
        /// Gera uma simulação da remessa com cotação em tempo real e taxas.
        /// </summary>
        [HttpGet("preview")]
        public async Task<ActionResult<InternationalTransferPreviewDto>> GetPreview([FromQuery] decimal amountBrl)
        {
            if (amountBrl <= 0)
                return BadRequest("O valor da remessa deve ser maior que zero.");

            var preview = await _internationalService.CreatePreviewAsync(amountBrl);
            return Ok(preview);
        }

        /// <summary>
        /// Executa a remessa internacional após a confirmação do usuário.
        /// </summary>
        [HttpPost("transfer")]
        public async Task<IActionResult> ExecuteTransfer([FromBody] InternationalTransferRequestDto request)
        {
            var userId = GetUserId();

            _logger.LogInformation("Iniciando remessa internacional para o usuário {UserId}", userId);

            try
            {
                var success = await _internationalService.ExecuteTransferAsync(userId, request);

                if (success)
                {
                    return Ok(new { Message = "Transferência internacional enviada com sucesso!" });
                }

                return StatusCode(500, "Ocorreu um erro interno ao processar a transferência.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado no Controller de Transações Internacionais.");
                return StatusCode(500, "Erro inesperado. Tente novamente mais tarde.");
            }
        }
    }
}
