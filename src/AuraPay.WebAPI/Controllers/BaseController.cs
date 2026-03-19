using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuraPay.WebAPI.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Extrai o ID real do usuário (chave primária do banco AuraPay) de dentro do Token JWT.
        /// </summary>
        protected Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            {
                // Lançamos uma exceção personalizada ou genérica que o Middleware capturará
                throw new UnauthorizedAccessException("Usuário não identificado no token.");
            }

            return userId;
        }
    }
}
