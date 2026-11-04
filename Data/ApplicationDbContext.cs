using Chats24h.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chats24h.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>(); // property get only

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ChatRoom>()
            .HasIndex(room => room.ExpiresAt);
    }
}