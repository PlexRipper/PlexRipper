using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlexLibraryAccessHistoryEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlexLibraryAccessHistoryEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RefreshRunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlexAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexAccountNameSnapshot = table.Column<string>(type: "TEXT", nullable: true),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlexServerNameSnapshot = table.Column<string>(type: "TEXT", nullable: true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlexLibraryNameSnapshot = table.Column<string>(type: "TEXT", nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexLibraryAccessHistoryEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlexLibraryAccessHistoryEvents_PlexAccountId_OccurredAtUtc",
                table: "PlexLibraryAccessHistoryEvents",
                columns: new[] { "PlexAccountId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexLibraryAccessHistoryEvents_PlexAccountId_PlexLibraryId_OccurredAtUtc",
                table: "PlexLibraryAccessHistoryEvents",
                columns: new[] { "PlexAccountId", "PlexLibraryId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexLibraryAccessHistoryEvents_PlexAccountId_PlexServerId_OccurredAtUtc",
                table: "PlexLibraryAccessHistoryEvents",
                columns: new[] { "PlexAccountId", "PlexServerId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexLibraryAccessHistoryEvents_RefreshRunId",
                table: "PlexLibraryAccessHistoryEvents",
                column: "RefreshRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlexLibraryAccessHistoryEvents");
        }
    }
}
