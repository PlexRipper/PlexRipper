using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexLibraryId",
                    table: "BackgroundJobLibrarySyncJobQueues",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexServerId",
                    table: "BackgroundJobLibrarySyncJobQueues",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 8);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexLibraryId",
                    table: "BackgroundJobLibrarySyncJobQueues",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexServerId",
                    table: "BackgroundJobLibrarySyncJobQueues",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 9);
        }
    }
}
