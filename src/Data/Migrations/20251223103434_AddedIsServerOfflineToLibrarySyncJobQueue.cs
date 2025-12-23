using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsServerOfflineToLibrarySyncJobQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<string>(
                    name: "ErrorMessage",
                    table: "BackgroundJobLibrarySyncJobQueues",
                    type: "TEXT",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "TEXT",
                    oldNullable: true
                )
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder
                .AddColumn<bool>(
                    name: "IsServerOffline",
                    table: "BackgroundJobLibrarySyncJobQueues",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: false
                )
                .Annotation("Relational:ColumnOrder", 6);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsServerOffline", table: "BackgroundJobLibrarySyncJobQueues");

            migrationBuilder
                .AlterColumn<string>(
                    name: "ErrorMessage",
                    table: "BackgroundJobLibrarySyncJobQueues",
                    type: "TEXT",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "TEXT",
                    oldNullable: true
                )
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 7);
        }
    }
}
