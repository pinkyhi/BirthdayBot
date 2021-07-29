using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BirthdayBot.DAL.Migrations
{
    public partial class settings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Address_Component",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Long_Name = table.Column<string>(nullable: true),
                    Short_Name = table.Column<string>(nullable: true),
                    Types = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address_Component", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    BigFileUniqueId = table.Column<string>(nullable: true),
                    SmallFileUniqueId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    IsBot = table.Column<bool>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true),
                    LanguageCode = table.Column<string>(nullable: true),
                    CurrentStatus = table.Column<int>(nullable: true),
                    MiddlewareData = table.Column<string>(nullable: true),
                    RegistrationDate = table.Column<DateTime>(nullable: true),
                    BirthDate = table.Column<DateTime>(nullable: false),
                    Settings_BirthYearConfidentiality = table.Column<int>(nullable: true, defaultValue: 1),
                    Settings_BirthDateConfidentiality = table.Column<int>(nullable: true, defaultValue: 0),
                    Settings_StrongNotification_0 = table.Column<int>(nullable: true, defaultValue: 7),
                    Settings_StrongNotification_1 = table.Column<int>(nullable: true, defaultValue: 3),
                    Settings_StrongNotification_2 = table.Column<int>(nullable: true, defaultValue: 0),
                    Settings_CommonNotification_0 = table.Column<int>(nullable: true, defaultValue: 0),
                    Limitations_StartLocationInputAttempts = table.Column<int>(nullable: true, defaultValue: 5),
                    Limitations_StartLocationInputBlockDate = table.Column<DateTime>(nullable: true),
                    Limitations_ChangeLocationInputAttempts = table.Column<int>(nullable: true, defaultValue: 3),
                    Limitations_ChangeLocationInputBlockDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(nullable: false),
                    Formatted_Address = table.Column<string>(nullable: true),
                    Types = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Address_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMember",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    ChatId = table.Column<long>(nullable: false),
                    IsAnonymous = table.Column<bool>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMember", x => new { x.UserId, x.ChatId });
                    table.ForeignKey(
                        name: "FK_ChatMember_Chat_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMember_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Note",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    IsStrong = table.Column<bool>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Note", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Note_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscription",
                columns: table => new
                {
                    SubscriberId = table.Column<long>(nullable: false),
                    TargetId = table.Column<long>(nullable: false),
                    IsStrong = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => new { x.SubscriberId, x.TargetId });
                    table.ForeignKey(
                        name: "FK_Subscription_Users_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscription_Users_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Address_ComponentConnector",
                columns: table => new
                {
                    AddressId = table.Column<long>(nullable: false),
                    AddressComponentId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address_ComponentConnector", x => new { x.AddressComponentId, x.AddressId });
                    table.ForeignKey(
                        name: "FK_Address_ComponentConnector_Address_Component_AddressComponentId",
                        column: x => x.AddressComponentId,
                        principalTable: "Address_Component",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Address_ComponentConnector_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_UserId",
                table: "Address",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_ComponentConnector_AddressId",
                table: "Address_ComponentConnector",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMember_ChatId",
                table: "ChatMember",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_Note_UserId",
                table: "Note",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_TargetId",
                table: "Subscription",
                column: "TargetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address_ComponentConnector");

            migrationBuilder.DropTable(
                name: "ChatMember");

            migrationBuilder.DropTable(
                name: "Note");

            migrationBuilder.DropTable(
                name: "Subscription");

            migrationBuilder.DropTable(
                name: "Address_Component");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "Chat");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
