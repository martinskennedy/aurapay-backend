using AuraPay.Application.DTOs;
using AuraPay.Application.Interfaces;
using AuraPay.Domain.Entities;
using AuraPay.Domain.Interfaces;
using AutoMapper.Configuration;
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

        public UserService(IUserRepository userRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserDto> RegisterUserAsync(CreateUserRequestDto request, Guid externalId)
        {
            // 1. Verificar se o usuário já existe para evitar duplicidade
            var existingUser = await _userRepository.GetByExternalIdAsync(externalId);
            if (existingUser != null) throw new Exception("Usuário já sincronizado.");

            // 2. Criar a entidade User
            var newUser = new User(request.FullName, request.Email, request.Document, externalId);
            await _userRepository.AddAsync(newUser);

            // 3. CRIAR A CONTA AUTOMATICAMENTE
            // Gerar um número de conta aleatório simples para o teste
            var randomAcc = new Random().Next(100000, 999999).ToString();
            var account = new Account(newUser.Id, randomAcc);

            // Adicionar saldo inicial para testes
            account.Deposit(500);

            await _accountRepository.AddAsync(account);

            // 4. Salvar tudo (User + Account) numa única transação
            await _unitOfWork.CommitAsync();

            return new UserDto(newUser.Id, newUser.FullName, newUser.Email, newUser.Document);
        }

        public async Task<UserDto?> GetByExternalIdAsync(Guid externalId)
        {
            var user = await _userRepository.GetByExternalIdAsync(externalId);
            if (user == null) return null;

            return new UserDto(user.Id, user.FullName, user.Email, user.Document);
        }       
    }
}
