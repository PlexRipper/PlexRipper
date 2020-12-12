using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class MinorUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                "PlexServerId",
                "PlexLibraries",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                "PlexId",
                "PlexAccounts",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "PlexId",
                "PlexAccounts");

            migrationBuilder.AlterColumn<int>(
                "PlexServerId",
                "PlexLibraries",
                "INTEGER",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}