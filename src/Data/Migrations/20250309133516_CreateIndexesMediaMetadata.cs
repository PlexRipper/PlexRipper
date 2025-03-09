using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateIndexesMediaMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlexGenres_PlexKey",
                table: "PlexGenres");

            migrationBuilder.DropIndex(
                name: "IX_PlexCountries_PlexKey",
                table: "PlexCountries");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlexGenres_Name",
                table: "PlexGenres");

            migrationBuilder.DropIndex(
                name: "IX_PlexCountries_Name",
                table: "PlexCountries");

            migrationBuilder.CreateIndex(
                name: "IX_PlexGenres_PlexKey",
                table: "PlexGenres",
                column: "PlexKey");

            migrationBuilder.CreateIndex(
                name: "IX_PlexCountries_PlexKey",
                table: "PlexCountries",
                column: "PlexKey");
        }
    }
}
