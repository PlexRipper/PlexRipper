using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class asda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DestinationDirectory", table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(name: "DownloadDirectory", table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(name: "DestinationDirectory", table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(name: "DownloadDirectory", table: "DownloadTaskMovieFile");

            migrationBuilder
                .AddColumn<string>(
                    name: "DirectoryMeta",
                    table: "DownloadTaskTvShowEpisodeFile",
                    type: "TEXT",
                    nullable: true
                )
                .Annotation("Relational:ColumnOrder", 16);

            migrationBuilder
                .AddColumn<string>(name: "DirectoryMeta", table: "DownloadTaskMovieFile", type: "TEXT", nullable: true)
                .Annotation("Relational:ColumnOrder", 16);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DirectoryMeta", table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(name: "DirectoryMeta", table: "DownloadTaskMovieFile");

            migrationBuilder
                .AddColumn<string>(
                    name: "DestinationDirectory",
                    table: "DownloadTaskTvShowEpisodeFile",
                    type: "TEXT",
                    nullable: true
                )
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder
                .AddColumn<string>(
                    name: "DownloadDirectory",
                    table: "DownloadTaskTvShowEpisodeFile",
                    type: "TEXT",
                    nullable: true
                )
                .Annotation("Relational:ColumnOrder", 16);

            migrationBuilder
                .AddColumn<string>(
                    name: "DestinationDirectory",
                    table: "DownloadTaskMovieFile",
                    type: "TEXT",
                    nullable: true
                )
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder
                .AddColumn<string>(
                    name: "DownloadDirectory",
                    table: "DownloadTaskMovieFile",
                    type: "TEXT",
                    nullable: true
                )
                .Annotation("Relational:ColumnOrder", 16);
        }
    }
}
