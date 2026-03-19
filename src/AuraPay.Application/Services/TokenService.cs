using AuraPay.Application.DTOs;
using AuraPay.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(UserDto user)
        {
            // 1. Define a chave secreta
            var secretKey = _config["JwtSettings:Secret"] ?? "Chave_Super_Secreta_De_Pelo_Menos_32_Caracteres";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // 2. Define os Claims (o que o Token "carrega" de informação)
            // Injeta o NameIdentifier que o BaseController.GetUserId() procura
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };

            // 3. Credenciais de assinatura
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4. Configurações do Token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Token válido por 1 hora
                SigningCredentials = creds,
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
