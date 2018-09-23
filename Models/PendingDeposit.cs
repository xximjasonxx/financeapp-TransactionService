
namespace TransactionService.Models
{
    public class PendingDeposit
    {
        public string Initiator { get; set; }
        public string TargetAccount { get; set; }
        public string DepositImageUrl { get; set; }
    }
}