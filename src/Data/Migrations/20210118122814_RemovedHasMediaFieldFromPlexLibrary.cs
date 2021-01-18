using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class RemovedHasMediaFieldFromPlexLibrary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasMedia",
                table: "PlexLibraries");

            migrationBuilder.DropColumn(
                name: "MediaSize",
                table: "PlexLibraries");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasMedia",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "MediaSize",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
