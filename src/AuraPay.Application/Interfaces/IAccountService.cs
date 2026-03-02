using AuraPay.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Interfaces
{
    public interface IAccountService
    {
        Task<AccountDto> CreateAccountAsync(Guid userId);
        Task<AccountDto?> GetBalanceAsync(Guid userId);
    }
}
