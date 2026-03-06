using AuraPay.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AuraPay.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Realiza a autenticação do usuário junto ao Supabase Auth.
        /// </summary>
        /// <remarks>
        /// Ao fornecer e-mail e senha válidos, você receberá um **accessToken**.
        /// Use este token no botão "Authorize" (campo Bearer) para acessar as rotas protegidas.
        /// </remarks>
        /// <param name="request">Credenciais de acesso (Email e Password).</param>
        /// <returns>O token de acesso JWT.</returns>
        /// <response code="200">Autenticação bem-sucedida, retorna o token JWT.</response>
        /// <response code="401">E-mail ou senha inválidos.</response>
        /// <response code="500">Erro na comunicação com o provedor de identidade (Supabase).</response>
        [AllowAnonymous] // Permite logar sem estar autenticado
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var client = _httpClientFactory.CreateClient();

            // A URL do seu Supabase Auth
            var supabaseUrl = "https://tgfipyvrglihoqwtfkug.supabase.co/auth/v1/token?grant_type=password";
            var apiKey = _config["Supabase:AnonKey"]; // Pega do secrets

            client.DefaultRequestHeaders.Add("apikey", apiKey);

            var response = await client.PostAsJsonAsync(supabaseUrl, new
            {
                email = request.Email,
                password = request.Password
            });

            if (!response.IsSuccessStatusCode)
            //return Unauthorized("E-mail ou senha inválidos no Supabase.");
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                //_logger.LogError("Erro do Supabase: {Error}", errorBody); // Se tiver logger
                return StatusCode((int)response.StatusCode, $"Supabase diz: {errorBody}");
            }

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var token = doc.RootElement.GetProperty("access_token").GetString();

            return Ok(new { accessToken = token });
        }
    }
}
