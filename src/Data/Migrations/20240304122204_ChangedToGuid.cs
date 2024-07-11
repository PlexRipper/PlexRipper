using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangedToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskMovieFile_DownloadTaskMovie_ParentId",
                table: "DownloadTaskMovieFile"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_DownloadTaskTvShowEpisode_ParentId",
                table: "DownloadTaskTvShowEpisodeFile"
            );

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_ParentId",
                table: "DownloadTaskTvShowEpisodeFile"
            );

            migrationBuilder.DropIndex(name: "IX_DownloadTaskMovieFile_ParentId", table: "DownloadTaskMovieFile");

            migrationBuilder.AlterColumn<Guid>(
                name: "DownloadTaskFileBaseId",
                table: "DownloadWorkerTasks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "DownloadTaskTvShowId",
                table: "DownloadTaskTvShowSeason",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "DownloadTaskTvShowSeason",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId1",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "DownloadTaskTvShowSeasonId",
                table: "DownloadTaskTvShowEpisode",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "DownloadTaskTvShowEpisode",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "DownloadTaskTvShow",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "DownloadTaskMovieFile",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId1",
                table: "DownloadTaskMovieFile",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "DownloadTaskMovie",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_ParentId1",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "ParentId1"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFile_ParentId1",
                table: "DownloadTaskMovieFile",
                column: "ParentId1"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskMovieFile_DownloadTaskMovie_ParentId1",
                table: "DownloadTaskMovieFile",
                column: "ParentId1",
                principalTable: "DownloadTaskMovie",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_DownloadTaskTvShowEpisode_ParentId1",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "ParentId1",
                principalTable: "DownloadTaskTvShowEpisode",
                principalColumn: "Id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskMovieFile_DownloadTaskMovie_ParentId1",
                table: "DownloadTaskMovieFile"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_DownloadTaskTvShowEpisode_ParentId1",
                table: "DownloadTaskTvShowEpisodeFile"
            );

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_ParentId1",
                table: "DownloadTaskTvShowEpisodeFile"
            );

            migrationBuilder.DropIndex(name: "IX_DownloadTaskMovieFile_ParentId1", table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(name: "ParentId1", table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(name: "ParentId1", table: "DownloadTaskMovieFile");

            migrationBuilder.AlterColumn<int>(
                name: "DownloadTaskFileBaseId",
                table: "DownloadWorkerTasks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<int>(
                name: "DownloadTaskTvShowId",
                table: "DownloadTaskTvShowSeason",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DownloadTaskTvShowSeason",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT"
            );

            migrationBuilder.AlterColumn<int>(
                name: "DownloadTaskTvShowSeasonId",
                table: "DownloadTaskTvShowEpisode",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DownloadTaskTvShowEpisode",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DownloadTaskTvShow",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DownloadTaskMovieFile",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DownloadTaskMovie",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_ParentId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "ParentId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFile_ParentId",
                table: "DownloadTaskMovieFile",
                column: "ParentId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskMovieFile_DownloadTaskMovie_ParentId",
                table: "DownloadTaskMovieFile",
                column: "ParentId",
                principalTable: "DownloadTaskMovie",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_DownloadTaskTvShowEpisode_ParentId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "ParentId",
                principalTable: "DownloadTaskTvShowEpisode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
