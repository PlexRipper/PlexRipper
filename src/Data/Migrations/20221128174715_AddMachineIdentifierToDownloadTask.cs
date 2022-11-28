using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    public partial class AddMachineIdentifierToDownloadTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServerMachineIdentifier",
                table: "DownloadTasks",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 19);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServerMachineIdentifier",
                table: "DownloadTasks");
        }
    }
}
