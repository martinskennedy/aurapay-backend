using AuraPay.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Validators
{
    public class CreateUserRequestDtoValidator : AbstractValidator<CreateUserRequestDto>
    {
        public CreateUserRequestDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("O nome completo é obrigatório.")
                .MinimumLength(3).WithMessage("O nome deve ter pelo menos 3 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("O formato do e-mail é inválido.");

            RuleFor(x => x.Document)
                .NotEmpty().WithMessage("O documento (CPF) é obrigatório.")
                .Matches(@"^\d{11}$").WithMessage("O documento deve conter exatamente 11 dígitos numéricos.");
        }
    }
}
