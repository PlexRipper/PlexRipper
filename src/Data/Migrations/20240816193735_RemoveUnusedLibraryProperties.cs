using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedLibraryProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LibraryLocationId",
                table: "PlexLibraries");

            migrationBuilder.DropColumn(
                name: "LibraryLocationPath",
                table: "PlexLibraries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LibraryLocationId",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 10);

            migrationBuilder.AddColumn<string>(
                name: "LibraryLocationPath",
                table: "PlexLibraries",
                type: "TEXT",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 4);
        }
    }
}
