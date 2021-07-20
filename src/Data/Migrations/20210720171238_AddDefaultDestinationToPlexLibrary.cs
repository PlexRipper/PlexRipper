using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class AddDefaultDestinationToPlexLibrary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultDestinationId",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PlexLibraries_DefaultDestinationId",
                table: "PlexLibraries",
                column: "DefaultDestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraries_FolderPaths_DefaultDestinationId",
                table: "PlexLibraries",
                column: "DefaultDestinationId",
                principalTable: "FolderPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraries_FolderPaths_DefaultDestinationId",
                table: "PlexLibraries");

            migrationBuilder.DropIndex(
                name: "IX_PlexLibraries_DefaultDestinationId",
                table: "PlexLibraries");

            migrationBuilder.DropColumn(
                name: "DefaultDestinationId",
                table: "PlexLibraries");
        }
    }
}
