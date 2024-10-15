using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakePlexServerConnectionIdUniqueInStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlexServerStatuses_PlexServerConnectionId",
                table: "PlexServerStatuses"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexServerStatuses_PlexServerConnectionId",
                table: "PlexServerStatuses",
                column: "PlexServerConnectionId",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlexServerStatuses_PlexServerConnectionId",
                table: "PlexServerStatuses"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexServerStatuses_PlexServerConnectionId",
                table: "PlexServerStatuses",
                column: "PlexServerConnectionId"
            );
        }
    }
}
