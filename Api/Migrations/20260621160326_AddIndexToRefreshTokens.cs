using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_UserId",
                table: "Books");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_UserId",
                table: "Books",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Books_UserId",
                table: "Books");

            migrationBuilder.CreateIndex(
                name: "IX_Books_UserId",
                table: "Books",
                column: "UserId");
        }
    }
}
