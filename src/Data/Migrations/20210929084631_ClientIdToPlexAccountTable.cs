using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class ClientIdToPlexAccountTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "PlexAccounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "Notifications",
                type: "TEXT",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "PlexAccounts");

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "Notifications",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldUnicode: false,
                oldMaxLength: 20);
        }
    }
}
