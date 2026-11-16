using System.Security.Claims;
using Chats24h.Data;
using Chats24h.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chats24h.Pages;

[Authorize]
public class CreateSuccessModel(ApplicationDbContext db) : PageModel
{
    public ChatRoom Room { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid roomId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var room = await db.ChatRooms.AsNoTracking()
            .SingleOrDefaultAsync(r => r.Id == roomId && r.OwnerId == userId && r.ExpiresAt > DateTimeOffset.UtcNow);
        if (room is null) return NotFound();
        Room = room;
        return Page();
    }
}
