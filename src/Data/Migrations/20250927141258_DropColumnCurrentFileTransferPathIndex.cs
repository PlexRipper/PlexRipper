using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropColumnCurrentFileTransferPathIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentFileTransferPathIndex",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(
                name: "CurrentFileTransferPathIndex",
                table: "DownloadTaskMovieFile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentFileTransferPathIndex",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentFileTransferPathIndex",
                table: "DownloadTaskMovieFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
