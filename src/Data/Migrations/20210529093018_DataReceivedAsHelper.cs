using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class DataReceivedAsHelper : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "DownloadWorkerTasks");

            migrationBuilder.DropColumn(
                name: "DataReceived",
                table: "DownloadTasks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "DownloadWorkerTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DataReceived",
                table: "DownloadTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
