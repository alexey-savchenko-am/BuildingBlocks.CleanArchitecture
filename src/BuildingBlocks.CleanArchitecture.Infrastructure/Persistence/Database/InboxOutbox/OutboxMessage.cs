using System.ComponentModel.DataAnnotations.Schema;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.InboxOutbox;

[Table("outbox_messages")]
public sealed class OutboxMessage
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [Column("message_type")]
    public MessageType MessageType { get; set; } = MessageType.DomainEvent;

    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("occured_on_utc")]
    public DateTime OccuredOnUtc { get; set; }

    [Column("processed_on_utc")]
    public DateTime? ProcessedOnUtc { get; set; }

    [Column("error")]
    public string? Error { get; set; }

    [Column("retry_count")]
    public int RetryCount { get; set; }
}
