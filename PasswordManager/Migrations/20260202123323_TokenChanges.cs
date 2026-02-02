using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasswordManager.Migrations
{
    /// <inheritdoc />
    public partial class TokenChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationCode",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "EmailVerificationExpiresAt",
                table: "Users",
                newName: "TokenExpiresAt");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "TwoFactorAuthentications",
                newName: "TokenExpiresAt");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "TwoFactorAuthentications",
                newName: "Token");

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Token",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "TokenExpiresAt",
                table: "Users",
                newName: "EmailVerificationExpiresAt");

            migrationBuilder.RenameColumn(
                name: "TokenExpiresAt",
                table: "TwoFactorAuthentications",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "Token",
                table: "TwoFactorAuthentications",
                newName: "Code");

            migrationBuilder.AddColumn<int>(
                name: "EmailVerificationCode",
                table: "Users",
                type: "int",
                nullable: true);
        }
    }
}
