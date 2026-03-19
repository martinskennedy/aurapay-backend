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

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);

                // Garante que não existam dois usuários com o mesmo e-mail ou CPF
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Document).IsUnique();

                entity.Property(u => u.FullName).IsRequired().HasMaxLength(150);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");
                entity.HasKey(t => t.Id);

                // Força o EF a usar a coluna AccountId que já existe no banco
                entity.HasOne(t => t.Account)
                      .WithMany() // Se a Account não tiver uma lista de Transactions, deixe vazio
                      .HasForeignKey(t => t.AccountId)
                      .IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
