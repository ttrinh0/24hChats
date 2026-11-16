using System;
using Microsoft.AspNetCore.Identity;

namespace Chats24h.Models;

public class ChatUser : IdentityUser
{
    public string Nickname { get; set; } = "";
    public string Colour { get; set; } = "";
}
