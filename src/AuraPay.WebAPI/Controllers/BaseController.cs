using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuraPay.WebAPI.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Extrai o ID único do Supabase (sub) de dentro do Token JWT.
        /// </summary>
        protected Guid GetExternalId()
        {
            // O Supabase mapeia o ID do usuário no claim 'sub' ou 'nameidentifier'
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

            if (claim == null || !Guid.TryParse(claim.Value, out var guidId))
            {
                // Lançamos uma exceção personalizada ou genérica que o Middleware capturará
                throw new UnauthorizedAccessException("Usuário não identificado no token de autenticação.");
            }

            return guidId;
        }
    }
}
