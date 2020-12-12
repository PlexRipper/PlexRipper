using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class UpdatedAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "ConfirmedAt",
                "Accounts");

            migrationBuilder.DropColumn(
                "IsConfirmed",
                "Accounts");

            migrationBuilder.AddColumn<int>(
                "AccountId",
                "PlexAccounts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                "DisplayName",
                "Accounts",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                "IsEnabled",
                "Accounts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                "IsValidated",
                "Accounts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                "ValidatedAt",
                "Accounts",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                "PlexServers",
                table => new
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
                    Home = table.Column<bool>(nullable: false),
                },
                constraints: table => { table.PrimaryKey("PK_PlexServers", x => x.Id); });

            migrationBuilder.CreateTable(
                "PlexAccountServers",
                table => new
                {
                    PlexAccountId = table.Column<long>(nullable: false),
                    PlexServerId = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexAccountServers", x => new { x.PlexAccountId, x.PlexServerId });
                    table.ForeignKey(
                        "FK_PlexAccountServers_PlexAccounts_PlexAccountId",
                        x => x.PlexAccountId,
                        "PlexAccounts",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_PlexAccountServers_PlexServers_PlexServerId",
                        x => x.PlexServerId,
                        "PlexServers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_PlexAccounts_AccountId",
                "PlexAccounts",
                "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_PlexAccountServers_PlexServerId",
                "PlexAccountServers",
                "PlexServerId");

            migrationBuilder.AddForeignKey(
                "FK_PlexAccounts_Accounts_AccountId",
                "PlexAccounts",
                "AccountId",
                "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_PlexAccounts_Accounts_AccountId",
                "PlexAccounts");

            migrationBuilder.DropTable(
                "PlexAccountServers");

            migrationBuilder.DropTable(
                "PlexServers");

            migrationBuilder.DropIndex(
                "IX_PlexAccounts_AccountId",
                "PlexAccounts");

            migrationBuilder.DropColumn(
                "AccountId",
                "PlexAccounts");

            migrationBuilder.DropColumn(
                "DisplayName",
                "Accounts");

            migrationBuilder.DropColumn(
                "IsEnabled",
                "Accounts");

            migrationBuilder.DropColumn(
                "IsValidated",
                "Accounts");

            migrationBuilder.DropColumn(
                "ValidatedAt",
                "Accounts");

            migrationBuilder.AddColumn<DateTime>(
                "ConfirmedAt",
                "Accounts",
                "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                "IsConfirmed",
                "Accounts",
                "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}