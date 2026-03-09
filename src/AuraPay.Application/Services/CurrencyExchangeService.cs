using AuraPay.Application.Interfaces;
using AuraPay.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AuraPay.Application.Services
{
    public class CurrencyExchangeService : ICurrencyExchangeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CurrencyExchangeService> _logger;

        public CurrencyExchangeService(IHttpClientFactory httpClientFactory, ILogger<CurrencyExchangeService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // Método para obter a cotação em tempo real usando a AwesomeAPI
        public async Task<decimal> GetLiveRateAsync(Currency from, Currency to)
        {
            var client = _httpClientFactory.CreateClient();

            // Exemplo de URL AwesomeAPI: https://economia.awesomeapi.com.br/last/USD-BRL
            var pair = $"{from}-{to}";
            var url = $"https://economia.awesomeapi.com.br/last/{pair}";

            try
            {
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro ao consultar cotação. Status: {Status}", response.StatusCode);
                    throw new Exception("Não foi possível obter a cotação em tempo real.");
                }

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                // A AwesomeAPI retorna um objeto com a chave do par, ex: "USDBRL"
                var key = $"{from}{to}";
                if (doc.RootElement.TryGetProperty(key, out var data))
                {
                    var bid = data.GetProperty("bid").GetString();
                    return decimal.Parse(bid!, System.Globalization.CultureInfo.InvariantCulture);
                }

                throw new Exception("Formato de resposta da API de câmbio inválido.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Falha na comunicação com serviço de câmbio externo.");
                throw;
            }
        }
    }
}
