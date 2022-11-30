using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    public partial class AddedPlexServerConnectionsToPlexServer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlexServerConnection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Protocol = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    Local = table.Column<bool>(type: "INTEGER", nullable: false),
                    Relay = table.Column<bool>(type: "INTEGER", nullable: false),
                    IPv6 = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexServerConnection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexServerConnection_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlexServerConnection_PlexServerId",
                table: "PlexServerConnection",
                column: "PlexServerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlexServerConnection");
        }
    }
}
