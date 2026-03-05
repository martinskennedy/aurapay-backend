using AuraPay.Application.Services;
using AuraPay.Domain.Entities;
using AuraPay.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.UnitTests.Services
{
    public class CardServiceTests
    {
        private readonly Mock<ICardRepository> _cardRepositoryMock;
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<CardService>> _loggerMock;
        private readonly CardService _cardService;

        public CardServiceTests()
        {
            _cardRepositoryMock = new Mock<ICardRepository>();
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<CardService>>();

            _cardService = new CardService(
                _cardRepositoryMock.Object,
                _accountRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        // Criar cartão virtual _ Deve criar e retornar o cartão quando a conta existir
        public async Task CreateVirtualCardAsync_ShouldCreateAndReturnCard_WhenAccountExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var holderName = "João Silva";

            // Usamos o construtor internal da Account que já configuramos
            var account = new Account(accountId, userId);

            _accountRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync(account);

            // Configuramos o Unit of Work para retornar que 1 linha foi afetada
            _unitOfWorkMock.Setup(uow => uow.CommitAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _cardService.CreateVirtualCardAsync(userId, holderName);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.CardHolderName.Should().Be(holderName.ToUpper());
            result.LastFourDigits.Should().StartWith("****");
            result.LastFourDigits.Should().HaveLength(19); // "**** **** **** 1234" tem 19 caracteres
            result.ExpiryDate.Should().MatchRegex(@"^\d{2}/\d{2}$"); // Valida o formato MM/yy
            result.IsActive.Should().BeTrue();

            // Verificação de Persistência:
            // 1. Garante que o cartão foi passado para o repositório
            _cardRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Card>(c =>
                c.AccountId == accountId &&
                c.CardHolderName == holderName.ToUpper()
            )), Times.Once);

            // 2. Garante que o banco de dados recebeu o comando de salvar
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);

            // 3. Verifica se o log de sucesso foi gerado
            _loggerMock.VerifyLogMustContain("criado com sucesso", LogLevel.Information);
        }

        [Fact]
        // Criar cartão virtual _ Deve lançar uma exceção de chave não encontrada quando a conta não existir.
        public async Task CreateVirtualCardAsync_ShouldThrowKeyNotFoundException_WhenAccountDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _accountRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync((Account)null);

            // Act
            var act = () => _cardService.CreateVirtualCardAsync(userId, "Titular");

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Conta não encontrada.");

            _cardRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Card>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Never);
        }

        [Fact]
        // Obter dados confidenciais _ Deve lançar exceção de não autorizado quando o usuário não for o proprietário
        public async Task GetSensitiveDataAsync_ShouldThrowUnauthorized_WhenUserIsNotTheOwner()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userAccountId = Guid.NewGuid();
            var otherAccountId = Guid.NewGuid();
            var cardId = Guid.NewGuid();

            // Mock da conta do usuário que está logado
            var userAccount = new Account(userAccountId, userId);
            _accountRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync(userAccount);

            // Mock de um cartão que pertence a OUTRA conta (otherAccountId)
            var cardOfOtherUser = new Card(cardId, otherAccountId, "OUTRO TITULAR", "1234123412341234", "123");
            _cardRepositoryMock.Setup(repo => repo.GetByIdAsync(cardId))
                .ReturnsAsync(cardOfOtherUser);

            // Act
            var act = () => _cardService.GetSensitiveDataAsync(cardId, userId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Você não tem permissão para visualizar este cartão.");

            // Verifica se o log CRITICAL foi chamado
            _loggerMock.VerifyLogMustContain("TENTATIVA DE ACESSO INDEVIDO", LogLevel.Critical);
        }

        [Fact]
        // Obter dados confidenciais _ Deve retornar dados quando o usuário for o proprietário
        public async Task GetSensitiveDataAsync_ShouldReturnData_WhenUserIsTheOwner()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var cardId = Guid.NewGuid();
            var expectedNumber = "4000123456789012";

            var account = new Account(accountId, userId);
            var card = new Card(cardId, accountId, "MEU NOME", expectedNumber, "999");

            _accountRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(account);
            _cardRepositoryMock.Setup(repo => repo.GetByIdAsync(cardId)).ReturnsAsync(card);

            // Act
            var result = await _cardService.GetSensitiveDataAsync(cardId, userId);

            // Assert
            result.Should().NotBeNull();
            result.CardNumber.Should().Be(expectedNumber);
            result.CVV.Should().Be("999");
        }
    }
}