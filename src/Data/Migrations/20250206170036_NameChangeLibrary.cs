using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class NameChangeLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryCountries_PlexCountry_CountryId",
                table: "PlexLibraryCountries"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlexMovieCountries_PlexCountry_CountryId",
                table: "PlexMovieCountries"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowCountries_PlexCountry_CountryId",
                table: "PlexTvShowCountries"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_PlexCountry", table: "PlexCountry");

            migrationBuilder.RenameTable(name: "PlexCountry", newName: "PlexCountries");

            migrationBuilder.RenameColumn(name: "CountryId", table: "PlexLibraryCountries", newName: "CountriesId");

            migrationBuilder.AddPrimaryKey(name: "PK_PlexCountries", table: "PlexCountries", column: "Id");

            migrationBuilder.CreateIndex(name: "IX_PlexRoles_PlexKey", table: "PlexRoles", column: "PlexKey");

            migrationBuilder.CreateIndex(name: "IX_PlexGenres_PlexKey", table: "PlexGenres", column: "PlexKey");

            migrationBuilder.CreateIndex(name: "IX_PlexCountries_PlexKey", table: "PlexCountries", column: "PlexKey");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryCountries_PlexCountries_CountriesId",
                table: "PlexLibraryCountries",
                column: "CountriesId",
                principalTable: "PlexCountries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexMovieCountries_PlexCountries_CountryId",
                table: "PlexMovieCountries",
                column: "CountryId",
                principalTable: "PlexCountries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowCountries_PlexCountries_CountryId",
                table: "PlexTvShowCountries",
                column: "CountryId",
                principalTable: "PlexCountries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryCountries_PlexCountries_CountriesId",
                table: "PlexLibraryCountries"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlexMovieCountries_PlexCountries_CountryId",
                table: "PlexMovieCountries"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowCountries_PlexCountries_CountryId",
                table: "PlexTvShowCountries"
            );

            migrationBuilder.DropIndex(name: "IX_PlexRoles_PlexKey", table: "PlexRoles");

            migrationBuilder.DropIndex(name: "IX_PlexGenres_PlexKey", table: "PlexGenres");

            migrationBuilder.DropPrimaryKey(name: "PK_PlexCountries", table: "PlexCountries");

            migrationBuilder.DropIndex(name: "IX_PlexCountries_PlexKey", table: "PlexCountries");

            migrationBuilder.RenameTable(name: "PlexCountries", newName: "PlexCountry");

            migrationBuilder.RenameColumn(name: "CountriesId", table: "PlexLibraryCountries", newName: "CountryId");

            migrationBuilder.AddPrimaryKey(name: "PK_PlexCountry", table: "PlexCountry", column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryCountries_PlexCountry_CountryId",
                table: "PlexLibraryCountries",
                column: "CountryId",
                principalTable: "PlexCountry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexMovieCountries_PlexCountry_CountryId",
                table: "PlexMovieCountries",
                column: "CountryId",
                principalTable: "PlexCountry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowCountries_PlexCountry_CountryId",
                table: "PlexTvShowCountries",
                column: "CountryId",
                principalTable: "PlexCountry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
