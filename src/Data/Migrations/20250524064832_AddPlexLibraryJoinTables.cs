using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlexLibraryJoinTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryCountries_PlexCountries_CountriesId",
                table: "PlexLibraryCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryCountries_PlexLibraries_PlexLibrariesId",
                table: "PlexLibraryCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryGenres_PlexGenres_GenresId",
                table: "PlexLibraryGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryGenres_PlexLibraries_PlexLibrariesId",
                table: "PlexLibraryGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryRoles_PlexLibraries_PlexLibrariesId",
                table: "PlexLibraryRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryRoles_PlexRoles_RolesId",
                table: "PlexLibraryRoles");

            migrationBuilder.RenameColumn(
                name: "RolesId",
                table: "PlexLibraryRoles",
                newName: "PlexRoleId");

            migrationBuilder.RenameColumn(
                name: "PlexLibrariesId",
                table: "PlexLibraryRoles",
                newName: "PlexLibraryId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexLibraryRoles_RolesId",
                table: "PlexLibraryRoles",
                newName: "IX_PlexLibraryRoles_PlexRoleId");

            migrationBuilder.RenameColumn(
                name: "PlexLibrariesId",
                table: "PlexLibraryGenres",
                newName: "PlexLibraryId");

            migrationBuilder.RenameColumn(
                name: "GenresId",
                table: "PlexLibraryGenres",
                newName: "PlexGenreId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexLibraryGenres_PlexLibrariesId",
                table: "PlexLibraryGenres",
                newName: "IX_PlexLibraryGenres_PlexLibraryId");

            migrationBuilder.RenameColumn(
                name: "PlexLibrariesId",
                table: "PlexLibraryCountries",
                newName: "PlexLibraryId");

            migrationBuilder.RenameColumn(
                name: "CountriesId",
                table: "PlexLibraryCountries",
                newName: "PlexCountryId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexLibraryCountries_PlexLibrariesId",
                table: "PlexLibraryCountries",
                newName: "IX_PlexLibraryCountries_PlexLibraryId");

            migrationBuilder.AlterColumn<int>(
                name: "PlexRoleId",
                table: "PlexLibraryRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "PlexLibraryId",
                table: "PlexLibraryRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexLibraryId",
                table: "PlexLibraryGenres",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexGenreId",
                table: "PlexLibraryGenres",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "PlexLibraryId",
                table: "PlexLibraryCountries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexCountryId",
                table: "PlexLibraryCountries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryCountries_PlexCountries_PlexCountryId",
                table: "PlexLibraryCountries",
                column: "PlexCountryId",
                principalTable: "PlexCountries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryCountries_PlexLibraries_PlexLibraryId",
                table: "PlexLibraryCountries",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryGenres_PlexGenres_PlexGenreId",
                table: "PlexLibraryGenres",
                column: "PlexGenreId",
                principalTable: "PlexGenres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryGenres_PlexLibraries_PlexLibraryId",
                table: "PlexLibraryGenres",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryRoles_PlexLibraries_PlexLibraryId",
                table: "PlexLibraryRoles",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryRoles_PlexRoles_PlexRoleId",
                table: "PlexLibraryRoles",
                column: "PlexRoleId",
                principalTable: "PlexRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryCountries_PlexCountries_PlexCountryId",
                table: "PlexLibraryCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryCountries_PlexLibraries_PlexLibraryId",
                table: "PlexLibraryCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryGenres_PlexGenres_PlexGenreId",
                table: "PlexLibraryGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryGenres_PlexLibraries_PlexLibraryId",
                table: "PlexLibraryGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryRoles_PlexLibraries_PlexLibraryId",
                table: "PlexLibraryRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexLibraryRoles_PlexRoles_PlexRoleId",
                table: "PlexLibraryRoles");

            migrationBuilder.RenameColumn(
                name: "PlexRoleId",
                table: "PlexLibraryRoles",
                newName: "RolesId");

            migrationBuilder.RenameColumn(
                name: "PlexLibraryId",
                table: "PlexLibraryRoles",
                newName: "PlexLibrariesId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexLibraryRoles_PlexRoleId",
                table: "PlexLibraryRoles",
                newName: "IX_PlexLibraryRoles_RolesId");

            migrationBuilder.RenameColumn(
                name: "PlexLibraryId",
                table: "PlexLibraryGenres",
                newName: "PlexLibrariesId");

            migrationBuilder.RenameColumn(
                name: "PlexGenreId",
                table: "PlexLibraryGenres",
                newName: "GenresId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexLibraryGenres_PlexLibraryId",
                table: "PlexLibraryGenres",
                newName: "IX_PlexLibraryGenres_PlexLibrariesId");

            migrationBuilder.RenameColumn(
                name: "PlexLibraryId",
                table: "PlexLibraryCountries",
                newName: "PlexLibrariesId");

            migrationBuilder.RenameColumn(
                name: "PlexCountryId",
                table: "PlexLibraryCountries",
                newName: "CountriesId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexLibraryCountries_PlexLibraryId",
                table: "PlexLibraryCountries",
                newName: "IX_PlexLibraryCountries_PlexLibrariesId");

            migrationBuilder.AlterColumn<int>(
                name: "RolesId",
                table: "PlexLibraryRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "PlexLibrariesId",
                table: "PlexLibraryRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexLibrariesId",
                table: "PlexLibraryGenres",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "GenresId",
                table: "PlexLibraryGenres",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "PlexLibrariesId",
                table: "PlexLibraryCountries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "CountriesId",
                table: "PlexLibraryCountries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryCountries_PlexCountries_CountriesId",
                table: "PlexLibraryCountries",
                column: "CountriesId",
                principalTable: "PlexCountries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryCountries_PlexLibraries_PlexLibrariesId",
                table: "PlexLibraryCountries",
                column: "PlexLibrariesId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryGenres_PlexGenres_GenresId",
                table: "PlexLibraryGenres",
                column: "GenresId",
                principalTable: "PlexGenres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryGenres_PlexLibraries_PlexLibrariesId",
                table: "PlexLibraryGenres",
                column: "PlexLibrariesId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryRoles_PlexLibraries_PlexLibrariesId",
                table: "PlexLibraryRoles",
                column: "PlexLibrariesId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexLibraryRoles_PlexRoles_RolesId",
                table: "PlexLibraryRoles",
                column: "RolesId",
                principalTable: "PlexRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
