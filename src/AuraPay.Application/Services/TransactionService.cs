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
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(IAccountRepository accountRepository, ITransactionRepository transactionRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<TransactionService> logger)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
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
            if (originAccount == null)
            {
                _logger.LogWarning("Transferência abortada: Usuário {UserId} não possui conta.", originUserId);
                throw new KeyNotFoundException("Conta de origem não encontrada.");
            }

            // 3. Buscar conta de destino pelo numero
            var destinationAccount = await _accountRepository.GetByAccountNumberAsync(request.DestinationAccountNumber);
            if (destinationAccount == null)
            {
                _logger.LogWarning("Transferência abortada: Conta de destino {AccNumber} inexistente.", request.DestinationAccountNumber);
                throw new KeyNotFoundException("Conta de destino não encontrada.");
            }

            // 4. Impedir transferência para si mesmo
            if (originAccount.Id == destinationAccount.Id)
            {
                _logger.LogWarning("Tentativa de transferência para a própria conta. UserId: {UserId}", originUserId);
                throw new InvalidOperationException("Não é possível transferir para a própria conta.");
            }

            try
            {
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

                if (result > 0)
                {
                    _logger.LogInformation("Transferência concluída com sucesso. Origem: {Origem}, Destino: {Destino}, Valor: {Valor}",
                        originAccount.AccountNumber, destinationAccount.AccountNumber, request.Amount);

                    return true;
                }

                _logger.LogError("Falha ao salvar a transferência no banco de dados.");
                return false;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Transferência negada: {Motivo}. Origem: {UserId}", ex.Message, originUserId);
                throw; 
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "ERRO INESPERADO durante transferência do usuário {UserId}", originUserId);
                throw;
            }
        }

        public async Task<IEnumerable<TransactionResponseDto>> GetHistoryAsync(Guid userId)
        {
            _logger.LogInformation("Buscando histórico para o UserId: {UserId}", userId);

            // 1. Achar o usuário local pelo ID do Supabase
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuário não sincronizado.");

            // 2. Achar a conta do usuário
            var account = await _accountRepository.GetByUserIdAsync(user.Id);
            if (account == null) throw new KeyNotFoundException("Conta não encontrada.");

            // 3. Buscar as transações usando o ID da CONTA (que é o que está na tabela Transactions)
            var transactions = await _transactionRepository.GetByAccountIdAsync(account.Id);

            _logger.LogInformation("Histórico retornado: {Count} transações encontradas para a conta {AccId}",
                transactions.Count(), account.Id);

            return transactions.Select(t => new TransactionResponseDto(
                t.Id,
                Math.Round(t.Amount, 2),
                t.Type.ToString(),
                t.Timestamp
            ));
        }
    }
}
