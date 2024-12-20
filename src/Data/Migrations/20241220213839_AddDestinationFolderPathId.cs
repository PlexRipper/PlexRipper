using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDestinationFolderPathId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "FileDataTransferred",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 20)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AddColumn<int>(
                name: "DestinationFolderPathId",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FileDataTransferred",
                table: "DownloadTaskMovieFile",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 20)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AddColumn<int>(
                name: "DestinationFolderPathId",
                table: "DownloadTaskMovieFile",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationFolderPathId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(
                name: "DestinationFolderPathId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.AlterColumn<long>(
                name: "FileDataTransferred",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 20);

            migrationBuilder.AlterColumn<long>(
                name: "FileDataTransferred",
                table: "DownloadTaskMovieFile",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 20);
        }
    }
}
