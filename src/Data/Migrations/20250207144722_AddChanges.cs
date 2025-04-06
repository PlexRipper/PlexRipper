using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexMovieId",
                    table: "PlexMovieRoles",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AddColumn<int>(
                    name: "PlexLibraryId",
                    table: "PlexMovieRoles",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexMovieId",
                    table: "PlexMovieGenres",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AddColumn<int>(
                    name: "PlexLibraryId",
                    table: "PlexMovieGenres",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexMovieId",
                    table: "PlexMovieCountries",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AddColumn<int>(
                    name: "PlexLibraryId",
                    table: "PlexMovieCountries",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Relational:ColumnOrder", 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PlexLibraryId", table: "PlexMovieRoles");

            migrationBuilder.DropColumn(name: "PlexLibraryId", table: "PlexMovieGenres");

            migrationBuilder.DropColumn(name: "PlexLibraryId", table: "PlexMovieCountries");

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexMovieId",
                    table: "PlexMovieRoles",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexMovieId",
                    table: "PlexMovieGenres",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexMovieId",
                    table: "PlexMovieCountries",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 3);
        }
    }
}
