using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlexMediaComparisonTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: true)
                .Annotation("Relational:ColumnOrder", 21);

            migrationBuilder.CreateTable(
                name: "BackgroundJobLibraryComparisonJobQueues",
                columns: table => new
                {
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    RemotePlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedPlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaType = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundJobLibraryComparisonJobQueues", x => new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.MediaType });
                    table.ForeignKey(
                        name: "FK_BackgroundJobLibraryComparisonJobQueues_PlexLibraries_OwnedPlexLibraryId",
                        column: x => x.OwnedPlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BackgroundJobLibraryComparisonJobQueues_PlexLibraries_RemotePlexLibraryId",
                        column: x => x.RemotePlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexComparisonScopes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RemotePlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedPlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaType = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RemoteLibraryUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OwnedLibraryUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexComparisonScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexComparisonScopes_PlexLibraries_OwnedPlexLibraryId",
                        column: x => x.OwnedPlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexComparisonScopes_PlexLibraries_RemotePlexLibraryId",
                        column: x => x.RemotePlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexEpisodeComparisons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RemotePlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedPlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    RemotePlexMediaId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedPlexMediaId = table.Column<int>(type: "INTEGER", nullable: false),
                    HitState = table.Column<int>(type: "INTEGER", nullable: false),
                    RemoteQuality = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedQuality = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchType = table.Column<int>(type: "INTEGER", nullable: false),
                    ComparedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexEpisodeComparisons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexEpisodeComparisons_PlexLibraries_OwnedPlexLibraryId",
                        column: x => x.OwnedPlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexEpisodeComparisons_PlexLibraries_RemotePlexLibraryId",
                        column: x => x.RemotePlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexEpisodeComparisons_PlexTvShowEpisodes_OwnedPlexMediaId",
                        column: x => x.OwnedPlexMediaId,
                        principalTable: "PlexTvShowEpisodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexEpisodeComparisons_PlexTvShowEpisodes_RemotePlexMediaId",
                        column: x => x.RemotePlexMediaId,
                        principalTable: "PlexTvShowEpisodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexMovieComparisons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RemotePlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedPlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    RemotePlexMediaId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedPlexMediaId = table.Column<int>(type: "INTEGER", nullable: false),
                    HitState = table.Column<int>(type: "INTEGER", nullable: false),
                    RemoteQuality = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedQuality = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchType = table.Column<int>(type: "INTEGER", nullable: false),
                    ComparedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieComparisons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexMovieComparisons_PlexLibraries_OwnedPlexLibraryId",
                        column: x => x.OwnedPlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexMovieComparisons_PlexLibraries_RemotePlexLibraryId",
                        column: x => x.RemotePlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexMovieComparisons_PlexMovie_OwnedPlexMediaId",
                        column: x => x.OwnedPlexMediaId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexMovieComparisons_PlexMovie_RemotePlexMediaId",
                        column: x => x.RemotePlexMediaId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexSeasonComparisons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RemotePlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedPlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    RemotePlexMediaId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedPlexMediaId = table.Column<int>(type: "INTEGER", nullable: false),
                    HitState = table.Column<int>(type: "INTEGER", nullable: false),
                    RemoteQuality = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedQuality = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchType = table.Column<int>(type: "INTEGER", nullable: false),
                    ComparedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexSeasonComparisons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexSeasonComparisons_PlexLibraries_OwnedPlexLibraryId",
                        column: x => x.OwnedPlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexSeasonComparisons_PlexLibraries_RemotePlexLibraryId",
                        column: x => x.RemotePlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexSeasonComparisons_PlexTvShowSeason_OwnedPlexMediaId",
                        column: x => x.OwnedPlexMediaId,
                        principalTable: "PlexTvShowSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexSeasonComparisons_PlexTvShowSeason_RemotePlexMediaId",
                        column: x => x.RemotePlexMediaId,
                        principalTable: "PlexTvShowSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexTvShowComparisons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RemotePlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedPlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    RemotePlexMediaId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedPlexMediaId = table.Column<int>(type: "INTEGER", nullable: false),
                    HitState = table.Column<int>(type: "INTEGER", nullable: false),
                    RemoteQuality = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedQuality = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchType = table.Column<int>(type: "INTEGER", nullable: false),
                    ComparedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowComparisons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShowComparisons_PlexLibraries_OwnedPlexLibraryId",
                        column: x => x.OwnedPlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexTvShowComparisons_PlexLibraries_RemotePlexLibraryId",
                        column: x => x.RemotePlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexTvShowComparisons_PlexTvShows_OwnedPlexMediaId",
                        column: x => x.OwnedPlexMediaId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexTvShowComparisons_PlexTvShows_RemotePlexMediaId",
                        column: x => x.RemotePlexMediaId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobLibraryComparisonJobQueues_OwnedPlexLibraryId",
                table: "BackgroundJobLibraryComparisonJobQueues",
                column: "OwnedPlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobLibraryComparisonJobQueues_Status_Priority_CreatedAt",
                table: "BackgroundJobLibraryComparisonJobQueues",
                columns: new[] { "Status", "Priority", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexComparisonScopes_OwnedPlexLibraryId",
                table: "PlexComparisonScopes",
                column: "OwnedPlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexComparisonScopes_RemotePlexLibraryId_OwnedPlexLibraryId_MediaType",
                table: "PlexComparisonScopes",
                columns: new[] { "RemotePlexLibraryId", "OwnedPlexLibraryId", "MediaType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexEpisodeComparisons_OwnedPlexLibraryId_OwnedPlexMediaId",
                table: "PlexEpisodeComparisons",
                columns: new[] { "OwnedPlexLibraryId", "OwnedPlexMediaId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexEpisodeComparisons_OwnedPlexMediaId",
                table: "PlexEpisodeComparisons",
                column: "OwnedPlexMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexEpisodeComparisons_RemotePlexLibraryId_OwnedPlexLibraryId_HitState",
                table: "PlexEpisodeComparisons",
                columns: new[] { "RemotePlexLibraryId", "OwnedPlexLibraryId", "HitState" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexEpisodeComparisons_RemotePlexLibraryId_OwnedPlexLibraryId_RemotePlexMediaId_OwnedPlexMediaId",
                table: "PlexEpisodeComparisons",
                columns: new[] { "RemotePlexLibraryId", "OwnedPlexLibraryId", "RemotePlexMediaId", "OwnedPlexMediaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexEpisodeComparisons_RemotePlexMediaId",
                table: "PlexEpisodeComparisons",
                column: "RemotePlexMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieComparisons_OwnedPlexLibraryId_OwnedPlexMediaId",
                table: "PlexMovieComparisons",
                columns: new[] { "OwnedPlexLibraryId", "OwnedPlexMediaId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieComparisons_OwnedPlexMediaId",
                table: "PlexMovieComparisons",
                column: "OwnedPlexMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieComparisons_RemotePlexLibraryId_OwnedPlexLibraryId_HitState",
                table: "PlexMovieComparisons",
                columns: new[] { "RemotePlexLibraryId", "OwnedPlexLibraryId", "HitState" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieComparisons_RemotePlexLibraryId_OwnedPlexLibraryId_RemotePlexMediaId_OwnedPlexMediaId",
                table: "PlexMovieComparisons",
                columns: new[] { "RemotePlexLibraryId", "OwnedPlexLibraryId", "RemotePlexMediaId", "OwnedPlexMediaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieComparisons_RemotePlexMediaId",
                table: "PlexMovieComparisons",
                column: "RemotePlexMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexSeasonComparisons_OwnedPlexLibraryId_OwnedPlexMediaId",
                table: "PlexSeasonComparisons",
                columns: new[] { "OwnedPlexLibraryId", "OwnedPlexMediaId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexSeasonComparisons_OwnedPlexMediaId",
                table: "PlexSeasonComparisons",
                column: "OwnedPlexMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexSeasonComparisons_RemotePlexLibraryId_OwnedPlexLibraryId_HitState",
                table: "PlexSeasonComparisons",
                columns: new[] { "RemotePlexLibraryId", "OwnedPlexLibraryId", "HitState" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexSeasonComparisons_RemotePlexLibraryId_OwnedPlexLibraryId_RemotePlexMediaId_OwnedPlexMediaId",
                table: "PlexSeasonComparisons",
                columns: new[] { "RemotePlexLibraryId", "OwnedPlexLibraryId", "RemotePlexMediaId", "OwnedPlexMediaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexSeasonComparisons_RemotePlexMediaId",
                table: "PlexSeasonComparisons",
                column: "RemotePlexMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowComparisons_OwnedPlexLibraryId_OwnedPlexMediaId",
                table: "PlexTvShowComparisons",
                columns: new[] { "OwnedPlexLibraryId", "OwnedPlexMediaId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowComparisons_OwnedPlexMediaId",
                table: "PlexTvShowComparisons",
                column: "OwnedPlexMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowComparisons_RemotePlexLibraryId_OwnedPlexLibraryId_HitState",
                table: "PlexTvShowComparisons",
                columns: new[] { "RemotePlexLibraryId", "OwnedPlexLibraryId", "HitState" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowComparisons_RemotePlexLibraryId_OwnedPlexLibraryId_RemotePlexMediaId_OwnedPlexMediaId",
                table: "PlexTvShowComparisons",
                columns: new[] { "RemotePlexLibraryId", "OwnedPlexLibraryId", "RemotePlexMediaId", "OwnedPlexMediaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowComparisons_RemotePlexMediaId",
                table: "PlexTvShowComparisons",
                column: "RemotePlexMediaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackgroundJobLibraryComparisonJobQueues");

            migrationBuilder.DropTable(
                name: "PlexComparisonScopes");

            migrationBuilder.DropTable(
                name: "PlexEpisodeComparisons");

            migrationBuilder.DropTable(
                name: "PlexMovieComparisons");

            migrationBuilder.DropTable(
                name: "PlexSeasonComparisons");

            migrationBuilder.DropTable(
                name: "PlexTvShowComparisons");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "PlexLibraries");
        }
    }
}
