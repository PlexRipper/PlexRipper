using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DownloadTaskMovieFileId",
                table: "DownloadWorkerTasks",
                type: "INTEGER",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "DownloadTaskTvShowEpisodeFileId",
                table: "DownloadWorkerTasks",
                type: "INTEGER",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "DownloadTaskMovie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    DataTotal = table.Column<long>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: true),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTaskMovie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTaskMovie_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskMovie_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DownloadTaskTvShow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    DataTotal = table.Column<long>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: true),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTaskTvShow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShow_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShow_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DownloadTaskMovieFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Percentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataReceived = table.Column<long>(type: "INTEGER", nullable: false),
                    DataTotal = table.Column<long>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    FileLocationUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Quality = table.Column<string>(type: "TEXT", nullable: true),
                    DownloadDirectory = table.Column<string>(type: "TEXT", nullable: true),
                    DestinationDirectory = table.Column<string>(type: "TEXT", nullable: true),
                    DownloadSpeed = table.Column<long>(type: "INTEGER", nullable: false),
                    FileTransferSpeed = table.Column<long>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    DestinationFolderId = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadFolderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTaskMovieFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTaskMovieFile_DownloadTaskMovie_ParentId",
                        column: x => x.ParentId,
                        principalTable: "DownloadTaskMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskMovieFile_FolderPaths_DestinationFolderId",
                        column: x => x.DestinationFolderId,
                        principalTable: "FolderPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskMovieFile_FolderPaths_DownloadFolderId",
                        column: x => x.DownloadFolderId,
                        principalTable: "FolderPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskMovieFile_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskMovieFile_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DownloadTaskTvShowSeason",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    DataTotal = table.Column<long>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: true),
                    DownloadTaskTvShowId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTaskTvShowSeason", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowSeason_DownloadTaskTvShow_DownloadTaskTvShowId",
                        column: x => x.DownloadTaskTvShowId,
                        principalTable: "DownloadTaskTvShow",
                        principalColumn: "Id"
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowSeason_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowSeason_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DownloadTaskTvShowEpisode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    DataTotal = table.Column<long>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: true),
                    DownloadTaskTvShowSeasonId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTaskTvShowEpisode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowEpisode_DownloadTaskTvShowSeason_DownloadTaskTvShowSeasonId",
                        column: x => x.DownloadTaskTvShowSeasonId,
                        principalTable: "DownloadTaskTvShowSeason",
                        principalColumn: "Id"
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowEpisode_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowEpisode_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DownloadTaskTvShowEpisodeFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Percentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataReceived = table.Column<long>(type: "INTEGER", nullable: false),
                    DataTotal = table.Column<long>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    FileLocationUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Quality = table.Column<string>(type: "TEXT", nullable: true),
                    DownloadDirectory = table.Column<string>(type: "TEXT", nullable: true),
                    DestinationDirectory = table.Column<string>(type: "TEXT", nullable: true),
                    DownloadSpeed = table.Column<long>(type: "INTEGER", nullable: false),
                    FileTransferSpeed = table.Column<long>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    DestinationFolderId = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadFolderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTaskTvShowEpisodeFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowEpisodeFile_DownloadTaskTvShowEpisode_ParentId",
                        column: x => x.ParentId,
                        principalTable: "DownloadTaskTvShowEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowEpisodeFile_FolderPaths_DestinationFolderId",
                        column: x => x.DestinationFolderId,
                        principalTable: "FolderPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowEpisodeFile_FolderPaths_DownloadFolderId",
                        column: x => x.DownloadFolderId,
                        principalTable: "FolderPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowEpisodeFile_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowEpisodeFile_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadWorkerTasks_DownloadTaskMovieFileId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskMovieFileId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadWorkerTasks_DownloadTaskTvShowEpisodeFileId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskTvShowEpisodeFileId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovie_PlexLibraryId",
                table: "DownloadTaskMovie",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovie_PlexServerId",
                table: "DownloadTaskMovie",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFile_DestinationFolderId",
                table: "DownloadTaskMovieFile",
                column: "DestinationFolderId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFile_DownloadFolderId",
                table: "DownloadTaskMovieFile",
                column: "DownloadFolderId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFile_ParentId",
                table: "DownloadTaskMovieFile",
                column: "ParentId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFile_PlexLibraryId",
                table: "DownloadTaskMovieFile",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFile_PlexServerId",
                table: "DownloadTaskMovieFile",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShow_PlexLibraryId",
                table: "DownloadTaskTvShow",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShow_PlexServerId",
                table: "DownloadTaskTvShow",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisode_DownloadTaskTvShowSeasonId",
                table: "DownloadTaskTvShowEpisode",
                column: "DownloadTaskTvShowSeasonId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisode_PlexLibraryId",
                table: "DownloadTaskTvShowEpisode",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisode_PlexServerId",
                table: "DownloadTaskTvShowEpisode",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_DestinationFolderId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "DestinationFolderId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_DownloadFolderId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "DownloadFolderId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_ParentId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "ParentId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_PlexLibraryId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_PlexServerId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowSeason_DownloadTaskTvShowId",
                table: "DownloadTaskTvShowSeason",
                column: "DownloadTaskTvShowId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowSeason_PlexLibraryId",
                table: "DownloadTaskTvShowSeason",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowSeason_PlexServerId",
                table: "DownloadTaskTvShowSeason",
                column: "PlexServerId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskMovieFile_DownloadTaskMovieFileId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskMovieFileId",
                principalTable: "DownloadTaskMovieFile",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskTvShowEpisodeFile_DownloadTaskTvShowEpisodeFileId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskTvShowEpisodeFileId",
                principalTable: "DownloadTaskTvShowEpisodeFile",
                principalColumn: "Id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskMovieFile_DownloadTaskMovieFileId",
                table: "DownloadWorkerTasks"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskTvShowEpisodeFile_DownloadTaskTvShowEpisodeFileId",
                table: "DownloadWorkerTasks"
            );

            migrationBuilder.DropTable(name: "DownloadTaskMovieFile");

            migrationBuilder.DropTable(name: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropTable(name: "DownloadTaskMovie");

            migrationBuilder.DropTable(name: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropTable(name: "DownloadTaskTvShowSeason");

            migrationBuilder.DropTable(name: "DownloadTaskTvShow");

            migrationBuilder.DropIndex(
                name: "IX_DownloadWorkerTasks_DownloadTaskMovieFileId",
                table: "DownloadWorkerTasks"
            );

            migrationBuilder.DropIndex(
                name: "IX_DownloadWorkerTasks_DownloadTaskTvShowEpisodeFileId",
                table: "DownloadWorkerTasks"
            );

            migrationBuilder.DropColumn(name: "DownloadTaskMovieFileId", table: "DownloadWorkerTasks");

            migrationBuilder.DropColumn(name: "DownloadTaskTvShowEpisodeFileId", table: "DownloadWorkerTasks");
        }
    }
}
