using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class SpreadOutMetadataProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MetaData",
                table: "PlexLibraries");

            migrationBuilder.AddColumn<int>(
                name: "EpisodeCount",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 14);

            migrationBuilder.AddColumn<long>(
                name: "MediaSize",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Relational:ColumnOrder", 10);

            migrationBuilder.AddColumn<int>(
                name: "MovieCount",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 11);

            migrationBuilder.AddColumn<int>(
                name: "SeasonCount",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 13);

            migrationBuilder.AddColumn<int>(
                name: "TvShowCount",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 12);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EpisodeCount",
                table: "PlexLibraries");

            migrationBuilder.DropColumn(
                name: "MediaSize",
                table: "PlexLibraries");

            migrationBuilder.DropColumn(
                name: "MovieCount",
                table: "PlexLibraries");

            migrationBuilder.DropColumn(
                name: "SeasonCount",
                table: "PlexLibraries");

            migrationBuilder.DropColumn(
                name: "TvShowCount",
                table: "PlexLibraries");

            migrationBuilder.AddColumn<string>(
                name: "MetaData",
                table: "PlexLibraries",
                type: "TEXT",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 11);
        }
    }
}
