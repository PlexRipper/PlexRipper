using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedLibrarySyncJobQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LibrarySyncJobQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibrarySyncJobQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibrarySyncJobQueues_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_LibrarySyncJobQueues_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_LibrarySyncJobQueues_PlexLibraryId",
                table: "LibrarySyncJobQueues",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_LibrarySyncJobQueues_PlexServerId",
                table: "LibrarySyncJobQueues",
                column: "PlexServerId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "LibrarySyncJobQueues");
        }
    }
}
