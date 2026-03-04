using AuraPay.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Interfaces
{
    public interface ICardService
    {
        // Cria um cartão virtual para o usuário logado
        Task<CardResponseDto> CreateVirtualCardAsync(Guid userId, string holderName);

        // Lista todos os cartões da conta do usuário
        Task<IEnumerable<CardResponseDto>> GetMyCardsAsync(Guid userId);

        // Bloquear/Desbloquear Cartão
        Task<bool> ToggleCardStatusAsync(Guid cardId, Guid userId);

        // Mostra dados sensíveis do cartão (número, CVV, validade) - apenas para o titular do cartão
        Task<CardSensitiveDataDto?> GetSensitiveDataAsync(Guid cardId, Guid userId);
    }
}
