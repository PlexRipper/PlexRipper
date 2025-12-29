using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameLastUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "PlexTvShowEpisodeDataParts",
                newName: "LastSyncedAt"
            );

            migrationBuilder.RenameColumn(name: "UpdatedAt", table: "PlexMovieDataParts", newName: "LastSyncedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastSyncedAt",
                table: "PlexTvShowEpisodeDataParts",
                newName: "UpdatedAt"
            );

            migrationBuilder.RenameColumn(name: "LastSyncedAt", table: "PlexMovieDataParts", newName: "UpdatedAt");
        }
    }
}
