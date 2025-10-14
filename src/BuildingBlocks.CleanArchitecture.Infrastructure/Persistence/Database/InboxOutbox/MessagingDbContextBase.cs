using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.InboxOutbox;

public abstract class MessagingDbContextBase: DbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }

    public MessagingDbContextBase(DbContextOptions options)
        : base(options)
    { }
}
