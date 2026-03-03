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
