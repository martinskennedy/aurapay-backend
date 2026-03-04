using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.DTOs
{
    public record CardSensitiveDataDto(
          string CardHolderName,
          string CardNumber,
          string CVV,
          string ExpiryDate
      );
}
