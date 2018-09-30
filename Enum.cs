namespace TransactionService
{
    public enum TransactionStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum TransactionType
    {
        Payment,
        Deposit
    }
}