using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chats24h.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserChatPreferencesSupport1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Colour",
                table: "AspNetUsers");
        }
    }
}
