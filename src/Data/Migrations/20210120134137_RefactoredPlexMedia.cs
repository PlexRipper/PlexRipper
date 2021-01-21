using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class RefactoredPlexMedia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowGenre_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowGenre");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowRole_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowRole");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShows_PlexLibraries_PlexLibraryId",
                table: "PlexTvShows");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowSeason_PlexTvShows_TvShowId",
                table: "PlexTvShowSeason");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlexTvShows",
                table: "PlexTvShows");

            migrationBuilder.DropColumn(
                name: "Art",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "Banner",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "Thumb",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "Art",
                table: "PlexMovie");

            migrationBuilder.DropColumn(
                name: "Banner",
                table: "PlexMovie");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "PlexMovie");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "PlexMovie");

            migrationBuilder.DropColumn(
                name: "Thumb",
                table: "PlexMovie");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "PlexMovie");

            migrationBuilder.DropColumn(
                name: "Art",
                table: "PlexTvShows");

            migrationBuilder.DropColumn(
                name: "Banner",
                table: "PlexTvShows");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "PlexTvShows");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "PlexTvShows");

            migrationBuilder.DropColumn(
                name: "Thumb",
                table: "PlexTvShows");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "PlexTvShows");

            migrationBuilder.RenameTable(
                name: "PlexTvShows",
                newName: "PlexTvShow");

            migrationBuilder.RenameColumn(
                name: "ViewedLeafCount",
                table: "PlexTvShowSeason",
                newName: "Year");

            migrationBuilder.RenameColumn(
                name: "LeafCount",
                table: "PlexTvShowSeason",
                newName: "PlexServerId");

            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "PlexTvShowSeason",
                newName: "Studio");

            migrationBuilder.RenameColumn(
                name: "ViewedLeafCount",
                table: "PlexTvShowEpisodes",
                newName: "PlexServerId");

            migrationBuilder.RenameColumn(
                name: "LeafCount",
                table: "PlexTvShowEpisodes",
                newName: "MetaDataKey");

            migrationBuilder.RenameColumn(
                name: "ViewedLeafCount",
                table: "PlexMovie",
                newName: "PlexServerId");

            migrationBuilder.RenameColumn(
                name: "LeafCount",
                table: "PlexMovie",
                newName: "MetaDataKey");

            migrationBuilder.RenameColumn(
                name: "ViewedLeafCount",
                table: "PlexTvShow",
                newName: "PlexServerId");

            migrationBuilder.RenameColumn(
                name: "LeafCount",
                table: "PlexTvShow",
                newName: "MetaDataKey");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShows_PlexLibraryId",
                table: "PlexTvShow",
                newName: "IX_PlexTvShow_PlexLibraryId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OriginallyAvailableAt",
                table: "PlexTvShowSeason",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "ContentRating",
                table: "PlexTvShowSeason",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "PlexTvShowSeason",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasArt",
                table: "PlexTvShowSeason",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasBanner",
                table: "PlexTvShowSeason",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasTheme",
                table: "PlexTvShowSeason",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasThumb",
                table: "PlexTvShowSeason",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MetaDataKey",
                table: "PlexTvShowSeason",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "PlexTvShowSeason",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OriginallyAvailableAt",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<bool>(
                name: "HasArt",
                table: "PlexTvShowEpisodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasBanner",
                table: "PlexTvShowEpisodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasTheme",
                table: "PlexTvShowEpisodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasThumb",
                table: "PlexTvShowEpisodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OriginallyAvailableAt",
                table: "PlexMovie",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<bool>(
                name: "HasArt",
                table: "PlexMovie",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasBanner",
                table: "PlexMovie",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasTheme",
                table: "PlexMovie",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasThumb",
                table: "PlexMovie",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OriginallyAvailableAt",
                table: "PlexTvShow",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<bool>(
                name: "HasArt",
                table: "PlexTvShow",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasBanner",
                table: "PlexTvShow",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasTheme",
                table: "PlexTvShow",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasThumb",
                table: "PlexTvShow",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexTvShow",
                table: "PlexTvShow",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowSeason_PlexServerId",
                table: "PlexTvShowSeason",
                column: "PlexServerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodes_PlexServerId",
                table: "PlexTvShowEpisodes",
                column: "PlexServerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovie_PlexServerId",
                table: "PlexMovie",
                column: "PlexServerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShow_PlexServerId",
                table: "PlexTvShow",
                column: "PlexServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexMovie_PlexServers_PlexServerId",
                table: "PlexMovie",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShow_PlexLibraries_PlexLibraryId",
                table: "PlexTvShow",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShow_PlexServers_PlexServerId",
                table: "PlexTvShow",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowEpisodes_PlexServers_PlexServerId",
                table: "PlexTvShowEpisodes",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowGenre_PlexTvShow_PlexTvShowId",
                table: "PlexTvShowGenre",
                column: "PlexTvShowId",
                principalTable: "PlexTvShow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowRole_PlexTvShow_PlexTvShowId",
                table: "PlexTvShowRole",
                column: "PlexTvShowId",
                principalTable: "PlexTvShow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowSeason_PlexServers_PlexServerId",
                table: "PlexTvShowSeason",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowSeason_PlexTvShow_TvShowId",
                table: "PlexTvShowSeason",
                column: "TvShowId",
                principalTable: "PlexTvShow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexMovie_PlexServers_PlexServerId",
                table: "PlexMovie");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShow_PlexLibraries_PlexLibraryId",
                table: "PlexTvShow");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShow_PlexServers_PlexServerId",
                table: "PlexTvShow");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowEpisodes_PlexServers_PlexServerId",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowGenre_PlexTvShow_PlexTvShowId",
                table: "PlexTvShowGenre");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowRole_PlexTvShow_PlexTvShowId",
                table: "PlexTvShowRole");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowSeason_PlexServers_PlexServerId",
                table: "PlexTvShowSeason");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowSeason_PlexTvShow_TvShowId",
                table: "PlexTvShowSeason");

            migrationBuilder.DropIndex(
                name: "IX_PlexTvShowSeason_PlexServerId",
                table: "PlexTvShowSeason");

            migrationBuilder.DropIndex(
                name: "IX_PlexTvShowEpisodes_PlexServerId",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropIndex(
                name: "IX_PlexMovie_PlexServerId",
                table: "PlexMovie");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlexTvShow",
                table: "PlexTvShow");

            migrationBuilder.DropIndex(
                name: "IX_PlexTvShow_PlexServerId",
                table: "PlexTvShow");

            migrationBuilder.DropColumn(
                name: "ContentRating",
                table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(
                name: "HasArt",
                table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(
                name: "HasBanner",
                table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(
                name: "HasTheme",
                table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(
                name: "HasThumb",
                table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(
                name: "MetaDataKey",
                table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(
                name: "HasArt",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "HasBanner",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "HasTheme",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "HasThumb",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "HasArt",
                table: "PlexMovie");

            migrationBuilder.DropColumn(
                name: "HasBanner",
                table: "PlexMovie");

            migrationBuilder.DropColumn(
                name: "HasTheme",
                table: "PlexMovie");

            migrationBuilder.DropColumn(
                name: "HasThumb",
                table: "PlexMovie");

            migrationBuilder.DropColumn(
                name: "HasArt",
                table: "PlexTvShow");

            migrationBuilder.DropColumn(
                name: "HasBanner",
                table: "PlexTvShow");

            migrationBuilder.DropColumn(
                name: "HasTheme",
                table: "PlexTvShow");

            migrationBuilder.DropColumn(
                name: "HasThumb",
                table: "PlexTvShow");

            migrationBuilder.RenameTable(
                name: "PlexTvShow",
                newName: "PlexTvShows");

            migrationBuilder.RenameColumn(
                name: "Year",
                table: "PlexTvShowSeason",
                newName: "ViewedLeafCount");

            migrationBuilder.RenameColumn(
                name: "Studio",
                table: "PlexTvShowSeason",
                newName: "Guid");

            migrationBuilder.RenameColumn(
                name: "PlexServerId",
                table: "PlexTvShowSeason",
                newName: "LeafCount");

            migrationBuilder.RenameColumn(
                name: "PlexServerId",
                table: "PlexTvShowEpisodes",
                newName: "ViewedLeafCount");

            migrationBuilder.RenameColumn(
                name: "MetaDataKey",
                table: "PlexTvShowEpisodes",
                newName: "LeafCount");

            migrationBuilder.RenameColumn(
                name: "PlexServerId",
                table: "PlexMovie",
                newName: "ViewedLeafCount");

            migrationBuilder.RenameColumn(
                name: "MetaDataKey",
                table: "PlexMovie",
                newName: "LeafCount");

            migrationBuilder.RenameColumn(
                name: "PlexServerId",
                table: "PlexTvShows",
                newName: "ViewedLeafCount");

            migrationBuilder.RenameColumn(
                name: "MetaDataKey",
                table: "PlexTvShows",
                newName: "LeafCount");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShow_PlexLibraryId",
                table: "PlexTvShows",
                newName: "IX_PlexTvShows_PlexLibraryId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OriginallyAvailableAt",
                table: "PlexTvShowSeason",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OriginallyAvailableAt",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Art",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Banner",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Guid",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Thumb",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "PlexTvShowEpisodes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OriginallyAvailableAt",
                table: "PlexMovie",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Art",
                table: "PlexMovie",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Banner",
                table: "PlexMovie",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Guid",
                table: "PlexMovie",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "PlexMovie",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Thumb",
                table: "PlexMovie",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "PlexMovie",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OriginallyAvailableAt",
                table: "PlexTvShows",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Art",
                table: "PlexTvShows",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Banner",
                table: "PlexTvShows",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Guid",
                table: "PlexTvShows",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "PlexTvShows",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Thumb",
                table: "PlexTvShows",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "PlexTvShows",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexTvShows",
                table: "PlexTvShows",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowGenre_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowGenre",
                column: "PlexTvShowId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowRole_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowRole",
                column: "PlexTvShowId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShows_PlexLibraries_PlexLibraryId",
                table: "PlexTvShows",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowSeason_PlexTvShows_TvShowId",
                table: "PlexTvShowSeason",
                column: "TvShowId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
