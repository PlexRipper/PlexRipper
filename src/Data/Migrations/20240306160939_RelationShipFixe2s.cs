using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class RelationShipFixe2s : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "Created", table: "DownloadTaskTvShowSeason", newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "DownloadTaskTvShowEpisodeFile",
                newName: "CreatedAt"
            );

            migrationBuilder.RenameColumn(name: "Created", table: "DownloadTaskTvShowEpisode", newName: "CreatedAt");

            migrationBuilder.RenameColumn(name: "Created", table: "DownloadTaskTvShow", newName: "CreatedAt");

            migrationBuilder.RenameColumn(name: "Created", table: "DownloadTaskMovieFile", newName: "CreatedAt");

            migrationBuilder.RenameColumn(name: "Created", table: "DownloadTaskMovie", newName: "CreatedAt");

            migrationBuilder
                .AddColumn<string>(
                    name: "FullTitle",
                    table: "DownloadTaskTvShowEpisodeFile",
                    type: "TEXT",
                    nullable: true
                )
                .Annotation("Relational:ColumnOrder", 14);

            migrationBuilder
                .AddColumn<string>(
                    name: "Title",
                    table: "DownloadTaskTvShowEpisodeFile",
                    type: "TEXT",
                    nullable: true,
                    collation: "NATURALSORT"
                )
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AddColumn<string>(name: "FullTitle", table: "DownloadTaskMovieFile", type: "TEXT", nullable: true)
                .Annotation("Relational:ColumnOrder", 14);

            migrationBuilder
                .AddColumn<string>(
                    name: "Title",
                    table: "DownloadTaskMovieFile",
                    type: "TEXT",
                    nullable: true,
                    collation: "NATURALSORT"
                )
                .Annotation("Relational:ColumnOrder", 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FullTitle", table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(name: "Title", table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(name: "FullTitle", table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(name: "Title", table: "DownloadTaskMovieFile");

            migrationBuilder.RenameColumn(name: "CreatedAt", table: "DownloadTaskTvShowSeason", newName: "Created");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "DownloadTaskTvShowEpisodeFile",
                newName: "Created"
            );

            migrationBuilder.RenameColumn(name: "CreatedAt", table: "DownloadTaskTvShowEpisode", newName: "Created");

            migrationBuilder.RenameColumn(name: "CreatedAt", table: "DownloadTaskTvShow", newName: "Created");

            migrationBuilder.RenameColumn(name: "CreatedAt", table: "DownloadTaskMovieFile", newName: "Created");

            migrationBuilder.RenameColumn(name: "CreatedAt", table: "DownloadTaskMovie", newName: "Created");
        }
    }
}
