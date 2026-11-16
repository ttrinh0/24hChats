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
    public string RoomName { get; set; } = "";
    [BindProperty]
    [StringLength(160)]
    public string RoomDesc { get; set; } = "";
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
        Room = room;
        ChatUserId = user.Id;
        Messages = messages;
        ChatUserColour = user.Colour;
        return Page();
    }

    public async Task<IActionResult> OnPostEditAsync(Guid id, ChatRoom chatRoom)
    {
        RoomName = RoomName?.Trim() ?? "";
        RoomDesc = RoomDesc?.Trim() ?? "";
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return NotFound();
        }
        // if (currentUser != chatRoom.)

        if (string.IsNullOrWhiteSpace(RoomName))
        {
            ModelState.AddModelError(
                nameof(RoomName), "Enter a room name.");
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
        if (ModelState.IsValid)
        {

            await db.ChatRooms.AddAsync(room);
            await db.ChatMembers.AddAsync(chatMember);
            await db.SaveChangesAsync();
        }

        return RedirectToPage("/Chat", new { roomId = room.Id });
    }

    private async Task LoadAsync(ChatRoom chatRoom)
    {
        // RoomName = userName;

        // Input = new InputModel
        // {
        //     PhoneNumber = phoneNumber,
        //     Nickname = user.Nickname,
        //     Colour = user.Colour,
        // };
    }

}