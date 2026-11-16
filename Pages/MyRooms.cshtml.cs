using System.Security.Claims;
using Chats24h.Data;
using Chats24h.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chats24h.Pages;

[Authorize]
public class MyRoomsModel(ApplicationDbContext db) : PageModel
{
    public List<ChatRoom> Rooms { get; private set; } = [];
    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var now = DateTimeOffset.UtcNow;
        Rooms = await db.ChatRooms.AsNoTracking().Include(r => r.Owner)
            .Where(r => r.OwnerId == userId && r.ExpiresAt > now)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
    }
}
