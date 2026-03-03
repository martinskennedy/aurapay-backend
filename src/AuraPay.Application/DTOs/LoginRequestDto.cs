using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.DTOs
{
    public record LoginRequestDto(string Email, string Password);
    public record LoginResponseDto(string AccessToken, string TokenType, int ExpiresIn);
}
