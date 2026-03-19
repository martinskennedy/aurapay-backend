using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("AuraPay.IntegrationTests")]

namespace AuraPay.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public string Document { get; private set; } // CPF no Brasil
        public string PasswordHash { get; private set; } // O ID que vem do Supabase (Auth)

        public Account? Account { get; private set; }

        protected User() { }

        public User(string fullName, string email, string document, string passwordHash)
        {
            Id = Guid.NewGuid();
            FullName = fullName;
            Email = email;
            Document = document;
            PasswordHash = passwordHash;
        }

        // Construtor para Testes
        internal User(Guid id, string fullName, string email, string document, string passwordHash)
        {
            Id = id;
            FullName = fullName;
            Email = email;
            Document = document;
            PasswordHash = passwordHash;
        }

        // Método para atualizar a senha no futuro(ex: Esqueci minha senha)
        public void UpdatePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("O hash da senha não pode ser vazio.");

            PasswordHash = newPasswordHash;
        }
    }
}
