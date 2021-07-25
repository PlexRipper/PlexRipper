using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class MakeDefaultDestinationNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraries_FolderPaths_DefaultDestinationId",
                table: "PlexLibraries");

            migrationBuilder.AlterColumn<int>(
                name: "DefaultDestinationId",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraries_FolderPaths_DefaultDestinationId",
                table: "PlexLibraries",
                column: "DefaultDestinationId",
                principalTable: "FolderPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraries_FolderPaths_DefaultDestinationId",
                table: "PlexLibraries");

            migrationBuilder.AlterColumn<int>(
                name: "DefaultDestinationId",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraries_FolderPaths_DefaultDestinationId",
                table: "PlexLibraries",
                column: "DefaultDestinationId",
                principalTable: "FolderPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
