using System.ComponentModel.DataAnnotations;
using Chats24h.Data;
using Chats24h.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chats24h.Pages;

[Authorize]
public class CreateModel(ApplicationDbContext db, UserManager<ChatUser> userManager) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Enter a room name.")]
    [StringLength(80)]
    public string RoomName { get; set; } = "";
    [BindProperty]
    [StringLength(160)]
    public string? RoomDesc { get; set; }
    private readonly UserManager<ChatUser> _userManager = userManager;

    public async Task<IActionResult> OnPostAsync()
    {
        RoomName = RoomName?.Trim() ?? "";
        RoomDesc = RoomDesc?.Trim() ?? "";
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(RoomName))
        {
            ModelState.AddModelError(
                nameof(RoomName), "Enter a room name.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var now = DateTimeOffset.UtcNow;

        var room = new ChatRoom
        {
            Name = RoomName,
            Description = RoomDesc,
            OwnerId = currentUser.Id,
            CreatedAt = now,
            ExpiresAt = now.AddHours(24)
        };
        var chatMember = new ChatMember
        {
            UserId = currentUser.Id,
            RoomId = room.Id,
        };
        room.ChatMembers.Add(chatMember);
        db.ChatRooms.Add(room);
        await db.SaveChangesAsync();
        return RedirectToPage("/CreateSuccess", new { roomId = room.Id });
    }
}
