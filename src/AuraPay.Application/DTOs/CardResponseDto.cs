using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.DTOs
{
    public record CardResponseDto(
            Guid Id,
            string CardHolderName,
            string LastFourDigits, // Ex: "**** **** **** 1234"
            string ExpiryDate,     // Formatado como "MM/yy"
            bool IsActive
        );
}
