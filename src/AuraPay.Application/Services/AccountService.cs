using AuraPay.Application.DTOs;
using AuraPay.Application.Interfaces;
using AuraPay.Domain.Entities;
using AuraPay.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Services
{
    internal class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
        {
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AccountDto> CreateAccountAsync(Guid userId)
        {
            // 1. Verificar se o utilizador já tem uma conta
            var existingAccount = await _accountRepository.GetByUserIdAsync(userId);
            if (existingAccount != null)
                throw new Exception("O utilizador já possui uma conta ativa.");

            // 2. Gerar um número de conta único (Simulação)
            // Num cenário real, isto viria de um serviço de sequenciação ou regra de negócio
            var accountNumber = GenerateAccountNumber();

            // 3. Criar a nova entidade de conta
            var newAccount = new Account(userId, accountNumber);

            // 4. Persistir no banco de dados via Repositório e UoW
            await _accountRepository.AddAsync(newAccount);

            // 5. Retornar o DTO para a API
            return new AccountDto(
                newAccount.Id,
                newAccount.AccountNumber,
                newAccount.Balance,
                newAccount.CreatedAt
            );
        }

        public async Task<AccountDto?> GetBalanceAsync(Guid userId)
        {
            var account = await _accountRepository.GetByUserIdAsync(userId);

            if (account == null) return null;

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
