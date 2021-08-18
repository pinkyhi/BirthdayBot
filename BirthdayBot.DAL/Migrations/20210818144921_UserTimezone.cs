using Microsoft.EntityFrameworkCore.Migrations;

namespace BirthdayBot.DAL.Migrations
{
    public partial class UserTimezone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Timezone_DstOffset",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Timezone_RawOffset",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timezone_TimeZoneId",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timezone_TimeZoneName",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timezone_DstOffset",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Timezone_RawOffset",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Timezone_TimeZoneId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Timezone_TimeZoneName",
                table: "Users");
        }
    }
}
