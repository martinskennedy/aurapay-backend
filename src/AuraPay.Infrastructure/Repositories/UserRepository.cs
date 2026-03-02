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
    public class UserRepository : IUserRepository
    {
        private readonly AuraPayDbContext _context;

        public UserRepository(AuraPayDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByExternalIdAsync(Guid externalId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.ExternalId == externalId);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }        
    }
}
