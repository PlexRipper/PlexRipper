using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class MinorUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PlexServerId",
                table: "PlexLibraries",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PlexId",
                table: "PlexAccounts",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlexId",
                table: "PlexAccounts");

            migrationBuilder.AlterColumn<int>(
                name: "PlexServerId",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
