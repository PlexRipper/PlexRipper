using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class RemovedFilePathFromDownloadWorkerTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DownloadWorkerTasks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DownloadWorkerTasks",
                type: "TEXT",
                nullable: true);
        }
    }
}
