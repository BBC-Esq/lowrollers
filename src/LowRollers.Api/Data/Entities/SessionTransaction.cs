namespace LowRollers.Api.Data.Entities;

public class SessionTransaction
{
    public Guid SessionTransactionId { get; set; }
    public Guid SessionId { get; set; }
    public Guid PlayerId { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public bool IsCredit { get; set; }
    public int Amount { get; set; }

    public GameSession GameSession { get; set; } = null!;
    public Player Player { get; set; } = null!;
}
