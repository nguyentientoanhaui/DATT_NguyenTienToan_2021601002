using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Demo.Migrations
{
    /// <inheritdoc />
    public partial class FixChatbotDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ChatSessions_SessionId",
                table: "ChatMessages");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ChatSessions_SessionId",
                table: "ChatSessions");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_IsFromUser",
                table: "ChatMessages");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "ChatMessages",
                newName: "SenderName");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ChatMessages",
                newName: "SenderId");

            migrationBuilder.RenameColumn(
                name: "IsFromUser",
                table: "ChatMessages",
                newName: "IsRead");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ChatMessages",
                newName: "Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_ChatMessages_CreatedAt",
                table: "ChatMessages",
                newName: "IX_ChatMessages_Timestamp");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastActivityAt",
                table: "ChatSessions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "LastMessage",
                table: "ChatSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ChatSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "SessionId",
                table: "ChatMessages",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "MessageType",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderType",
                table: "ChatMessages",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderType",
                table: "ChatMessages",
                column: "SenderType");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ChatSessions_SessionId",
                table: "ChatMessages",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ChatSessions_SessionId",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_SenderType",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "LastMessage",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "MessageType",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "SenderType",
                table: "ChatMessages");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "ChatMessages",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "SenderName",
                table: "ChatMessages",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "ChatMessages",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "ChatMessages",
                newName: "IsFromUser");

            migrationBuilder.RenameIndex(
                name: "IX_ChatMessages_Timestamp",
                table: "ChatMessages",
                newName: "IX_ChatMessages_CreatedAt");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastActivityAt",
                table: "ChatSessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "ChatMessages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ChatSessions_SessionId",
                table: "ChatSessions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_IsFromUser",
                table: "ChatMessages",
                column: "IsFromUser");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ChatSessions_SessionId",
                table: "ChatMessages",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
