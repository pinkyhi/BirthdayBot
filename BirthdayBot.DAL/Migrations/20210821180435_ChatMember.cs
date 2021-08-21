using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BirthdayBot.DAL.Migrations
{
    public partial class ChatMember : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                table: "ChatMember");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ChatMember");

            migrationBuilder.AddColumn<DateTime>(
                name: "AddingDate",
                table: "ChatMember",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddingDate",
                table: "ChatMember");

            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymous",
                table: "ChatMember",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ChatMember",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
