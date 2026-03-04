using AuraPay.Domain.Entities;
using AuraPay.Domain.Interfaces;
using AuraPay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Infrastructure.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly AuraPayDbContext _context;

        public CardRepository(AuraPayDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Card card)
        {
            await _context.Cards.AddAsync(card);
        }

        public async Task<IEnumerable<Card>> GetByAccountIdAsync(Guid accountId)
        {
            return await _context.Cards
                .Where(c => c.AccountId == accountId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Card?> GetByIdAsync(Guid id)
        {
            return await _context.Cards.FindAsync(id);
        }

        public void Update(Card card)
        {
            _context.Cards.Update(card);
        }
    }
}
