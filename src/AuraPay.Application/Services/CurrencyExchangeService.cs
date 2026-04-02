using AuraPay.Application.Interfaces;
using AuraPay.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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

        public async Task<decimal> GetLiveRateAsync(Currency from, Currency to)
        {
            var client = _httpClientFactory.CreateClient();

            var baseCurrency = from.ToString();
            var targetCurrency = to.ToString();

            var url = $"https://open.er-api.com/v6/latest/{baseCurrency}";

            try
            {
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Erro ao consultar cotação na open.er-api. Status: {Status}. URL: {Url}",
                        response.StatusCode,
                        url
                    );

                    throw new HttpRequestException("Não foi possível obter a cotação em tempo real.");
                }

                var content = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (root.TryGetProperty("result", out var resultElement))
                {
                    var result = resultElement.GetString();

                    if (!string.Equals(result, "success", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogError(
                            "A open.er-api retornou resultado inválido. URL: {Url}. Conteúdo: {Content}",
                            url,
                            content
                        );

                        throw new InvalidOperationException("A API de câmbio retornou uma resposta inválida.");
                    }
                }

                if (!root.TryGetProperty("rates", out var ratesElement))
                {
                    _logger.LogError(
                        "A resposta da open.er-api não contém 'rates'. URL: {Url}. Conteúdo: {Content}",
                        url,
                        content
                    );

                    throw new InvalidOperationException("Formato de resposta da API de câmbio inválido.");
                }

                if (!ratesElement.TryGetProperty(targetCurrency, out var rateElement))
                {
                    _logger.LogError(
                        "A moeda de destino {TargetCurrency} não foi encontrada na resposta. URL: {Url}. Conteúdo: {Content}",
                        targetCurrency,
                        url,
                        content
                    );

                    throw new InvalidOperationException("A moeda de destino não foi encontrada na resposta da API de câmbio.");
                }

                var rate = rateElement.GetDecimal();

                _logger.LogInformation(
                    "Cotação obtida com sucesso. Base: {BaseCurrency}. Destino: {TargetCurrency}. Taxa: {Rate}",
                    baseCurrency,
                    targetCurrency,
                    rate
                );

                return rate;
            }
            catch (JsonException ex)
            {
                _logger.LogCritical(ex, "Erro ao interpretar resposta da open.er-api. URL: {Url}", url);
                throw new InvalidOperationException("Erro ao processar a resposta da API de câmbio.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogCritical(ex, "Falha na comunicação com serviço de câmbio externo. URL: {Url}", url);
                throw;
            }
        }
    }
}