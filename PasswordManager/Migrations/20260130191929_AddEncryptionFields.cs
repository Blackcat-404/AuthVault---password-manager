using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasswordManager.Migrations
{
    /// <inheritdoc />
    public partial class AddEncryptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.AddColumn<byte[]>(
                name: "AuthHash",
                table: "Users",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "AuthSalt",
                table: "Users",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "EncryptionSalt",
                table: "Users",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordLastChangedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoteIV",
                table: "NoteData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoginIV",
                table: "LoginData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoteIV",
                table: "LoginData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordIV",
                table: "LoginData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardNumberIV",
                table: "CardData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpireMonthIV",
                table: "CardData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpireYearIV",
                table: "CardData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoteIV",
                table: "CardData",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuthSalt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EncryptionSalt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordLastChangedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NoteIV",
                table: "NoteData");

            migrationBuilder.DropColumn(
                name: "LoginIV",
                table: "LoginData");

            migrationBuilder.DropColumn(
                name: "NoteIV",
                table: "LoginData");

            migrationBuilder.DropColumn(
                name: "PasswordIV",
                table: "LoginData");

            migrationBuilder.DropColumn(
                name: "CardNumberIV",
                table: "CardData");

            migrationBuilder.DropColumn(
                name: "ExpireMonthIV",
                table: "CardData");

            migrationBuilder.DropColumn(
                name: "ExpireYearIV",
                table: "CardData");

            migrationBuilder.DropColumn(
                name: "NoteIV",
                table: "CardData");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
