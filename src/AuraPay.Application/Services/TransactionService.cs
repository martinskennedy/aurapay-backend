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
    public class TransactionService : ITransactionService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(IAccountRepository accountRepository, ITransactionRepository transactionRepository, IUnitOfWork unitOfWork, ILogger<TransactionService> logger)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> TransferAsync(Guid originUserId, TransferRequestDto request)
        {
            // 1. Log de Auditoria Inicial
            _logger.LogInformation("Iniciando transferência: Origem {UserId}, Destino {AccNumber}, Valor {Amount}",
                originUserId, request.DestinationAccountNumber, request.Amount);

            // 2. Buscar conta de origem pelo UserId (quem está logado)
            var originAccount = await _accountRepository.GetByUserIdAsync(originUserId);
            if (originAccount == null) throw new Exception("Conta de origem não encontrada.");

            // 3. Buscar conta de destino pelo numero
            var destinationAccount = await _accountRepository.GetByAccountNumberAsync(request.DestinationAccountNumber);
            if (destinationAccount == null) throw new Exception("Conta de destino não encontrada.");

            // 4. Impedir transferência para si mesmo
            if (originAccount.Id == destinationAccount.Id)
                throw new InvalidOperationException("Não é possível transferir para a própria conta.");

            // 5. Executar a lógica de negócio (Regras de Domínio)
            // Valida se há saldo suficiente
            originAccount.Withdraw(request.Amount);
            destinationAccount.Deposit(request.Amount);

            // 6. Criar os registros de extrato (Auditoria)
            var debitNote = new Transaction(originAccount.Id, request.Amount, TransactionType.TransferOut);
            var creditNote = new Transaction(destinationAccount.Id, request.Amount, TransactionType.TransferIn);

            await _transactionRepository.AddAsync(debitNote);
            await _transactionRepository.AddAsync(creditNote);

            // 5. Persistir as mudanças no banco de forma ATÔMICA
            // O CommitAsync salva as duas contas e as duas transações ou NADA caso ocorra algum erro.
            var result = await _unitOfWork.CommitAsync();

            _logger.LogInformation("Transferência concluída com sucesso. Transação ID: {DebitId}", debitNote.Id);

            return result > 0;
        }
    }
}
