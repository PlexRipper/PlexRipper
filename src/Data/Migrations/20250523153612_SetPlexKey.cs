using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetPlexKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlexRoles_Name",
                table: "PlexRoles");

            migrationBuilder.DropIndex(
                name: "IX_PlexGenres_Name",
                table: "PlexGenres");

            migrationBuilder.DropIndex(
                name: "IX_PlexCountries_Name",
                table: "PlexCountries");

            migrationBuilder.AlterColumn<string>(
                name: "TagKey",
                table: "PlexRoles",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexRoles_PlexKey",
                table: "PlexRoles",
                column: "PlexKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexGenres_PlexKey",
                table: "PlexGenres",
                column: "PlexKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexCountries_PlexKey",
                table: "PlexCountries",
                column: "PlexKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlexRoles_PlexKey",
                table: "PlexRoles");

            migrationBuilder.DropIndex(
                name: "IX_PlexGenres_PlexKey",
                table: "PlexGenres");

            migrationBuilder.DropIndex(
                name: "IX_PlexCountries_PlexKey",
                table: "PlexCountries");

            migrationBuilder.AlterColumn<string>(
                name: "TagKey",
                table: "PlexRoles",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_PlexRoles_Name",
                table: "PlexRoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexGenres_Name",
                table: "PlexGenres",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexCountries_Name",
                table: "PlexCountries",
                column: "Name",
                unique: true);
        }
    }
}
