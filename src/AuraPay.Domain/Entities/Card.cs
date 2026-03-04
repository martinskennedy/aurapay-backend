using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Domain.Entities
{
    public class Card
    {
        public Guid Id { get; private set; }
        public Guid AccountId { get; private set; }
        public string CardHolderName { get; private set; } // Nome impresso no cartão
        public string CardNumber { get; private set; }     // 16 dígitos (mascarados ou criptografados)
        public string CVV { get; private set; }            // Código de segurança
        public DateTime ExpiryDate { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected Card() { }

        public Card(Guid accountId, string holderName, string cardNumber, string cvv)
        {
            Id = Guid.NewGuid();
            AccountId = accountId;
            CardHolderName = holderName.ToUpper();
            CardNumber = cardNumber;
            CVV = cvv;
            ExpiryDate = DateTime.UtcNow.AddYears(5); // Validade padrão de 5 anos
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void Deactivate() => IsActive = false;

        public void Activate() => IsActive = true;
    }
}
