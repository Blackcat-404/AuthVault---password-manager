using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasswordManager.Migrations
{
    /// <inheritdoc />
    public partial class FolderIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardData_Folders_FolderId",
                table: "CardData");

            migrationBuilder.DropForeignKey(
                name: "FK_LoginData_Folders_FolderId",
                table: "LoginData");

            migrationBuilder.DropForeignKey(
                name: "FK_NoteData_Folders_FolderId",
                table: "NoteData");

            migrationBuilder.AlterColumn<int>(
                name: "FolderId",
                table: "NoteData",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "FolderId",
                table: "LoginData",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "FolderId",
                table: "CardData",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CardData_Folders_FolderId",
                table: "CardData",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LoginData_Folders_FolderId",
                table: "LoginData",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteData_Folders_FolderId",
                table: "NoteData",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardData_Folders_FolderId",
                table: "CardData");

            migrationBuilder.DropForeignKey(
                name: "FK_LoginData_Folders_FolderId",
                table: "LoginData");

            migrationBuilder.DropForeignKey(
                name: "FK_NoteData_Folders_FolderId",
                table: "NoteData");

            migrationBuilder.AlterColumn<int>(
                name: "FolderId",
                table: "NoteData",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FolderId",
                table: "LoginData",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FolderId",
                table: "CardData",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CardData_Folders_FolderId",
                table: "CardData",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoginData_Folders_FolderId",
                table: "LoginData",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NoteData_Folders_FolderId",
                table: "NoteData",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
