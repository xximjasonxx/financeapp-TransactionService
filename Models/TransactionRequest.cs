
namespace TransactionService.Models
{
    public class TransactionRequest
    {
        public string Payee { get; set; }

        public string TargetAccount { get; set; }

        public decimal Amount { get; set; }
    }
}