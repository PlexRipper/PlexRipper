using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDownloadsPausedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDownloadsPausedByUser",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 16);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDownloadsPausedByUser",
                table: "PlexServers");
        }
    }
}
