using Chats24h.Data;
using Chats24h.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chats24h.Pages;

public class ChatModel(ApplicationDbContext db) : PageModel
{
    public ChatRoom Room { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid? roomId)
    {
        if (roomId is null)
        {
            return RedirectToPage("/Index");
        }

        var now = DateTimeOffset.UtcNow;

        var room = await db.ChatRooms
            .AsNoTracking()
            .SingleOrDefaultAsync(room =>
                room.Id == roomId.Value &&
                room.ExpiresAt > now);

        if (room is null)
        {
            return NotFound("This room does not exist or has expired.");
        }

        Room = room;
        return Page();
    }
}