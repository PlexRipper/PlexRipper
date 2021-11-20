using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class DataReceivedAndPercentageToDownloadTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DataReceived",
                table: "DownloadTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "Percentage",
                table: "DownloadTasks",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataReceived",
                table: "DownloadTasks");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "DownloadTasks");
        }
    }
}
