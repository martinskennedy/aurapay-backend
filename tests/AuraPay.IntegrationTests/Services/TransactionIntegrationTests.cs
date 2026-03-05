using AuraPay.Application.DTOs;
using AuraPay.Application.Interfaces;
using AuraPay.Domain.Entities;
using AuraPay.Infrastructure.Data;
using AuraPay.IntegrationTests.Config;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.IntegrationTests.Services
{
    public class TransactionIntegrationTests : BaseIntegrationTest
    {
        private readonly ITransactionService _transactionService;

        public TransactionIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
        {
            // Pegamos a instância REAL do serviço registrada no Program.cs
            _transactionService = _serviceProvider.GetRequiredService<ITransactionService>();
        }

        [Fact]
        // Testa a transferência bem-sucedida entre duas contas
        public async Task TransferAsync_ShouldUpdateDatabase_WhenSuccessful()
        {
            // 1. Arrange - Criar dados REAIS no banco em memória
            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();

            var accountA = new Account(userA, "11111");
            accountA.Deposit(500m); // Saldo inicial real

            var accountB = new Account(userB, "22222");

            _context.Accounts.AddRange(accountA, accountB);
            await _context.SaveChangesAsync();

            var request = new TransferRequestDto(accountB.AccountNumber, 200m);

            // 2. Act - Chama o serviço real que usa Repositories e UoW reais
            var result = await _transactionService.TransferAsync(userA, request);

            // 3. Assert - Verificar se o estado do BANCO mudou
            result.Should().BeTrue();

            // Criamos um novo escopo para garantir que estamos lendo do banco e não do cache do EF
            var accountAFromDb = _context.Accounts.First(a => a.Id == accountA.Id);
            var accountBFromDb = _context.Accounts.First(a => a.Id == accountB.Id);
            var transactions = _context.Transactions.ToList();

            accountAFromDb.Balance.Should().Be(300m);
            accountBFromDb.Balance.Should().Be(200m);

            // Verifica se as 2 transações (débito e crédito) foram persistidas
            transactions.Should().HaveCount(2);
            transactions.Should().Contain(t => t.AccountId == accountA.Id && t.Amount == 200m);
            transactions.Should().Contain(t => t.AccountId == accountB.Id && t.Amount == 200m);
        }

        [Fact]
        // Testa que, se ocorrer um erro (ex: saldo insuficiente), o estado do banco NÃO é alterado (atomicidade)
        public async Task TransferAsync_ShouldMaintainAtomicState_WhenDomainExceptionOccurs()
        {
            // 1. Arrange - Saldo inicial real
            var originUserId = Guid.NewGuid();
            var initialBalance = 100m;
            var transferAmount = 500m; // Valor MAIOR que o saldo para forçar erro

            var originAcc = new Account(originUserId, "88888");
            originAcc.Deposit(initialBalance);

            var destAcc = new Account(Guid.NewGuid(), "99999");

            _context.Accounts.AddRange(originAcc, destAcc);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear(); // Limpa o cache para forçar leitura do banco depois

            var request = new TransferRequestDto(destAcc.AccountNumber, transferAmount);

            // 2. Act
            // O TransactionService vai chamar originAccount.Withdraw(500), que lançará InvalidOperationException
            var act = () => _transactionService.TransferAsync(originUserId, request);

            // 3. Assert
            await act.Should().ThrowAsync<InvalidOperationException>();

            // 4. Verificação de Integridade
            // Criamos um novo escopo para garantir que estamos vendo o que o banco salvou de fato
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AuraPayDbContext>();

                var accountAfterFailure = db.Accounts.First(a => a.AccountNumber == "88888");
                var transactionsCount = db.Transactions.Count();

                // O saldo NÃO pode ter mudado, mesmo que o objeto em memória tenha sido alterado no Service
                accountAfterFailure.Balance.Should().Be(initialBalance);

                // Nenhuma transação de extrato deve ter sido persistida
                transactionsCount.Should().Be(0);
            }
        }
    }
}
