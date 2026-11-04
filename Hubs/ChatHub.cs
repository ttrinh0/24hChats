using Chats24h.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chats24h.Hubs;

public class ChatHub(ApplicationDbContext db) : Hub
{
    private const string RoomKey = "JoinedRoom";

    private static string GroupName(Guid roomId) => $"room:{roomId}";

    public async Task JoinRoom(Guid roomId)
    {
        await RequireActiveRoomAsync(roomId);

        if (Context.Items.TryGetValue(RoomKey, out var previous)
            && previous is Guid previousRoomId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                GroupName(previousRoomId));
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GroupName(roomId));

        Context.Items[RoomKey] = roomId;
    }

    public async Task SendMessage(string nickname, string message)
    {
        if (!Context.Items.TryGetValue(RoomKey, out var value)
            || value is not Guid roomId)
        {
            throw new HubException("Join a room first.");
        }

        nickname = nickname?.Trim() ?? "";
        message = message?.Trim() ?? "";

        if (nickname.Length is < 1 or > 40)
        {
            throw new HubException("Use a nickname of 1–40 characters.");
        }

        if (message.Length is < 1 or > 2000)
        {
            throw new HubException("Messages must contain 1–2000 characters.");
        }

        await RequireActiveRoomAsync(roomId);

        await Clients.Group(GroupName(roomId)).SendAsync(
            "ReceiveMessage",
            nickname,
            message);
    }

    private async Task RequireActiveRoomAsync(Guid roomId)
    {
        var now = DateTimeOffset.UtcNow;

        var exists = await db.ChatRooms.AnyAsync(room =>
            room.Id == roomId &&
            room.ExpiresAt > now);

        if (!exists)
        {
            throw new HubException("This room has expired or no longer exists.");
        }
    }
}