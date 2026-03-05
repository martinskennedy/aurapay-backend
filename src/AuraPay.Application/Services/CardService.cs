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
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CardService> _logger;

        public CardService(ICardRepository cardRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork, ILogger<CardService> logger)
        {
            _cardRepository = cardRepository;
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CardResponseDto> CreateVirtualCardAsync(Guid userId, string holderName)
        {
            _logger.LogInformation("Solicitação de novo cartão virtual para o usuário {UserId}.", userId);

            var account = await _accountRepository.GetByUserIdAsync(userId);
            if (account == null)
            {
                _logger.LogWarning("Falha ao criar cartão: Conta não encontrada para o usuário {UserId}.", userId);
                throw new KeyNotFoundException("Conta não encontrada.");
            }
                
            // Gera dados do cartão
            var random = new Random();
            var number = $"4000{random.Next(1000, 9999)}{random.Next(1000, 9999)}{random.Next(1000, 9999)}";
            var cvv = random.Next(100, 999).ToString();

            var card = new Card(account.Id, holderName, number, cvv);

            await _cardRepository.AddAsync(card);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Cartão virtual {CardId} criado com sucesso para a conta {AccountId}.", card.Id, account.Id);

            return MapToDto(card);
        }

        public async Task<CardSensitiveDataDto?> GetSensitiveDataAsync(Guid cardId, Guid userId)
        {
            _logger.LogWarning("ALERTA DE SEGURANÇA: Usuário {UserId} solicitando exibição de dados sensíveis do cartão {CardId}.", userId, cardId);

            // 1. Buscar conta do usuário
            var account = await _accountRepository.GetByUserIdAsync(userId);
            if (account == null) return null;

            // 2. Buscar o cartão
            var card = await _cardRepository.GetByIdAsync(cardId);

            // 3. Verifica se o cartão existe e pertence a esta conta
            if (card == null || card.AccountId != account.Id)
            {
                _logger.LogCritical("TENTATIVA DE ACESSO INDEVIDO: Usuário {UserId} tentou acessar cartão {CardId} que não lhe pertence!", userId, cardId);
                throw new UnauthorizedAccessException("Você não tem permissão para visualizar este cartão.");
            }

            _logger.LogInformation("Dados sensíveis do cartão {CardId} revelados com sucesso para o titular.", cardId);

            // 4. Retornar os dados reais (sem máscara)
            return new CardSensitiveDataDto(
                card.CardHolderName,
                card.CardNumber,
                card.CVV,
                card.ExpiryDate.ToString("MM/yy")
            );
        }

        public async Task<IEnumerable<CardResponseDto>> GetMyCardsAsync(Guid userId)
        {
            _logger.LogInformation("Usuário {UserId} listando seus cartões.", userId);

            var account = await _accountRepository.GetByUserIdAsync(userId);
            if (account == null) return Enumerable.Empty<CardResponseDto>();

            var cards = await _cardRepository.GetByAccountIdAsync(account.Id);
            return cards.Select(MapToDto);
        }

        public async Task<bool> ToggleCardStatusAsync(Guid cardId, Guid userId)
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null)
            {
                _logger.LogWarning("Tentativa de alterar status de cartão inexistente: {CardId}", cardId);
                return false;
            }

            // Validação de segurança: o cartão pertence à conta do usuário logado?
            var account = await _accountRepository.GetByUserIdAsync(userId);
            if (card.AccountId != account?.Id)
            {
                _logger.LogCritical("Tentativa não autorizada de alteração de status do cartão {CardId} pelo usuário {UserId}.", cardId, userId);
                throw new UnauthorizedAccessException();
            }

            if (card.IsActive) card.Deactivate();
            else card.Activate();

            _cardRepository.Update(card);
            var success = await _unitOfWork.CommitAsync() > 0;

            if (success) _logger.LogInformation("Status do cartão {CardId} alterado com sucesso.", cardId);

            return success;
        }

        private CardResponseDto MapToDto(Card card)
        {
            return new CardResponseDto(
                card.Id,
                card.CardHolderName,
                $"**** **** **** {card.CardNumber.Substring(card.CardNumber.Length - 4)}",
                card.ExpiryDate.ToString("MM/yy"),
                card.IsActive
            );
        }
    }
}
