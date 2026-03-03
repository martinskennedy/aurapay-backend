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
    public class AccountRepository : IAccountRepository
    {
        private readonly AuraPayDbContext _context;

        public AccountRepository(AuraPayDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByIdAsync(Guid id)
        {
            return await _context.Accounts.FindAsync(id);
        }

        public async Task<Account?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId); // UserId vem do Supabase Auth
        }

        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
        }

        public void Update(Account account)
        {
            _context.Accounts.Update(account);
        }

        public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }
    }
}
