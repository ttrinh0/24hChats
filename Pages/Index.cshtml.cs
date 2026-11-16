using System.Security.Claims;
using Chats24h.Data;
using Chats24h.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chats24h.Pages;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    public List<ChatRoom> MyRooms { get; private set; } = [];
    public List<ChatRoom> RecentRooms { get; private set; } = [];
    public List<ChatRoom> OpenRooms { get; private set; } = [];
    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return;
        var now = DateTimeOffset.UtcNow;
        var active = db.ChatRooms.AsNoTracking().Include(r => r.Owner).Where(r => r.ExpiresAt > now);
        MyRooms = await active.Where(r => r.OwnerId == userId).OrderByDescending(r => r.CreatedAt).Take(3).ToListAsync();
        RecentRooms = await db.ChatRooms.AsNoTracking().Include(r => r.Owner)
            .Where(r => r.ExpiresAt > now && r.ChatMembers.Any(m => m.UserId == userId))
            .OrderByDescending(r => r.ChatMembers.Where(m => m.UserId == userId).Select(m => m.JoinedAt).First())
            .Take(3).ToListAsync();
        OpenRooms = await active.OrderByDescending(r => r.CreatedAt).Take(6).ToListAsync();
    }
}
