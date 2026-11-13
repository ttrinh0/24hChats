using Chats24h.Data;
using Chats24h.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chats24h.Pages;

[Authorize]
public class ChatModel(ApplicationDbContext db, UserManager<ChatUser> userManager) : PageModel
{
    public ChatRoom Room { get; private set; } = null!;
    public string Nickname { get; private set; } = null!;
    private readonly UserManager<ChatUser> _userManager = userManager;

    public async Task<IActionResult> OnGetAsync(Guid? roomId)
    {
        if (roomId is null)
        {
            return RedirectToPage("/Index");
        }

        var now = DateTimeOffset.UtcNow;

        var room = await db.ChatRooms
            .Include(c => c.Owner)
            .AsNoTracking()
            .SingleOrDefaultAsync(room =>
                room.Id == roomId.Value &&
                room.ExpiresAt > now);
        if (room is null)
        {
            return NotFound("This room does not exist or has expired.");
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            Forbid();
        }

        Room = room;
        Nickname = user?.Nickname ?? "Default";
        return Page();
    }
}