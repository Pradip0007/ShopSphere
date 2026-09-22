using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE Users
                SET DisplayName = SUBSTRING(Email, 1, CHARINDEX('@', Email) - 1)
                WHERE DisplayName = ''
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DisplayName",
                table: "Users",
                column: "DisplayName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_DisplayName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");
        }
    }
}
