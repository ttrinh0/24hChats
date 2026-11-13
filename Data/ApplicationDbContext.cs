using Chats24h.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chats24h.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ChatUser>(options)
{
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>(); // property get only
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatUser> ChatUsers => Set<ChatUser>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ChatRoom>()
            .HasIndex(room => room.ExpiresAt);

        builder.Entity<ChatRoom>()
            .HasOne(m => m.Owner)
            .WithMany()
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ChatMessage>()
        .HasOne(m => m.ChatRoom)
        .WithMany()
        .HasForeignKey(m => m.RoomId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ChatMessage>()
        .HasOne(m => m.User)
        .WithMany()
        .HasForeignKey(m => m.UserId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}