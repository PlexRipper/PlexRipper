using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryIdToGenreTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexTvShowId",
                    table: "PlexTvShowRoles",
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
                    table: "PlexTvShowRoles",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexTvShowId",
                    table: "PlexTvShowGenres",
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
                    table: "PlexTvShowGenres",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexTvShowId",
                    table: "PlexTvShowCountries",
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
                    table: "PlexTvShowCountries",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Relational:ColumnOrder", 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PlexLibraryId", table: "PlexTvShowRoles");

            migrationBuilder.DropColumn(name: "PlexLibraryId", table: "PlexTvShowGenres");

            migrationBuilder.DropColumn(name: "PlexLibraryId", table: "PlexTvShowCountries");

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexTvShowId",
                    table: "PlexTvShowRoles",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexTvShowId",
                    table: "PlexTvShowGenres",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexTvShowId",
                    table: "PlexTvShowCountries",
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
