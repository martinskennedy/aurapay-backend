using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.DTOs
{
    // O que o usuário envia para realizar a transferência de fato
    public record InternationalTransferRequestDto(
        decimal AmountBrl,
        string BeneficiaryName,
        string SwiftCode,
        string Iban,
        string BankName
    );
}
