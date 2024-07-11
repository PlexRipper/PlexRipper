using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetNaturalSortForSortTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SortTitle",
                table: "PlexTvShowSeason",
                type: "TEXT",
                nullable: true,
                collation: "NATURALSORT",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "SortTitle",
                table: "PlexTvShows",
                type: "TEXT",
                nullable: true,
                collation: "NATURALSORT",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "SortTitle",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true,
                collation: "NATURALSORT",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "SortTitle",
                table: "PlexMovie",
                type: "TEXT",
                nullable: true,
                collation: "NATURALSORT",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "PlexLibraries",
                type: "TEXT",
                nullable: true,
                collation: "NATURALSORT",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "DownloadTasks",
                type: "TEXT",
                nullable: true,
                collation: "NATURALSORT",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SortTitle",
                table: "PlexTvShowSeason",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldCollation: "NATURALSORT"
            );

            migrationBuilder.AlterColumn<string>(
                name: "SortTitle",
                table: "PlexTvShows",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldCollation: "NATURALSORT"
            );

            migrationBuilder.AlterColumn<string>(
                name: "SortTitle",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldCollation: "NATURALSORT"
            );

            migrationBuilder.AlterColumn<string>(
                name: "SortTitle",
                table: "PlexMovie",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldCollation: "NATURALSORT"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "PlexLibraries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldCollation: "NATURALSORT"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "DownloadTasks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldCollation: "NATURALSORT"
            );
        }
    }
}
