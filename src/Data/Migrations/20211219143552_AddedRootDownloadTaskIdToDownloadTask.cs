using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class AddedRootDownloadTaskIdToDownloadTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RootDownloadTaskId",
                table: "DownloadTasks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTasks_RootDownloadTaskId",
                table: "DownloadTasks",
                column: "RootDownloadTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTasks_DownloadTasks_RootDownloadTaskId",
                table: "DownloadTasks",
                column: "RootDownloadTaskId",
                principalTable: "DownloadTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTasks_DownloadTasks_RootDownloadTaskId",
                table: "DownloadTasks");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTasks_RootDownloadTaskId",
                table: "DownloadTasks");

            migrationBuilder.DropColumn(
                name: "RootDownloadTaskId",
                table: "DownloadTasks");
        }
    }
}
