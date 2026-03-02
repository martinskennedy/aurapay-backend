using AuraPay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Infrastructure.Data.Mappings
{
    public class AccountMapping : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.AccountNumber).IsRequired().HasMaxLength(20);
            builder.Property(a => a.Balance).HasPrecision(18, 2);
            builder.Property(a => a.UserId).IsRequired();

            // Relacionamento 1:N (Uma conta tem muitas transações)
            builder.HasMany<Transaction>()
                   .WithOne()
                   .HasForeignKey(t => t.AccountId);
        }
    }
}
