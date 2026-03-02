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
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AuraPayDbContext _context;

        public TransactionRepository(AuraPayDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
        }

        public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
        }
    }
}
