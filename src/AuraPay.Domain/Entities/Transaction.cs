using AuraPay.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; private set; }
        public Guid AccountId { get; private set; }
        public decimal Amount { get; private set; }
        public TransactionType Type { get; private set; }
        public DateTime Timestamp { get; private set; }

        public virtual Account Account { get; private set; }

        protected Transaction() { }

        public Transaction(Guid accountId, decimal amount, TransactionType type)
        {
            Id = Guid.NewGuid();
            AccountId = accountId;
            Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
            Type = type;
            Timestamp = DateTime.UtcNow;
        }
    }
}
