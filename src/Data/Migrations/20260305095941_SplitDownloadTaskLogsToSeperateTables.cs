using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitDownloadTaskLogsToSeperateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadTasksLogs");

            migrationBuilder.CreateTable(
                name: "DownloadTaskMovieFileLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false, defaultValue: "Unknown"),
                    LogLevel = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false, defaultValue: "None"),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DownloadTaskFileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DownloadTaskMovieId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTaskMovieFileLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTaskMovieFileLogs_DownloadTaskMovieFile_DownloadTaskFileId",
                        column: x => x.DownloadTaskFileId,
                        principalTable: "DownloadTaskMovieFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadTaskMovieFileLogs_DownloadTaskMovie_DownloadTaskMovieId",
                        column: x => x.DownloadTaskMovieId,
                        principalTable: "DownloadTaskMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DownloadTaskTvShowEpisodeFileLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false, defaultValue: "Unknown"),
                    LogLevel = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false, defaultValue: "None"),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DownloadTaskFileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DownloadTaskTvShowEpisodeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DownloadTaskTvShowSeasonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DownloadTaskTvShowId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTaskTvShowEpisodeFileLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowEpisodeFileLogs_DownloadTaskTvShowEpisodeFile_DownloadTaskFileId",
                        column: x => x.DownloadTaskFileId,
                        principalTable: "DownloadTaskTvShowEpisodeFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowEpisodeFileLogs_DownloadTaskTvShowEpisode_DownloadTaskTvShowEpisodeId",
                        column: x => x.DownloadTaskTvShowEpisodeId,
                        principalTable: "DownloadTaskTvShowEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowEpisodeFileLogs_DownloadTaskTvShowSeason_DownloadTaskTvShowSeasonId",
                        column: x => x.DownloadTaskTvShowSeasonId,
                        principalTable: "DownloadTaskTvShowSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowEpisodeFileLogs_DownloadTaskTvShow_DownloadTaskTvShowId",
                        column: x => x.DownloadTaskTvShowId,
                        principalTable: "DownloadTaskTvShow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFileLogs_DownloadTaskFileId",
                table: "DownloadTaskMovieFileLogs",
                column: "DownloadTaskFileId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFileLogs_DownloadTaskMovieId",
                table: "DownloadTaskMovieFileLogs",
                column: "DownloadTaskMovieId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFileLogs_DownloadTaskFileId",
                table: "DownloadTaskTvShowEpisodeFileLogs",
                column: "DownloadTaskFileId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFileLogs_DownloadTaskTvShowEpisodeId",
                table: "DownloadTaskTvShowEpisodeFileLogs",
                column: "DownloadTaskTvShowEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFileLogs_DownloadTaskTvShowId",
                table: "DownloadTaskTvShowEpisodeFileLogs",
                column: "DownloadTaskTvShowId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFileLogs_DownloadTaskTvShowSeasonId",
                table: "DownloadTaskTvShowEpisodeFileLogs",
                column: "DownloadTaskTvShowSeasonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadTaskMovieFileLogs");

            migrationBuilder.DropTable(
                name: "DownloadTaskTvShowEpisodeFileLogs");

            migrationBuilder.CreateTable(
                name: "DownloadTasksLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false, defaultValue: "Unknown"),
                    LogLevel = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false, defaultValue: "None"),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DownloadTaskId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTasksLogs", x => x.Id);
                });
        }
    }
}
