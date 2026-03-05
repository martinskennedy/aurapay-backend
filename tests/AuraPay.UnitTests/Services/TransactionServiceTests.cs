using AuraPay.Application.DTOs;
using AuraPay.Application.Services;
using AuraPay.Domain.Entities;
using AuraPay.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.UnitTests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<TransactionService>> _loggerMock;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<TransactionService>>();

            _transactionService = new TransactionService(
                _accountRepositoryMock.Object,
                _transactionRepositoryMock.Object,
                _userRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        // Transferência _ Deve concluir com sucesso quando os saldos forem válidos
        public async Task TransferAsync_ShouldSucceed_WhenBalancesAreValid()
        {
            // Arrange
            var originUserId = Guid.NewGuid();
            var originAcc = new Account(Guid.NewGuid(), originUserId, 500m); // Saldo de 500
            var destAcc = new Account(Guid.NewGuid(), Guid.NewGuid(), 100m); // Saldo de 100

            var request = new TransferRequestDto(destAcc.AccountNumber, 200m);

            _accountRepositoryMock.Setup(r => r.GetByUserIdAsync(originUserId)).ReturnsAsync(originAcc);
            _accountRepositoryMock.Setup(r => r.GetByAccountNumberAsync(request.DestinationAccountNumber)).ReturnsAsync(destAcc);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _transactionService.TransferAsync(originUserId, request);

            // Assert
            result.Should().BeTrue();
            originAcc.Balance.Should().Be(300m); // 500 - 200
            destAcc.Balance.Should().Be(300m);   // 100 + 200

            // Verifica se criou as duas notas fiscais (Transactions)
            _transactionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Exactly(2));
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            _loggerMock.VerifyLogMustContain("concluída com sucesso", LogLevel.Information);
        }

        [Fact]
        // Transferência _ Deve lançar InvalidOperationException quando o saldo for insuficiente
        public async Task TransferAsync_ShouldThrowInvalidOperationException_WhenBalanceIsInsufficient()
        {
            // Arrange
            var originUserId = Guid.NewGuid();
            var originAcc = new Account(Guid.NewGuid(), originUserId, 50m); // Só tem 50
            var destAcc = new Account(Guid.NewGuid(), Guid.NewGuid(), 0m);

            var request = new TransferRequestDto("99999", 100m); // Tenta transferir 100

            _accountRepositoryMock.Setup(r => r.GetByUserIdAsync(originUserId)).ReturnsAsync(originAcc);
            _accountRepositoryMock.Setup(r => r.GetByAccountNumberAsync(It.IsAny<string>())).ReturnsAsync(destAcc);

            // Act
            var act = () => _transactionService.TransferAsync(originUserId, request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Saldo insuficiente."); // Mensagem vinda da Entidade Account

            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never); // Nunca deve salvar se falhou saldo

            _loggerMock.VerifyLogMustContain("Transferência negada", LogLevel.Warning);
        }

        [Fact]
        // Transferência _ Deve lançar InvalidOperationException quando tentar transferir para a própria conta
        public async Task TransferAsync_ShouldThrowException_WhenTransferringToSameAccount()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var accId = Guid.NewGuid();
            var account = new Account(accId, userId, 1000m);

            var request = new TransferRequestDto(account.AccountNumber, 100m);

            _accountRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(account);
            _accountRepositoryMock.Setup(r => r.GetByAccountNumberAsync(account.AccountNumber)).ReturnsAsync(account);

            // Act
            var act = () => _transactionService.TransferAsync(userId, request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Não é possível transferir para a própria conta.");

            _loggerMock.VerifyLogMustContain("Tentativa de transferência para a própria conta", LogLevel.Warning);
        }

        [Fact]
        // Transferência _ Deve lançar KeyNotFoundException quando a conta de destino não for encontrada
        public async Task TransferAsync_ShouldThrowKeyNotFoundException_WhenDestinationAccountNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var originAcc = new Account(Guid.NewGuid(), userId, 100m);

            _accountRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(originAcc);
            _accountRepositoryMock.Setup(r => r.GetByAccountNumberAsync(It.IsAny<string>()))
                .ReturnsAsync((Account)null);

            var request = new TransferRequestDto("00000", 10m);

            // Act
            var act = () => _transactionService.TransferAsync(userId, request);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Conta de destino não encontrada.");

            _loggerMock.VerifyLogMustContain("Transferência abortada: Conta de destino", LogLevel.Warning);
        }
    }
}
