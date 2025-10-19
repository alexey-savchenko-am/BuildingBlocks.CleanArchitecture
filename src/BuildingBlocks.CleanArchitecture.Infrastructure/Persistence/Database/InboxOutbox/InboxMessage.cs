namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.InboxOutbox;

public sealed class InboxMessage
{
    public Guid Id { get; set; } 
    public Guid MessageId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime ReceivedOnUtc { get; set; } 
    public DateTime? ProcessedOnUtc { get; set; } 
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}