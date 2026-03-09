using AuraPay.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Interfaces
{
    public interface IInternationalTransactionService
    {
        // Gera a simulação para o usuário ver as taxas
        Task<InternationalTransferPreviewDto> CreatePreviewAsync(decimal amountBrl);

        // Executa a transferência internacional de facto
        Task<bool> ExecuteTransferAsync(Guid userId, InternationalTransferRequestDto request);
    }
}
