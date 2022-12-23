using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BirthdayBot.DAL.Migrations
{
    public partial class notifiedFlags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastNotificationTime",
                table: "Subscription",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastNotificationTime",
                table: "Note",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastNotificationTime",
                table: "ChatMember",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastNotificationTime",
                table: "Subscription");

            migrationBuilder.DropColumn(
                name: "LastNotificationTime",
                table: "Note");

            migrationBuilder.DropColumn(
                name: "LastNotificationTime",
                table: "ChatMember");
        }
    }
}
