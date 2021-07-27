using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BirthdayBot.DAL.Migrations
{
    public partial class limitTypo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserLimitations_LocationChangeAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserLimitations_LocationChangeBlockDate",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "UserLimitations_ChangeLocationInputAttempts",
                table: "Users",
                nullable: true,
                defaultValue: 3);

            migrationBuilder.AddColumn<DateTime>(
                name: "UserLimitations_ChangeLocationInputBlockDate",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserLimitations_ChangeLocationInputAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserLimitations_ChangeLocationInputBlockDate",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "UserLimitations_LocationChangeAttempts",
                table: "Users",
                type: "int",
                nullable: true,
                defaultValue: 3);

            migrationBuilder.AddColumn<DateTime>(
                name: "UserLimitations_LocationChangeBlockDate",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }
    }
}
