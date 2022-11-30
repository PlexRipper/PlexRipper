using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    public partial class RemoveDirectConnectionPropertiesFromPlexServer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "Host",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "LocalAddresses",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "Port",
                table: "PlexServers");

            migrationBuilder.RenameColumn(
                name: "Scheme",
                table: "PlexServers",
                newName: "PublicAddress");

            migrationBuilder.AlterColumn<string>(
                name: "PublicAddress",
                table: "PlexServers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 2);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PublicAddress",
                table: "PlexServers",
                newName: "Scheme");

            migrationBuilder.AlterColumn<string>(
                name: "Scheme",
                table: "PlexServers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "PlexServers",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AddColumn<string>(
                name: "Host",
                table: "PlexServers",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AddColumn<string>(
                name: "LocalAddresses",
                table: "PlexServers",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 7);

            migrationBuilder.AddColumn<int>(
                name: "Port",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 4);
        }
    }
}
