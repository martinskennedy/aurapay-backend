using AuraPay.Application.DTOs;
using AuraPay.Domain.Entities;
using AuraPay.IntegrationTests.Config;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.IntegrationTests.Controllers
{
    public class TransactionControllerTests : BaseIntegrationTest
    {
        private readonly HttpClient _client;

        public TransactionControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
            // O CreateClient simula o navegador ou o Postman fazendo a chamada
            _client = _factory.CreateClient();
        }

        [Fact]
        // Testa o endpoint de transferência com uma requisição válida
        public async Task Transfer_ShouldReturnOk_WhenRequestIsValid()
        {
            // 1. Arrange
            var userId = Guid.NewGuid();
            TestAuthHandler.UserId = userId; // Define o usuário autenticado para o teste

            // Simulação de um hash de senha para o teste
            string mockHash = BCrypt.Net.BCrypt.HashPassword("senha123");

            // Usando o construtor internal visível via InternalsVisibleTo
            var userA = new User("Usuário Teste", "teste@aurapay.com", "12345678900", mockHash);

            // Forçamos o ID do objeto para bater com o do Token do TestAuthHandler
            typeof(User).GetProperty("Id")?.SetValue(userA, userId);

            var accA = new Account(userId, "777777");
            accA.Deposit(1000m); // Garante saldo para a transferência

            var accB = new Account(Guid.NewGuid(), "888888");

            // Adiciona tudo ao contexto de uma vez
            _context.Users.Add(userA);
            _context.Accounts.AddRange(accA, accB);

            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear(); // Força o Controller a ler do banco, não do cache

            var request = new TransferRequestDto(accB.AccountNumber, 200m);

            // 2. Act
            var response = await _client.PostAsJsonAsync("/api/transactions/transfer", request);

            // 3. Assert
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // Isso ajuda muito a debugar se o erro for 401 ou 404
                throw new Exception($"Erro na API: {response.StatusCode} - {errorContent}");
            }

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verificação extra: o saldo no banco realmente mudou?
            var accAInDb = _context.Accounts.First(a => a.Id == accA.Id);
            accAInDb.Balance.Should().Be(800m);
        }

        [Fact]
        // Testa o endpoint de transferência com um valor inválido (negativo)
        public async Task Transfer_ShouldReturnBadRequest_WhenAmountIsZeroOrNegative()
        {
            // 1. Arrange - Valor inválido para o FluentValidation pegar
            var request = new TransferRequestDto("12345", -10m);

            // 2. Act
            var response = await _client.PostAsJsonAsync("/api/transactions/transfer", request);

            // 3. Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
