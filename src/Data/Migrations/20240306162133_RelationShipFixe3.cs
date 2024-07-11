using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class RelationShipFixe3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskMovieFile_DownloadTaskId",
                table: "DownloadWorkerTasks"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskTvShowEpisodeFile_DownloadTaskId",
                table: "DownloadWorkerTasks"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskMovieFile_DownloadTaskId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskId",
                principalTable: "DownloadTaskMovieFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskTvShowEpisodeFile_DownloadTaskId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskId",
                principalTable: "DownloadTaskTvShowEpisodeFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
