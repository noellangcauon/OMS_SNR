using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class TransactionWrapper
    {
        public IEnumerable<Transaction> Transactions { get; set; }
    }
}
