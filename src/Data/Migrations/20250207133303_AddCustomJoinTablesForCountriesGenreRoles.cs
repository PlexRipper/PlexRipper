using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomJoinTablesForCountriesGenreRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexMovieCountries_PlexMovie_PlexMovieCountriesId",
                table: "PlexMovieCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexMovieGenres_PlexMovie_PlexMovieGenresId",
                table: "PlexMovieGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexMovieRoles_PlexMovie_PlexMovieRolesId",
                table: "PlexMovieRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowCountries_PlexTvShows_PlexTvShowCountriesId",
                table: "PlexTvShowCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowGenres_PlexTvShows_PlexTvShowGenresId",
                table: "PlexTvShowGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowRoles_PlexTvShows_PlexTvShowRolesId",
                table: "PlexTvShowRoles");

            migrationBuilder.RenameColumn(
                name: "PlexTvShowRolesId",
                table: "PlexTvShowRoles",
                newName: "PlexTvShowId");

            migrationBuilder.RenameColumn(
                name: "PlexTvShowGenresId",
                table: "PlexTvShowGenres",
                newName: "PlexTvShowId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowGenres_PlexTvShowGenresId",
                table: "PlexTvShowGenres",
                newName: "IX_PlexTvShowGenres_PlexTvShowId");

            migrationBuilder.RenameColumn(
                name: "PlexTvShowCountriesId",
                table: "PlexTvShowCountries",
                newName: "PlexTvShowId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowCountries_PlexTvShowCountriesId",
                table: "PlexTvShowCountries",
                newName: "IX_PlexTvShowCountries_PlexTvShowId");

            migrationBuilder.RenameColumn(
                name: "PlexMovieRolesId",
                table: "PlexMovieRoles",
                newName: "PlexMovieId");

            migrationBuilder.RenameColumn(
                name: "PlexMovieGenresId",
                table: "PlexMovieGenres",
                newName: "PlexMovieId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovieGenres_PlexMovieGenresId",
                table: "PlexMovieGenres",
                newName: "IX_PlexMovieGenres_PlexMovieId");

            migrationBuilder.RenameColumn(
                name: "PlexMovieCountriesId",
                table: "PlexMovieCountries",
                newName: "PlexMovieId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovieCountries_PlexMovieCountriesId",
                table: "PlexMovieCountries",
                newName: "IX_PlexMovieCountries_PlexMovieId");

            migrationBuilder.AlterColumn<int>(
                name: "RolesId",
                table: "PlexTvShowRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexTvShowId",
                table: "PlexTvShowRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "GenresId",
                table: "PlexTvShowGenres",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexTvShowId",
                table: "PlexTvShowGenres",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "CountryId",
                table: "PlexTvShowCountries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexTvShowId",
                table: "PlexTvShowCountries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "PlexRoles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TagKey",
                table: "PlexRoles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "RolesId",
                table: "PlexMovieRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexMovieId",
                table: "PlexMovieRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "GenresId",
                table: "PlexMovieGenres",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexMovieId",
                table: "PlexMovieGenres",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "CountryId",
                table: "PlexMovieCountries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexMovieId",
                table: "PlexMovieCountries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexMovieCountries_PlexMovie_PlexMovieId",
                table: "PlexMovieCountries",
                column: "PlexMovieId",
                principalTable: "PlexMovie",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexMovieGenres_PlexMovie_PlexMovieId",
                table: "PlexMovieGenres",
                column: "PlexMovieId",
                principalTable: "PlexMovie",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexMovieRoles_PlexMovie_PlexMovieId",
                table: "PlexMovieRoles",
                column: "PlexMovieId",
                principalTable: "PlexMovie",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowCountries_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowCountries",
                column: "PlexTvShowId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowGenres_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowGenres",
                column: "PlexTvShowId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowRoles_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowRoles",
                column: "PlexTvShowId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexMovieCountries_PlexMovie_PlexMovieId",
                table: "PlexMovieCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexMovieGenres_PlexMovie_PlexMovieId",
                table: "PlexMovieGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexMovieRoles_PlexMovie_PlexMovieId",
                table: "PlexMovieRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowCountries_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowGenres_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowRoles_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowRoles");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "PlexRoles");

            migrationBuilder.DropColumn(
                name: "TagKey",
                table: "PlexRoles");

            migrationBuilder.RenameColumn(
                name: "PlexTvShowId",
                table: "PlexTvShowRoles",
                newName: "PlexTvShowRolesId");

            migrationBuilder.RenameColumn(
                name: "PlexTvShowId",
                table: "PlexTvShowGenres",
                newName: "PlexTvShowGenresId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowGenres_PlexTvShowId",
                table: "PlexTvShowGenres",
                newName: "IX_PlexTvShowGenres_PlexTvShowGenresId");

            migrationBuilder.RenameColumn(
                name: "PlexTvShowId",
                table: "PlexTvShowCountries",
                newName: "PlexTvShowCountriesId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowCountries_PlexTvShowId",
                table: "PlexTvShowCountries",
                newName: "IX_PlexTvShowCountries_PlexTvShowCountriesId");

            migrationBuilder.RenameColumn(
                name: "PlexMovieId",
                table: "PlexMovieRoles",
                newName: "PlexMovieRolesId");

            migrationBuilder.RenameColumn(
                name: "PlexMovieId",
                table: "PlexMovieGenres",
                newName: "PlexMovieGenresId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovieGenres_PlexMovieId",
                table: "PlexMovieGenres",
                newName: "IX_PlexMovieGenres_PlexMovieGenresId");

            migrationBuilder.RenameColumn(
                name: "PlexMovieId",
                table: "PlexMovieCountries",
                newName: "PlexMovieCountriesId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovieCountries_PlexMovieId",
                table: "PlexMovieCountries",
                newName: "IX_PlexMovieCountries_PlexMovieCountriesId");

            migrationBuilder.AlterColumn<int>(
                name: "RolesId",
                table: "PlexTvShowRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexTvShowRolesId",
                table: "PlexTvShowRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "GenresId",
                table: "PlexTvShowGenres",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexTvShowGenresId",
                table: "PlexTvShowGenres",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "CountryId",
                table: "PlexTvShowCountries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexTvShowCountriesId",
                table: "PlexTvShowCountries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "RolesId",
                table: "PlexMovieRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexMovieRolesId",
                table: "PlexMovieRoles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "GenresId",
                table: "PlexMovieGenres",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexMovieGenresId",
                table: "PlexMovieGenres",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "CountryId",
                table: "PlexMovieCountries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexMovieCountriesId",
                table: "PlexMovieCountries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexMovieCountries_PlexMovie_PlexMovieCountriesId",
                table: "PlexMovieCountries",
                column: "PlexMovieCountriesId",
                principalTable: "PlexMovie",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexMovieGenres_PlexMovie_PlexMovieGenresId",
                table: "PlexMovieGenres",
                column: "PlexMovieGenresId",
                principalTable: "PlexMovie",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexMovieRoles_PlexMovie_PlexMovieRolesId",
                table: "PlexMovieRoles",
                column: "PlexMovieRolesId",
                principalTable: "PlexMovie",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowCountries_PlexTvShows_PlexTvShowCountriesId",
                table: "PlexTvShowCountries",
                column: "PlexTvShowCountriesId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowGenres_PlexTvShows_PlexTvShowGenresId",
                table: "PlexTvShowGenres",
                column: "PlexTvShowGenresId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowRoles_PlexTvShows_PlexTvShowRolesId",
                table: "PlexTvShowRoles",
                column: "PlexTvShowRolesId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
