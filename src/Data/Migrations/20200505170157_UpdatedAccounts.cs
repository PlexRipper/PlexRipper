using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class UpdatedAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "Accounts");

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "PlexAccounts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Accounts",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Accounts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsValidated",
                table: "Accounts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidatedAt",
                table: "Accounts",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "PlexServers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccessToken = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Port = table.Column<int>(nullable: false),
                    Version = table.Column<string>(nullable: true),
                    Scheme = table.Column<string>(nullable: true),
                    Host = table.Column<string>(nullable: true),
                    LocalAddresses = table.Column<string>(nullable: true),
                    MachineIdentifier = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    Owned = table.Column<bool>(nullable: false),
                    Synced = table.Column<bool>(nullable: false),
                    SourceTitle = table.Column<string>(nullable: true),
                    OwnerId = table.Column<long>(nullable: false),
                    Home = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlexAccountServers",
                columns: table => new
                {
                    PlexAccountId = table.Column<long>(nullable: false),
                    PlexServerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexAccountServers", x => new { x.PlexAccountId, x.PlexServerId });
                    table.ForeignKey(
                        name: "FK_PlexAccountServers_PlexAccounts_PlexAccountId",
                        column: x => x.PlexAccountId,
                        principalTable: "PlexAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexAccountServers_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlexAccounts_AccountId",
                table: "PlexAccounts",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexAccountServers_PlexServerId",
                table: "PlexAccountServers",
                column: "PlexServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexAccounts_Accounts_AccountId",
                table: "PlexAccounts",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexAccounts_Accounts_AccountId",
                table: "PlexAccounts");

            migrationBuilder.DropTable(
                name: "PlexAccountServers");

            migrationBuilder.DropTable(
                name: "PlexServers");

            migrationBuilder.DropIndex(
                name: "IX_PlexAccounts_AccountId",
                table: "PlexAccounts");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "PlexAccounts");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsValidated",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ValidatedAt",
                table: "Accounts");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
