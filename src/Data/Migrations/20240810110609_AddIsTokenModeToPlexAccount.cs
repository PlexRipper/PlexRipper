using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsTokenModeToPlexAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<string>(
                    name: "AuthenticationToken",
                    table: "PlexAccounts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 13);

            migrationBuilder.AddColumn<bool>(
                name: "IsAuthTokenMode",
                table: "PlexAccounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsAuthTokenMode", table: "PlexAccounts");

            migrationBuilder
                .AlterColumn<string>(
                    name: "AuthenticationToken",
                    table: "PlexAccounts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 13);
        }
    }
}
