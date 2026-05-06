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
            migrationBuilder.AddColumn<bool>(
                name: "OwnedOverride",
                table: "PlexServers",
                type: "INTEGER",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder.Sql("UPDATE PlexServers SET OwnedOverride = Home");

            migrationBuilder.DropColumn(
                name: "Home",
                table: "PlexServers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Home",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder.Sql("UPDATE PlexServers SET Home = COALESCE(OwnedOverride, 0)");

            migrationBuilder.DropColumn(
                name: "OwnedOverride",
                table: "PlexServers");
        }
    }
}
