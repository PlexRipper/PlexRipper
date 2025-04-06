using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnneededPropertiesOfPlexRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_PlexRoles_PlexKey", table: "PlexRoles");

            migrationBuilder.DropColumn(name: "PlexKey", table: "PlexRoles");

            migrationBuilder.DropColumn(name: "Role", table: "PlexRoles");

            migrationBuilder.DropColumn(name: "TagKey", table: "PlexRoles");

            migrationBuilder.DropColumn(name: "ThumbnailUrl", table: "PlexRoles");

            migrationBuilder.CreateIndex(name: "IX_PlexRoles_Name", table: "PlexRoles", column: "Name", unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_PlexRoles_Name", table: "PlexRoles");

            migrationBuilder.AddColumn<long>(
                name: "PlexKey",
                table: "PlexRoles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L
            );

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

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "PlexRoles",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.CreateIndex(name: "IX_PlexRoles_PlexKey", table: "PlexRoles", column: "PlexKey");
        }
    }
}
