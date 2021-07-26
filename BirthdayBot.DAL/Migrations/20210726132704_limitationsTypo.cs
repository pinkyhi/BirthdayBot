using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BirthdayBot.DAL.Migrations
{
    public partial class limitationsTypo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserLimitations_LocationChangeDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserLimitations_StartLocationInputBlock",
                table: "Users");

            migrationBuilder.AddColumn<DateTime>(
                name: "UserLimitations_LocationChangeBlockDate",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UserLimitations_StartLocationInputBlockDate",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserLimitations_LocationChangeBlockDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserLimitations_StartLocationInputBlockDate",
                table: "Users");

            migrationBuilder.AddColumn<DateTime>(
                name: "UserLimitations_LocationChangeDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UserLimitations_StartLocationInputBlock",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }
    }
}
