using AuraPay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Infrastructure.Data
{
    public class AuraPayDbContext : DbContext
    {
        public AuraPayDbContext(DbContextOptions<AuraPayDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Card> Cards { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuraPayDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
