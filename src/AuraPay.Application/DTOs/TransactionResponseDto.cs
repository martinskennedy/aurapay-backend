using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.DTOs
{
    public record TransactionResponseDto(
            Guid Id,
            decimal Amount,
            string Type, // "TransferIn" ou "TransferOut"
            DateTime Timestamp
        );
}
