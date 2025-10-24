using System.ComponentModel.DataAnnotations.Schema;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.InboxOutbox;

[Table("inbox_messages")]
public sealed class InboxMessage
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("message_id")]
    public Guid MessageId { get; set; }

    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("received_on_utc")]
    public DateTime ReceivedOnUtc { get; set; }

    [Column("processed_on_utc")]
    public DateTime? ProcessedOnUtc { get; set; }

    [Column("error")]
    public string? Error { get; set; }

    [Column("retry_count")]
    public int RetryCount { get; set; }
}