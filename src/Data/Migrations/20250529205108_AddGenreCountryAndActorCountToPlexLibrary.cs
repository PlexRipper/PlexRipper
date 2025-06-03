using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGenreCountryAndActorCountToPlexLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActorsCount",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 15);

            migrationBuilder.AddColumn<int>(
                name: "CountriesCount",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder.AddColumn<int>(
                name: "GenresCount",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 16);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActorsCount",
                table: "PlexLibraries");

            migrationBuilder.DropColumn(
                name: "CountriesCount",
                table: "PlexLibraries");

            migrationBuilder.DropColumn(
                name: "GenresCount",
                table: "PlexLibraries");
        }
    }
}
