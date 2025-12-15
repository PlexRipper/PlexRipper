using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LibrarySyncJobQueues_PlexLibraries_PlexLibraryId",
                table: "LibrarySyncJobQueues"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_LibrarySyncJobQueues_PlexServers_PlexServerId",
                table: "LibrarySyncJobQueues"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_LibrarySyncJobQueues", table: "LibrarySyncJobQueues");

            migrationBuilder.RenameTable(name: "LibrarySyncJobQueues", newName: "BackgroundJobLibrarySyncJobQueues");

            migrationBuilder.RenameIndex(
                name: "IX_LibrarySyncJobQueues_PlexLibraryId",
                table: "BackgroundJobLibrarySyncJobQueues",
                newName: "IX_BackgroundJobLibrarySyncJobQueues_PlexLibraryId"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_BackgroundJobLibrarySyncJobQueues",
                table: "BackgroundJobLibrarySyncJobQueues",
                columns: new[] { "PlexServerId", "PlexLibraryId" }
            );

            migrationBuilder.AddForeignKey(
                name: "FK_BackgroundJobLibrarySyncJobQueues_PlexLibraries_PlexLibraryId",
                table: "BackgroundJobLibrarySyncJobQueues",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_BackgroundJobLibrarySyncJobQueues_PlexServers_PlexServerId",
                table: "BackgroundJobLibrarySyncJobQueues",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BackgroundJobLibrarySyncJobQueues_PlexLibraries_PlexLibraryId",
                table: "BackgroundJobLibrarySyncJobQueues"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_BackgroundJobLibrarySyncJobQueues_PlexServers_PlexServerId",
                table: "BackgroundJobLibrarySyncJobQueues"
            );

            migrationBuilder.DropPrimaryKey(
                name: "PK_BackgroundJobLibrarySyncJobQueues",
                table: "BackgroundJobLibrarySyncJobQueues"
            );

            migrationBuilder.RenameTable(name: "BackgroundJobLibrarySyncJobQueues", newName: "LibrarySyncJobQueues");

            migrationBuilder.RenameIndex(
                name: "IX_BackgroundJobLibrarySyncJobQueues_PlexLibraryId",
                table: "LibrarySyncJobQueues",
                newName: "IX_LibrarySyncJobQueues_PlexLibraryId"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_LibrarySyncJobQueues",
                table: "LibrarySyncJobQueues",
                columns: new[] { "PlexServerId", "PlexLibraryId" }
            );

            migrationBuilder.AddForeignKey(
                name: "FK_LibrarySyncJobQueues_PlexLibraries_PlexLibraryId",
                table: "LibrarySyncJobQueues",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_LibrarySyncJobQueues_PlexServers_PlexServerId",
                table: "LibrarySyncJobQueues",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
