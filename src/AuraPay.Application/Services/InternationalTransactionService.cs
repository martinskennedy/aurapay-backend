using AuraPay.Application.DTOs;
using AuraPay.Application.Interfaces;
using AuraPay.Domain.Entities;
using AuraPay.Domain.Enums;
using AuraPay.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Services
{
    public class InternationalTransactionService : IInternationalTransactionService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICurrencyExchangeService _exchangeService;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InternationalTransactionService> _logger;

        // Constantes de negócio para fácil manutenção
        private const decimal IofRate = 0.011m; // 1.1%
        private const decimal AuraPayFee = 20.00m; // Taxa fixa em BRL

        public InternationalTransactionService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            ICurrencyExchangeService exchangeService,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ILogger<InternationalTransactionService> logger)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _exchangeService = exchangeService;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<InternationalTransferPreviewDto> CreatePreviewAsync(decimal amountBrl)
        {
            // Busca cotação real da AwesomeAPI
            var rate = await _exchangeService.GetLiveRateAsync(Currency.USD, Currency.BRL);

            var iof = amountBrl * IofRate;
            var totalToDeduct = amountBrl + iof + AuraPayFee;
            var final = amountBrl / rate;

            return new InternationalTransferPreviewDto(
                OriginalAmountBrl: amountBrl,
                ExchangeRate: rate,
                IofAmount: iof,
                ServiceFee: AuraPayFee,
                TotalToDeductBrl: totalToDeduct,
                FinalAmount: Math.Round(final, 2)
            );
        }

        public async Task<bool> ExecuteTransferAsync(Guid externalUserId, InternationalTransferRequestDto request)
        {
            // 1. ExternalId (Supabase) para o User local
            var user = await _userRepository.GetByExternalIdAsync(externalUserId);
            if (user == null) throw new KeyNotFoundException("Usuário não sincronizado no sistema local.");

            _logger.LogInformation("Iniciando Remessa Internacional: Usuário {UserId}, Valor {Amount} BRL", user.Id, request.AmountBrl);

            // 2. Buscamos a conta usando o ID interno (user.Id)
            var account = await _accountRepository.GetByUserIdAsync(user.Id);
            if (account == null) throw new KeyNotFoundException("Conta de origem não encontrada.");

            try
            {
                // Reutiliza a lógica do Preview para garantir que as taxas batam
                var preview = await CreatePreviewAsync(request.AmountBrl);

                decimal totalToDeduct = Math.Round(preview.TotalToDeductBrl, 2);

                // Executa o saque do valor TOTAL (valor enviado + taxas)
                // O método Withdraw da sua Entidade Account já valida saldo insuficiente
                account.Withdraw(totalToDeduct);

                // No portfólio, registramos a transação como uma saída (TransferOut)
                // No futuro, você pode criar um TransactionType.InternationalTransfer
                var transactionEntry = new Transaction(account.Id, totalToDeduct, TransactionType.TransferOut);

                await _transactionRepository.AddAsync(transactionEntry);

                // Persistência atômica: salva a alteração do saldo e o log da transação
                var result = await _unitOfWork.CommitAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Remessa enviada com sucesso. Total debitado: {Total} BRL. Destino: {Iban}",
                        totalToDeduct, request.Iban);
                    return true;
                }

                return false;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Remessa negada por regras de negócio: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Erro fatal no processamento da remessa internacional.");
                throw;
            }
        }
    }
}
