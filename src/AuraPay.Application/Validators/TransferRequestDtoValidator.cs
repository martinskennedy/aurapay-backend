using AuraPay.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Validators
{
    public class TransferRequestDtoValidator : AbstractValidator<TransferRequestDto>
    {
        public TransferRequestDtoValidator()
        {
            RuleFor(x => x.DestinationAccountNumber)
                .NotEmpty().WithMessage("O número da conta de destino é obrigatório.")
                .Length(6).WithMessage("O número da conta deve ter exatamente 6 dígitos.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("O valor da transferência deve ser superior a zero.");
        }
    }
}
