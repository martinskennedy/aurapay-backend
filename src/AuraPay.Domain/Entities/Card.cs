using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("AuraPay.UnitTests")]

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

        public Card() { }

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

        // Construtor INTERNAL para Testes
        internal Card(Guid id, Guid accountId, string holderName, string cardNumber, string cvv)
        {
            Id = id;
            AccountId = accountId; // Adicione esta linha se não estiver lá
            CardHolderName = holderName;
            CardNumber = cardNumber;
            CVV = cvv;
        }
    }
}
