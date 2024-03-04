using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetParentIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskMovieFile_DownloadTaskMovie_ParentId1",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowEpisode_DownloadTaskTvShowSeason_DownloadTaskTvShowSeasonId",
                table: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_DownloadTaskTvShowEpisode_ParentId1",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowSeason_DownloadTaskTvShow_DownloadTaskTvShowId",
                table: "DownloadTaskTvShowSeason");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowSeason_DownloadTaskTvShowId",
                table: "DownloadTaskTvShowSeason");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_ParentId1",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowEpisode_DownloadTaskTvShowSeasonId",
                table: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskMovieFile_ParentId1",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(
                name: "DownloadTaskTvShowId",
                table: "DownloadTaskTvShowSeason");

            migrationBuilder.DropColumn(
                name: "ParentId1",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(
                name: "DownloadTaskTvShowSeasonId",
                table: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropColumn(
                name: "ParentId1",
                table: "DownloadTaskMovieFile");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "DownloadTaskTvShowSeason",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "DownloadTaskTvShowEpisode",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "DownloadTaskMovieFile",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowSeason_ParentId",
                table: "DownloadTaskTvShowSeason",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_ParentId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisode_ParentId",
                table: "DownloadTaskTvShowEpisode",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFile_ParentId",
                table: "DownloadTaskMovieFile",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskMovieFile_DownloadTaskMovie_ParentId",
                table: "DownloadTaskMovieFile",
                column: "ParentId",
                principalTable: "DownloadTaskMovie",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowEpisode_DownloadTaskTvShowSeason_ParentId",
                table: "DownloadTaskTvShowEpisode",
                column: "ParentId",
                principalTable: "DownloadTaskTvShowSeason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_DownloadTaskTvShowEpisode_ParentId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "ParentId",
                principalTable: "DownloadTaskTvShowEpisode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowSeason_DownloadTaskTvShow_ParentId",
                table: "DownloadTaskTvShowSeason",
                column: "ParentId",
                principalTable: "DownloadTaskTvShow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskMovieFile_DownloadTaskMovie_ParentId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowEpisode_DownloadTaskTvShowSeason_ParentId",
                table: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_DownloadTaskTvShowEpisode_ParentId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowSeason_DownloadTaskTvShow_ParentId",
                table: "DownloadTaskTvShowSeason");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowSeason_ParentId",
                table: "DownloadTaskTvShowSeason");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_ParentId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowEpisode_ParentId",
                table: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskMovieFile_ParentId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "DownloadTaskTvShowSeason");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "DownloadTaskTvShowEpisode");

            migrationBuilder.AddColumn<Guid>(
                name: "DownloadTaskTvShowId",
                table: "DownloadTaskTvShowSeason",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ParentId",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId1",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DownloadTaskTvShowSeasonId",
                table: "DownloadTaskTvShowEpisode",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ParentId",
                table: "DownloadTaskMovieFile",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId1",
                table: "DownloadTaskMovieFile",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowSeason_DownloadTaskTvShowId",
                table: "DownloadTaskTvShowSeason",
                column: "DownloadTaskTvShowId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_ParentId1",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "ParentId1");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisode_DownloadTaskTvShowSeasonId",
                table: "DownloadTaskTvShowEpisode",
                column: "DownloadTaskTvShowSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFile_ParentId1",
                table: "DownloadTaskMovieFile",
                column: "ParentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskMovieFile_DownloadTaskMovie_ParentId1",
                table: "DownloadTaskMovieFile",
                column: "ParentId1",
                principalTable: "DownloadTaskMovie",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowEpisode_DownloadTaskTvShowSeason_DownloadTaskTvShowSeasonId",
                table: "DownloadTaskTvShowEpisode",
                column: "DownloadTaskTvShowSeasonId",
                principalTable: "DownloadTaskTvShowSeason",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_DownloadTaskTvShowEpisode_ParentId1",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "ParentId1",
                principalTable: "DownloadTaskTvShowEpisode",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowSeason_DownloadTaskTvShow_DownloadTaskTvShowId",
                table: "DownloadTaskTvShowSeason",
                column: "DownloadTaskTvShowId",
                principalTable: "DownloadTaskTvShow",
                principalColumn: "Id");
        }
    }
}
