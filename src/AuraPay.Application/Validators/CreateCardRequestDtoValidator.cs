using AuraPay.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Validators
{
    public class CreateCardRequestValidator : AbstractValidator<CreateCardRequest>
    {
        public CreateCardRequestValidator()
        {
            RuleFor(x => x.HolderName)
                .NotEmpty().WithMessage("O nome do titular é obrigatório.")
                .MinimumLength(3).WithMessage("O nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(50).WithMessage("O nome é muito longo.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("O nome do titular não deve conter números ou caracteres especiais.");
        }
    }
}
