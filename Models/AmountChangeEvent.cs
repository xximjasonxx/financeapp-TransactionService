
namespace TransactionService.Models
{
    public class AmountChangeEvent
    {
        public string TargetAccountId { get; set; }
        public decimal ValueChangeAmount { get; set; }
    }
}