using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGuidToPlexMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<string>(
                    name: "MediaData",
                    table: "PlexTvShowSeason",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 23);

            migrationBuilder
                .AddColumn<string>(
                    name: "Guid",
                    table: "PlexTvShowSeason",
                    type: "TEXT",
                    nullable: false,
                    defaultValue: ""
                )
                .Annotation("Relational:ColumnOrder", 23);

            migrationBuilder
                .AlterColumn<string>(
                    name: "MediaData",
                    table: "PlexTvShows",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 23);

            migrationBuilder
                .AddColumn<string>(name: "Guid", table: "PlexTvShows", type: "TEXT", nullable: false, defaultValue: "")
                .Annotation("Relational:ColumnOrder", 23);

            migrationBuilder
                .AlterColumn<string>(
                    name: "MediaData",
                    table: "PlexTvShowEpisodes",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 23);

            migrationBuilder
                .AddColumn<string>(
                    name: "Guid",
                    table: "PlexTvShowEpisodes",
                    type: "TEXT",
                    nullable: false,
                    defaultValue: ""
                )
                .Annotation("Relational:ColumnOrder", 23);

            migrationBuilder
                .AlterColumn<string>(
                    name: "MediaData",
                    table: "PlexMovie",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 23);

            migrationBuilder
                .AddColumn<string>(name: "Guid", table: "PlexMovie", type: "TEXT", nullable: false, defaultValue: "")
                .Annotation("Relational:ColumnOrder", 23);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Guid", table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(name: "Guid", table: "PlexTvShows");

            migrationBuilder.DropColumn(name: "Guid", table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(name: "Guid", table: "PlexMovie");

            migrationBuilder
                .AlterColumn<string>(
                    name: "MediaData",
                    table: "PlexTvShowSeason",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 23);

            migrationBuilder
                .AlterColumn<string>(
                    name: "MediaData",
                    table: "PlexTvShows",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 23);

            migrationBuilder
                .AlterColumn<string>(
                    name: "MediaData",
                    table: "PlexTvShowEpisodes",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 23);

            migrationBuilder
                .AlterColumn<string>(
                    name: "MediaData",
                    table: "PlexMovie",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 23);
        }
    }
}
