using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    public partial class RemovedOwned : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexServerConnections_PlexServers_PlexServerId",
                table: "PlexServerConnections");

            migrationBuilder.DropColumn(
                name: "Owned",
                table: "PlexAccountServers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AuthTokenCreationDate",
                table: "PlexAccountServers",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "AuthToken",
                table: "PlexAccountServers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexServerConnections_PlexServers_PlexServerId",
                table: "PlexServerConnections",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexServerConnections_PlexServers_PlexServerId",
                table: "PlexServerConnections");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AuthTokenCreationDate",
                table: "PlexAccountServers",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "AuthToken",
                table: "PlexAccountServers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddColumn<bool>(
                name: "Owned",
                table: "PlexAccountServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexServerConnections_PlexServers_PlexServerId",
                table: "PlexServerConnections",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id");
        }
    }
}
