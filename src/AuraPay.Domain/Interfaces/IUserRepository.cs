using AuraPay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByExternalIdAsync(Guid externalId); // Para buscar pelo ID do Supabase
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
    }
}
