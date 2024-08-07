using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedUriToPlexServerConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<bool>(
                    name: "Relay",
                    table: "PlexServerConnections",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "PortFix",
                    table: "PlexServerConnections",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "Local",
                    table: "PlexServerConnections",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "IPv6",
                    table: "PlexServerConnections",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "IPv4",
                    table: "PlexServerConnections",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder
                .AddColumn<string>(
                    name: "Uri",
                    table: "PlexServerConnections",
                    type: "TEXT",
                    nullable: false,
                    defaultValue: ""
                )
                .Annotation("Relational:ColumnOrder", 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Uri", table: "PlexServerConnections");

            migrationBuilder
                .AlterColumn<bool>(
                    name: "Relay",
                    table: "PlexServerConnections",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "PortFix",
                    table: "PlexServerConnections",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "Local",
                    table: "PlexServerConnections",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "IPv6",
                    table: "PlexServerConnections",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "IPv4",
                    table: "PlexServerConnections",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 7);
        }
    }
}
