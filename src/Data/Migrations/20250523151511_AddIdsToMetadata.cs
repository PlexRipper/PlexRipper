using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdsToMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlexKey",
                table: "PlexRoles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "PlexRoles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TagKey",
                table: "PlexRoles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Thumb",
                table: "PlexRoles",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlexKey",
                table: "PlexRoles");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "PlexRoles");

            migrationBuilder.DropColumn(
                name: "TagKey",
                table: "PlexRoles");

            migrationBuilder.DropColumn(
                name: "Thumb",
                table: "PlexRoles");
        }
    }
}
