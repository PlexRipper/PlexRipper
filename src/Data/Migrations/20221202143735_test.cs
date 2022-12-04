using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    public partial class test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexServerConnections_PlexServers_PlexServerId",
                table: "PlexServerConnections");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexServerConnections_PlexServers_PlexServerId",
                table: "PlexServerConnections",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexServerConnections_PlexServers_PlexServerId",
                table: "PlexServerConnections");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexServerConnections_PlexServers_PlexServerId",
                table: "PlexServerConnections",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
