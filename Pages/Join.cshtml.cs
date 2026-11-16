using System.Security.Claims;
using Chats24h.Data;
using Chats24h.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chats24h.Pages;

[Authorize]
public class JoinModel(ApplicationDbContext db) : PageModel
{
    public List<ChatRoom> RecentRooms { get; private set; } = [];
    public List<ChatRoom> OpenRooms { get; private set; } = [];
    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var now = DateTimeOffset.UtcNow;
        RecentRooms = await db.ChatRooms.AsNoTracking().Include(r => r.Owner)
            .Where(r => r.ExpiresAt > now && r.ChatMembers.Any(m => m.UserId == userId))
            .OrderByDescending(r => r.ChatMembers.Where(m => m.UserId == userId).Select(m => m.JoinedAt).First())
            .Take(3).ToListAsync();
        OpenRooms = await db.ChatRooms.AsNoTracking().Include(r => r.Owner)
            .Where(r => r.ExpiresAt > now).OrderByDescending(r => r.CreatedAt).ToListAsync();
    }
}
