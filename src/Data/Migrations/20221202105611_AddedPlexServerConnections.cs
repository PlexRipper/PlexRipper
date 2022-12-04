using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    public partial class AddedPlexServerConnections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexServerConnection_PlexServers_PlexServerId",
                table: "PlexServerConnection");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlexServerConnection",
                table: "PlexServerConnection");

            migrationBuilder.RenameTable(
                name: "PlexServerConnection",
                newName: "PlexServerConnections");

            migrationBuilder.RenameIndex(
                name: "IX_PlexServerConnection_PlexServerId",
                table: "PlexServerConnections",
                newName: "IX_PlexServerConnections_PlexServerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexServerConnections",
                table: "PlexServerConnections",
                column: "Id");

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

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlexServerConnections",
                table: "PlexServerConnections");

            migrationBuilder.RenameTable(
                name: "PlexServerConnections",
                newName: "PlexServerConnection");

            migrationBuilder.RenameIndex(
                name: "IX_PlexServerConnections_PlexServerId",
                table: "PlexServerConnection",
                newName: "IX_PlexServerConnection_PlexServerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexServerConnection",
                table: "PlexServerConnection",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexServerConnection_PlexServers_PlexServerId",
                table: "PlexServerConnection",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
