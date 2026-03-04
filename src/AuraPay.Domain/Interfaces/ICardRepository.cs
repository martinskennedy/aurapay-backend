using AuraPay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Domain.Interfaces
{
    public interface ICardRepository
    {
        Task AddAsync(Card card);
        Task<IEnumerable<Card>> GetByAccountIdAsync(Guid accountId);
        Task<Card?> GetByIdAsync(Guid id);
        void Update(Card card);
    }
}
