
using System;

namespace TransactionService.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }

        public Guid TargetAccount { get; set; }

        public decimal Amount { get; set; }

        public Guid Owner { get; set; }
    }
}