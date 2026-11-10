using System;

namespace Chats24h.Models;

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoomId { get; set; }
    public string UserId { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTimeOffset SentAt { get; set; } = DateTime.UtcNow;

    //Navigation
    public ChatUser User { get; set; } = null!;
    public ChatRoom ChatRoom { get; set; } = null!;

}
