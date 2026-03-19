using AuraPay.Application.DTOs;
using AuraPay.Application.Interfaces;
using AuraPay.Domain.Entities;
using AuraPay.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IAccountRepository accountRepository, IUnitOfWork unitOfWork, ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AccountDto> CreateAccountAsync(Guid userId)
        {
            _logger.LogInformation("Tentativa de criação de conta iniciada para o usuário: {UserId}", userId);

            // 1. Verificar se o utilizador já tem uma conta
            var existingAccount = await _accountRepository.GetByUserIdAsync(userId);
            if (existingAccount != null)
            {
                _logger.LogWarning("Falha ao criar conta: Usuário {UserId} já possui a conta {AccountNumber}.", userId, existingAccount.AccountNumber);
                throw new InvalidOperationException("O utilizador já possui uma conta ativa.");
            }

            // 2. Gerar um número de conta único (Simulação)
            string accountNumber;
            bool isDuplicate;

            // Para garantir a unicidade, verificamos se o número gerado já existe no banco de dados
            do
            {
                accountNumber = GenerateAccountNumber();
                isDuplicate = await _accountRepository.GetByAccountNumberAsync(accountNumber) != null;
            } while (isDuplicate);

            // 3. Criar a nova entidade de conta
            var newAccount = new Account(userId, accountNumber);

            try
            {
                // 4. Persistir no banco de dados via Repositório e UoW
                await _accountRepository.AddAsync(newAccount);

                var success = await _unitOfWork.CommitAsync();

                if (success <= 0)
                {
                    _logger.LogError("Erro crítico ao persistir a conta para o usuário {UserId} no banco de dados.", userId);
                    throw new Exception("Não foi possível salvar a conta. Tente novamente mais tarde.");
                }

                _logger.LogInformation("Conta {AccNumber} criada com sucesso para o usuário {UserId}.", accountNumber, userId);

                // 5. Retornar o DTO para a API
                return new AccountDto(
                    newAccount.Id,
                    newAccount.AccountNumber,
                    newAccount.Balance,
                    newAccount.CreatedAt
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar conta para o usuário {UserId}.", userId);
                throw; // Re-throw para que a camada superior possa lidar com a resposta adequada
            }
        }

        public async Task<AccountDto?> GetBalanceAsync(Guid userId)
        {
            _logger.LogInformation("Usuário {UserId} consultando saldo.", userId);

            var account = await _accountRepository.GetByUserIdAsync(userId);

            if (account == null)
            {
                _logger.LogWarning("Consulta de saldo falhou: Conta não encontrada para o usuário {UserId}.", userId);
                return null;
            }

            return new AccountDto(
                account.Id,
                account.AccountNumber,
                account.Balance,
                account.CreatedAt
            );
        }

        private string GenerateAccountNumber()
        {
            // Gera um número aleatório de 6 dígitos para a conta
            return new Random().Next(100000, 999999).ToString();
        }
    }
}
