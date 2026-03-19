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

        // Método para registrar um novo usuário com uma conta associada
        public async Task<UserDto> RegisterUserAsync(CreateUserRequestDto request, string password)
        {
            _logger.LogInformation("Iniciando registro para o e-mail: {Email}", request.Email);

            // 1. Verificar se o usuário já existe para evitar duplicidade
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Tentativa de registro duplicado ignorada. E-mail: {E-mail}", request.Email);
                throw new InvalidOperationException("Este E-mail já está cadastrado.");
            }

            // 2. Gerar o Hash da senha (Segurança de produção)
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            try
            {
                // 3. Criar a entidade User com o Hash
                var newUser = new User(request.FullName, request.Email, request.Document, passwordHash);
                await _userRepository.AddAsync(newUser);

                // 4. Criar a conta com bônus
                // Gerar um número de conta aleatório simples para o teste
                var randomAcc = new Random().Next(100000, 999999).ToString();
                var account = new Account(newUser.Id, randomAcc);

                // Adicionar saldo inicial para testes
                _logger.LogInformation("Gerando conta inicial {AccNumber} com bônus para o novo usuário.", randomAcc);

                account.Deposit(5000);

                await _accountRepository.AddAsync(account);

                // 5. Salvar tudo (User + Account) em uma única transação
                var result = await _unitOfWork.CommitAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Usuário {UserId} e conta {AccNumber} registrados com sucesso.", newUser.Id, randomAcc);
                }
                else
                {
                    _logger.LogError("Falha ao persistir dados do usuário no banco local. E-mail: {E-mail}", request.Email);
                }

                return new UserDto(newUser.Id, newUser.FullName, newUser.Email, newUser.Document);

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "ERRO CRÍTICO ao registrar usuário com EE-mail: {E-mail}", request.Email);
                throw new InvalidOperationException("Erro ao salvar os dados do usuário", ex);
            }
        }

        // Método para buscar um usuário pelo Id
        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Buscando usuário pelo Id: {Id}", id);

            var user = await _userRepository.GetByIdAsync(id);

            if (user == null) return null;

            return new UserDto(user.Id, user.FullName, user.Email, user.Document);
        }

        // Método para buscar um usuário pelo e-mail
        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            _logger.LogInformation("Buscando usuário local para o e-mail: {Email}", email);
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Usuário não encontrado para o e-mail: {Email}", email);
                return null;
            }

            return new UserDto(user.Id, user.FullName, user.Email, user.Document);
        }

        // Método para validar as credenciais do usuário
        public async Task<UserDto?> ValidateUserAsync(string email, string password)
        {
            _logger.LogInformation("Validando credenciais para o e-mail: {Email}", email);

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return null;

            // Compara a senha digitada com o Hash salvo no banco
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!isPasswordValid)
            {
                _logger.LogWarning("Senha inválida para o usuário: {Email}", email);
                return null;
            }

            return new UserDto(user.Id, user.FullName, user.Email, user.Document);
        }
    }
}
