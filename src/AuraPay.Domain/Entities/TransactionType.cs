using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Domain.Entities
{
    public enum TransactionType
    {
        Deposit = 1,
        Withdraw = 2,
        TransferIn = 3,
        TransferOut = 4
    }
}
