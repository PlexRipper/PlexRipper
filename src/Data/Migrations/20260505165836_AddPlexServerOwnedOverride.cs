using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlexServerOwnedOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Home",
                table: "PlexServers");

            migrationBuilder.AddColumn<bool>(
                name: "OwnedOverride",
                table: "PlexServers",
                type: "INTEGER",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 17);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnedOverride",
                table: "PlexServers");

            migrationBuilder.AddColumn<bool>(
                name: "Home",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 17);
        }
    }
}
