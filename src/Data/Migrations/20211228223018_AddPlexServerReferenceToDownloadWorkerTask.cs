using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class AddPlexServerReferenceToDownloadWorkerTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlexServerId",
                table: "DownloadWorkerTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DownloadWorkerTasks_PlexServerId",
                table: "DownloadWorkerTasks",
                column: "PlexServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadWorkerTasks_PlexServers_PlexServerId",
                table: "DownloadWorkerTasks",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadWorkerTasks_PlexServers_PlexServerId",
                table: "DownloadWorkerTasks");

            migrationBuilder.DropIndex(
                name: "IX_DownloadWorkerTasks_PlexServerId",
                table: "DownloadWorkerTasks");

            migrationBuilder.DropColumn(
                name: "PlexServerId",
                table: "DownloadWorkerTasks");
        }
    }
}
