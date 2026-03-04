using AuraPay.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<bool> TransferAsync(Guid originUserId, TransferRequestDto request);
        Task<IEnumerable<TransactionResponseDto>> GetHistoryAsync(Guid userId);
    }
}
