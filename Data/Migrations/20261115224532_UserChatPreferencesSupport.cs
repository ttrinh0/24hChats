using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chats24h.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserChatPreferencesSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "ChatRooms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Theme",
                table: "ChatRooms");
        }
    }
}
