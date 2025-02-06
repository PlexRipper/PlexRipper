using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class NameChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Role", table: "PlexRoles");

            migrationBuilder.DropColumn(name: "TagKey", table: "PlexRoles");

            migrationBuilder.RenameColumn(name: "PlexId", table: "PlexRoles", newName: "PlexKey");

            migrationBuilder.RenameColumn(name: "Title", table: "PlexGenres", newName: "Name");

            migrationBuilder.RenameColumn(name: "Key", table: "PlexGenres", newName: "PlexKey");

            migrationBuilder.RenameColumn(name: "PlexId", table: "PlexCountry", newName: "PlexKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "PlexKey", table: "PlexRoles", newName: "PlexId");

            migrationBuilder.RenameColumn(name: "PlexKey", table: "PlexGenres", newName: "Key");

            migrationBuilder.RenameColumn(name: "Name", table: "PlexGenres", newName: "Title");

            migrationBuilder.RenameColumn(name: "PlexKey", table: "PlexCountry", newName: "PlexId");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "PlexRoles",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "TagKey",
                table: "PlexRoles",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );
        }
    }
}
