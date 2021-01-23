using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class MovedDataTotalIntoMetaData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataTotal",
                table: "DownloadTasks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DataTotal",
                table: "DownloadTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
