using System.ComponentModel.DataAnnotations;

namespace Chats24h.Models;

public class ChatRoom
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(80)]
    public string Name { get; set; } = null!;
    [MaxLength(160)]
    public string Description { get; set; } = "";
    public string OwnerId { get; set; } = null!;
    public string Theme { get; set; } = "";
    public ICollection<ChatMember> ChatMembers { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public ChatUser Owner { get; set; } = null!;
}