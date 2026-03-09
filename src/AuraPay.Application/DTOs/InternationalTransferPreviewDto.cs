using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.DTOs
{
    public record InternationalTransferPreviewDto(
            decimal OriginalAmountBrl, // Valor que o usuário quer enviar (ex: R$ 1000)
            decimal ExchangeRate,      // Cotação (ex: 5.40)
            decimal IofAmount,         // Imposto (ex: R$ 11,00)
            decimal ServiceFee,        // Nossa taxa (ex: R$ 20,00)
            decimal TotalToDeductBrl,  // O que sai da conta (1000 + 11 + 20)
            decimal FinalAmount     // O que chega no destino (1000 / 5.40)
        );
}
