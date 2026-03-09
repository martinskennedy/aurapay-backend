using AuraPay.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Interfaces
{
    public interface ICurrencyExchangeService
    {
        Task<decimal> GetLiveRateAsync(Currency from, Currency to);
    }
}
