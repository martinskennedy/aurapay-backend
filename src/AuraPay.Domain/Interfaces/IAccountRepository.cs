using AuraPay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Domain.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(Guid id);
        Task<Account?> GetByUserIdAsync(Guid userId);
        Task AddAsync(Account account);
        void Update(Account account);
    }
}
