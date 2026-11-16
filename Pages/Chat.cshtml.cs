using System.ComponentModel.DataAnnotations;
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
    [BindProperty]
    [Required(ErrorMessage = "Enter a room name.")]
    [StringLength(80)]
    public string RoomName { get; set; } = null!;
    [BindProperty]
    [StringLength(160)]
    public string RoomDesc { get; set; } = null!;
    public ChatRoom Room { get; private set; } = null!;
    public string ChatUserId { get; private set; } = null!;
    public string ChatUserColour { get; private set; } = null!;
    public List<ChatMessage> Messages { get; private set; } = [];
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
            .Include(c => c.ChatMembers)
            .ThenInclude(m => m.ChatUser)
            .AsNoTracking()
            .SingleOrDefaultAsync(room =>
                room.Id == roomId.Value &&
                room.ExpiresAt > now);
        if (room is null)
        {
            return NotFound("This room does not exist or has expired.");
        }

        var messages = await db.ChatMessages
        .Include(c => c.User)
        .Where(c => c.RoomId == roomId)
        .OrderBy(c => c.SentAt)
        .ToListAsync();

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Forbid();
        }
        var membership = await db.ChatMembers.FindAsync(user.Id, room.Id);
        if (membership is null)
        {
            membership = new ChatMember { UserId = user.Id, RoomId = room.Id, JoinedAt = now };
            db.ChatMembers.Add(membership);
            room.ChatMembers.Add(new ChatMember { UserId = user.Id, RoomId = room.Id, ChatUser = user });
        }
        else
        {
            membership.JoinedAt = now;
        }
        await db.SaveChangesAsync();
        RoomName = room.Name;
        RoomDesc = room.Description;
        Room = room;
        ChatUserId = user.Id;
        Messages = messages;
        ChatUserColour = user.Colour;
        return Page();
    }

    public async Task<IActionResult> OnPostEditAsync(Guid? roomId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var room = await db.ChatRooms.FindAsync(roomId);
        if (currentUser == null || room == null)
        {
            return NotFound();
        }
        Console.WriteLine($"\n\n1{RoomName}1\n\n");

        if (string.IsNullOrWhiteSpace(RoomName))
        {
            ModelState.AddModelError(
                nameof(RoomName), "Enter a room name.");
        }

        if (ModelState.IsValid)
        {
            room.Name = RoomName;
            room.Description = RoomDesc;
            db.ChatRooms.Update(room);

            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

}
