using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Domain.Entities
{
    public class Account
    {
        public Guid Id { get; private set; }
        public string AccountNumber { get; private set; }
        public decimal Balance { get; private set; }
        public Guid UserId { get; private set; } // Referência ao usuário do Supabase Auth
        public DateTime CreatedAt { get; private set; }

        // Construtor para o EF Core
        protected Account() { }

        public Account(Guid userId, string accountNumber)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            AccountNumber = accountNumber;
            Balance = 0;
            CreatedAt = DateTime.UtcNow;
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("O valor do depósito deve ser positivo.");
            Balance += amount;
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("O valor do saque deve ser positivo.");
            if (Balance < amount) throw new InvalidOperationException("Saldo insuficiente.");
            Balance -= amount;
        }
    }
}
