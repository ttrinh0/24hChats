using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Chats24h.Models;

[PrimaryKey(nameof(UserId), nameof(RoomId))]
public class ChatMember
{

    [ForeignKey(nameof(ChatUser))]
    public string UserId { get; set; } = null!;
    [ForeignKey(nameof(ChatRoom))]
    public Guid RoomId { get; set; }
    public DateTimeOffset JoinedAt { get; set; } = DateTime.UtcNow;

    public ChatUser ChatUser { get; set; } = null!;
    public ChatRoom ChatRoom { get; set; } = null!;
}