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
        private readonly IAccountService _accountService;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserRepository userRepository, IAccountService accountService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _accountService = accountService;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserDto> RegisterUserAsync(CreateUserRequestDto request)
        {
            // 1. Validar se utilizador já existe por email ou documento
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null) throw new Exception("Utilizador já registado.");

            // 2. Criar a entidade User
            var newUser = new User(request.FullName, request.Email, request.Document, request.ExternalId);

            await _userRepository.AddAsync(newUser);

            // 3. Criar automaticamente a conta para este novo utilizador
            // Passando o ID do NOSSO User, não o do Supabase
            await _accountService.CreateAccountAsync(newUser.Id);

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
