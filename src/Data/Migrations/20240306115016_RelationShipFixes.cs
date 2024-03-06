using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class RelationShipFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskMovieFile_FolderPaths_DestinationFolderId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskMovieFile_FolderPaths_DownloadFolderId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_FolderPaths_DestinationFolderId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_FolderPaths_DownloadFolderId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTasks_DownloadTaskId",
                table: "DownloadWorkerTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_FileTasks_DownloadTasks_DownloadTaskId",
                table: "FileTasks");

            migrationBuilder.DropTable(
                name: "DownloadTasks");

            migrationBuilder.DropIndex(
                name: "IX_FileTasks_DownloadTaskId",
                table: "FileTasks");

            migrationBuilder.DropIndex(
                name: "IX_DownloadWorkerTasks_DownloadTaskFileBaseId",
                table: "DownloadWorkerTasks");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_DestinationFolderId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_DownloadFolderId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskMovieFile_DestinationFolderId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskMovieFile_DownloadFolderId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(
                name: "DownloadTaskFileBaseId",
                table: "DownloadWorkerTasks");

            migrationBuilder.DropColumn(
                name: "DestinationFolderId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(
                name: "DownloadFolderId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(
                name: "DestinationFolderId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(
                name: "DownloadFolderId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.AlterColumn<Guid>(
                name: "DownloadTaskId",
                table: "FileTasks",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "DestinationDirectory",
                table: "FileTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DownloadTaskType",
                table: "FileTasks",
                type: "TEXT",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "FileTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "FileTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "PlexLibraryId",
                table: "FileTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlexServerId",
                table: "FileTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "DownloadTaskId",
                table: "DownloadWorkerTasks",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "DownloadTaskTvShowSeason",
                type: "TEXT",
                nullable: true,
                collation: "NATURALSORT",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DownloadStatus",
                table: "DownloadTaskTvShowSeason",
                type: "TEXT",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "TEXT",
                nullable: true,
                collation: "NATURALSORT",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DownloadStatus",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "TEXT",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "DownloadTaskTvShowEpisode",
                type: "TEXT",
                nullable: true,
                collation: "NATURALSORT",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DownloadStatus",
                table: "DownloadTaskTvShowEpisode",
                type: "TEXT",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "DownloadTaskTvShow",
                type: "TEXT",
                nullable: true,
                collation: "NATURALSORT",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DownloadStatus",
                table: "DownloadTaskTvShow",
                type: "TEXT",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "DownloadTaskMovieFile",
                type: "TEXT",
                nullable: true,
                collation: "NATURALSORT",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DownloadStatus",
                table: "DownloadTaskMovieFile",
                type: "TEXT",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "DownloadTaskMovie",
                type: "TEXT",
                nullable: true,
                collation: "NATURALSORT",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DownloadStatus",
                table: "DownloadTaskMovie",
                type: "TEXT",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_FileTasks_PlexLibraryId",
                table: "FileTasks",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_FileTasks_PlexServerId",
                table: "FileTasks",
                column: "PlexServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskMovieFile_DownloadTaskId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskId",
                principalTable: "DownloadTaskMovieFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskTvShowEpisodeFile_DownloadTaskId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskId",
                principalTable: "DownloadTaskTvShowEpisodeFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FileTasks_PlexLibraries_PlexLibraryId",
                table: "FileTasks",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FileTasks_PlexServers_PlexServerId",
                table: "FileTasks",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskMovieFile_DownloadTaskId",
                table: "DownloadWorkerTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskTvShowEpisodeFile_DownloadTaskId",
                table: "DownloadWorkerTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_FileTasks_PlexLibraries_PlexLibraryId",
                table: "FileTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_FileTasks_PlexServers_PlexServerId",
                table: "FileTasks");

            migrationBuilder.DropIndex(
                name: "IX_FileTasks_PlexLibraryId",
                table: "FileTasks");

            migrationBuilder.DropIndex(
                name: "IX_FileTasks_PlexServerId",
                table: "FileTasks");

            migrationBuilder.DropColumn(
                name: "DestinationDirectory",
                table: "FileTasks");

            migrationBuilder.DropColumn(
                name: "DownloadTaskType",
                table: "FileTasks");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "FileTasks");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "FileTasks");

            migrationBuilder.DropColumn(
                name: "PlexLibraryId",
                table: "FileTasks");

            migrationBuilder.DropColumn(
                name: "PlexServerId",
                table: "FileTasks");

            migrationBuilder.AlterColumn<int>(
                name: "DownloadTaskId",
                table: "FileTasks",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "DownloadTaskId",
                table: "DownloadWorkerTasks",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<Guid>(
                name: "DownloadTaskFileBaseId",
                table: "DownloadWorkerTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "DownloadTaskTvShowSeason",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldCollation: "NATURALSORT");

            migrationBuilder.AlterColumn<int>(
                name: "DownloadStatus",
                table: "DownloadTaskTvShowSeason",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldCollation: "NATURALSORT");

            migrationBuilder.AlterColumn<int>(
                name: "DownloadStatus",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "DestinationFolderId",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DownloadFolderId",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "DownloadTaskTvShowEpisode",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldCollation: "NATURALSORT");

            migrationBuilder.AlterColumn<int>(
                name: "DownloadStatus",
                table: "DownloadTaskTvShowEpisode",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "DownloadTaskTvShow",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldCollation: "NATURALSORT");

            migrationBuilder.AlterColumn<int>(
                name: "DownloadStatus",
                table: "DownloadTaskTvShow",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "DownloadTaskMovieFile",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldCollation: "NATURALSORT");

            migrationBuilder.AlterColumn<int>(
                name: "DownloadStatus",
                table: "DownloadTaskMovieFile",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "DestinationFolderId",
                table: "DownloadTaskMovieFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DownloadFolderId",
                table: "DownloadTaskMovieFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "DownloadTaskMovie",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldCollation: "NATURALSORT");

            migrationBuilder.AlterColumn<int>(
                name: "DownloadStatus",
                table: "DownloadTaskMovie",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.CreateTable(
                name: "DownloadTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true, collation: "NATURALSORT"),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Percentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataReceived = table.Column<long>(type: "INTEGER", nullable: false),
                    DataTotal = table.Column<long>(type: "INTEGER", nullable: false),
                    MediaType = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    DownloadStatus = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    DownloadTaskType = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    FileLocationUrl = table.Column<string>(type: "TEXT", nullable: true),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: true),
                    Quality = table.Column<string>(type: "TEXT", nullable: true),
                    DownloadDirectory = table.Column<string>(type: "TEXT", nullable: true),
                    DestinationDirectory = table.Column<string>(type: "TEXT", nullable: true),
                    DownloadSpeed = table.Column<long>(type: "INTEGER", nullable: false),
                    FileTransferSpeed = table.Column<long>(type: "INTEGER", nullable: false),
                    ServerMachineIdentifier = table.Column<string>(type: "TEXT", nullable: true),
                    DestinationFolderId = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadFolderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<long>(type: "INTEGER", nullable: false),
                    RootDownloadTaskId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTasks_DownloadTasks_ParentId",
                        column: x => x.ParentId,
                        principalTable: "DownloadTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadTasks_FolderPaths_DestinationFolderId",
                        column: x => x.DestinationFolderId,
                        principalTable: "FolderPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadTasks_FolderPaths_DownloadFolderId",
                        column: x => x.DownloadFolderId,
                        principalTable: "FolderPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadTasks_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadTasks_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileTasks_DownloadTaskId",
                table: "FileTasks",
                column: "DownloadTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadWorkerTasks_DownloadTaskFileBaseId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskFileBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_DestinationFolderId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "DestinationFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_DownloadFolderId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "DownloadFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFile_DestinationFolderId",
                table: "DownloadTaskMovieFile",
                column: "DestinationFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFile_DownloadFolderId",
                table: "DownloadTaskMovieFile",
                column: "DownloadFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTasks_DestinationFolderId",
                table: "DownloadTasks",
                column: "DestinationFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTasks_DownloadFolderId",
                table: "DownloadTasks",
                column: "DownloadFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTasks_ParentId",
                table: "DownloadTasks",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTasks_PlexLibraryId",
                table: "DownloadTasks",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTasks_PlexServerId",
                table: "DownloadTasks",
                column: "PlexServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskMovieFile_FolderPaths_DestinationFolderId",
                table: "DownloadTaskMovieFile",
                column: "DestinationFolderId",
                principalTable: "FolderPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskMovieFile_FolderPaths_DownloadFolderId",
                table: "DownloadTaskMovieFile",
                column: "DownloadFolderId",
                principalTable: "FolderPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_FolderPaths_DestinationFolderId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "DestinationFolderId",
                principalTable: "FolderPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_FolderPaths_DownloadFolderId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "DownloadFolderId",
                principalTable: "FolderPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTasks_DownloadTaskId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskId",
                principalTable: "DownloadTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FileTasks_DownloadTasks_DownloadTaskId",
                table: "FileTasks",
                column: "DownloadTaskId",
                principalTable: "DownloadTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
