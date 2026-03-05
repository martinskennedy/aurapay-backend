using AuraPay.Application.DTOs;
using AuraPay.Application.Interfaces;
using AuraPay.Domain.Entities;
using AuraPay.Domain.Interfaces;
using AutoMapper.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UserDto> RegisterUserAsync(CreateUserRequestDto request, Guid externalId)
        {
            _logger.LogInformation("Iniciando processo de sincronização para ExternalId: {ExternalId}", externalId);

            // 1. Verificar se o usuário já existe para evitar duplicidade
            var existingUser = await _userRepository.GetByExternalIdAsync(externalId);
            if (existingUser != null)
            {
                _logger.LogWarning("Tentativa de registro duplicado ignorada. ExternalId: {ExternalId}", externalId);
                throw new Exception("Usuário já sincronizado.");
            }

            try
            {
                // 2. Criar a entidade User
                var newUser = new User(request.FullName, request.Email, request.Document, externalId);
                await _userRepository.AddAsync(newUser);

                // 3. CRIAR A CONTA AUTOMATICAMENTE
                // Gerar um número de conta aleatório simples para o teste
                var randomAcc = new Random().Next(100000, 999999).ToString();
                var account = new Account(newUser.Id, randomAcc);

                // Adicionar saldo inicial para testes
                _logger.LogInformation("Gerando conta inicial {AccNumber} com bônus para o novo usuário.", randomAcc);

                account.Deposit(5000);

                await _accountRepository.AddAsync(account);

                // 4. Salvar tudo (User + Account) numa única transação
                var result = await _unitOfWork.CommitAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Usuário {UserId} e conta {AccNumber} registrados com sucesso.", newUser.Id, randomAcc);
                }
                else
                {
                    _logger.LogError("Falha ao persistir dados do usuário no banco local. ExternalId: {ExternalId}", externalId);
                }

                return new UserDto(newUser.Id, newUser.FullName, newUser.Email, newUser.Document);

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "ERRO CRÍTICO ao registrar usuário com ExternalId: {ExternalId}", externalId);
                throw;
            }
        }

        public async Task<UserDto?> GetByExternalIdAsync(Guid externalId)
        {
            _logger.LogInformation("Buscando usuário local para o ExternalId: {ExternalId}", externalId);

            var user = await _userRepository.GetByExternalIdAsync(externalId);

            if (user == null)
            {
                _logger.LogWarning("Usuário não encontrado no banco local para o ExternalId: {ExternalId}", externalId);
                return null;
            }

            return new UserDto(user.Id, user.FullName, user.Email, user.Document);
        }       
    }
}
