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
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CardService(ICardRepository cardRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
        {
            _cardRepository = cardRepository;
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CardResponseDto> CreateVirtualCardAsync(Guid userId, string holderName)
        {
            var account = await _accountRepository.GetByUserIdAsync(userId);
            if (account == null) throw new KeyNotFoundException("Conta não encontrada.");

            // Gera dados do cartão
            var random = new Random();
            var number = $"4000{random.Next(1000, 9999)}{random.Next(1000, 9999)}{random.Next(1000, 9999)}";
            var cvv = random.Next(100, 999).ToString();

            var card = new Card(account.Id, holderName, number, cvv);

            await _cardRepository.AddAsync(card);
            await _unitOfWork.CommitAsync();

            return MapToDto(card);
        }

        public async Task<CardSensitiveDataDto?> GetSensitiveDataAsync(Guid cardId, Guid userId)
        {
            // 1. Buscar conta do usuário
            var account = await _accountRepository.GetByUserIdAsync(userId);
            if (account == null) return null;

            // 2. Buscar o cartão
            var card = await _cardRepository.GetByIdAsync(cardId);

            // 3. Verifica se o cartão existe e pertence a esta conta
            if (card == null || card.AccountId != account.Id)
                throw new UnauthorizedAccessException("Você não tem permissão para visualizar este cartão.");

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
            var account = await _accountRepository.GetByUserIdAsync(userId);
            if (account == null) return Enumerable.Empty<CardResponseDto>();

            var cards = await _cardRepository.GetByAccountIdAsync(account.Id);
            return cards.Select(MapToDto);
        }

        public async Task<bool> ToggleCardStatusAsync(Guid cardId, Guid userId)
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null) return false;

            // Validação de segurança: o cartão pertence à conta do usuário logado?
            var account = await _accountRepository.GetByUserIdAsync(userId);
            if (card.AccountId != account?.Id) throw new UnauthorizedAccessException();

            if (card.IsActive)
            {
                card.Deactivate();
            }
            else { card.Activate(); }

            _cardRepository.Update(card);
            return await _unitOfWork.CommitAsync() > 0;
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
