using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConvertTitleSortToSortIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "SortTitle", table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(name: "SortTitle", table: "PlexTvShows");

            migrationBuilder.DropColumn(name: "SortTitle", table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(name: "SortTitle", table: "PlexMovie");

            migrationBuilder
                .AddColumn<int>(
                    name: "SortIndex",
                    table: "PlexTvShowSeason",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AddColumn<int>(
                    name: "SortIndex",
                    table: "PlexTvShows",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AddColumn<int>(
                    name: "SortIndex",
                    table: "PlexTvShowEpisodes",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AddColumn<int>(
                    name: "SortIndex",
                    table: "PlexMovie",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowSeason_SortIndex",
                table: "PlexTvShowSeason",
                column: "SortIndex"
            );

            migrationBuilder.CreateIndex(name: "IX_PlexTvShows_SortIndex", table: "PlexTvShows", column: "SortIndex");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodes_SortIndex",
                table: "PlexTvShowEpisodes",
                column: "SortIndex"
            );

            migrationBuilder.CreateIndex(name: "IX_PlexMovie_SortIndex", table: "PlexMovie", column: "SortIndex");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_PlexTvShowSeason_SortIndex", table: "PlexTvShowSeason");

            migrationBuilder.DropIndex(name: "IX_PlexTvShows_SortIndex", table: "PlexTvShows");

            migrationBuilder.DropIndex(name: "IX_PlexTvShowEpisodes_SortIndex", table: "PlexTvShowEpisodes");

            migrationBuilder.DropIndex(name: "IX_PlexMovie_SortIndex", table: "PlexMovie");

            migrationBuilder.DropColumn(name: "SortIndex", table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(name: "SortIndex", table: "PlexTvShows");

            migrationBuilder.DropColumn(name: "SortIndex", table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(name: "SortIndex", table: "PlexMovie");

            migrationBuilder
                .AddColumn<string>(
                    name: "SortTitle",
                    table: "PlexTvShowSeason",
                    type: "TEXT",
                    nullable: false,
                    defaultValue: "",
                    collation: "NATURALSORT"
                )
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AddColumn<string>(
                    name: "SortTitle",
                    table: "PlexTvShows",
                    type: "TEXT",
                    nullable: false,
                    defaultValue: "",
                    collation: "NATURALSORT"
                )
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AddColumn<string>(
                    name: "SortTitle",
                    table: "PlexTvShowEpisodes",
                    type: "TEXT",
                    nullable: false,
                    defaultValue: "",
                    collation: "NATURALSORT"
                )
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AddColumn<string>(
                    name: "SortTitle",
                    table: "PlexMovie",
                    type: "TEXT",
                    nullable: false,
                    defaultValue: "",
                    collation: "NATURALSORT"
                )
                .Annotation("Relational:ColumnOrder", 4);
        }
    }
}
