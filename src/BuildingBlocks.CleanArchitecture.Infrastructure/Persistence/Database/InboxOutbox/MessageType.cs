using System.Text.Json.Serialization;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.InboxOutbox;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageType
{
    DomainEvent,
    IntegrationEvent
}