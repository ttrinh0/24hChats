using System.ComponentModel.DataAnnotations;
using Chats24h.Data;
using Chats24h.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Chats24h.Pages;

public class JoinModel(ApplicationDbContext db) : PageModel
{
    public List<ChatRoom> Rooms { get; private set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Enter a room name.")]
    [StringLength(80)]
    public string RoomName { get; set; } = "";

    public async Task OnGetAsync() // 'Task' type is somewhat like Promise in .js
    {
        await LoadRoomsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        RoomName = RoomName?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(RoomName))
        {
            ModelState.AddModelError(
                nameof(RoomName), "Enter a room name.");
        }

        if (!ModelState.IsValid)
        {
            await LoadRoomsAsync();
            return Page();
        }

        var now = DateTimeOffset.UtcNow;

        var room = new ChatRoom
        {
            Name = RoomName,
            CreatedAt = now,
            ExpiresAt = now.AddHours(24)
        };

        db.ChatRooms.Add(room);
        await db.SaveChangesAsync();

        return RedirectToPage("/Chat", new { roomId = room.Id });
    }

    private async Task LoadRoomsAsync()
    {
        var now = DateTimeOffset.UtcNow;

        Rooms = await db.ChatRooms
            .AsNoTracking()
            .Where(room => room.ExpiresAt > now)
            .OrderByDescending(room => room.CreatedAt)
            .ToListAsync();
    }
}