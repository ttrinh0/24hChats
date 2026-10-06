using System;
using Microsoft.AspNetCore.SignalR;

namespace Chats24h.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("RecievedMessage", user, message);
    }
}
