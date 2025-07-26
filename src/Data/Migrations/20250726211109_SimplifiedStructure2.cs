using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplifiedStructure2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowMediaQuality_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowMediaQuality"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowMediaQuality_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowMediaQuality"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowSeasonMediaQuality_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowSeasonMediaQuality"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowSeasonMediaQuality_PlexTvShowSeason_PlexTvShowSeasonId",
                table: "PlexTvShowSeasonMediaQuality"
            );

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlexTvShowSeasonMediaQuality",
                table: "PlexTvShowSeasonMediaQuality"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_PlexTvShowMediaQuality", table: "PlexTvShowMediaQuality");

            migrationBuilder.RenameTable(
                name: "PlexTvShowSeasonMediaQuality",
                newName: "PlexTvShowSeasonMediaQualities"
            );

            migrationBuilder.RenameTable(name: "PlexTvShowMediaQuality", newName: "PlexTvShowMediaQualities");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowSeasonMediaQuality_PlexTvShowSeasonId",
                table: "PlexTvShowSeasonMediaQualities",
                newName: "IX_PlexTvShowSeasonMediaQualities_PlexTvShowSeasonId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowSeasonMediaQuality_PlexLibraryId",
                table: "PlexTvShowSeasonMediaQualities",
                newName: "IX_PlexTvShowSeasonMediaQualities_PlexLibraryId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowMediaQuality_PlexTvShowId",
                table: "PlexTvShowMediaQualities",
                newName: "IX_PlexTvShowMediaQualities_PlexTvShowId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowMediaQuality_PlexLibraryId",
                table: "PlexTvShowMediaQualities",
                newName: "IX_PlexTvShowMediaQualities_PlexLibraryId"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexTvShowSeasonMediaQualities",
                table: "PlexTvShowSeasonMediaQualities",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexTvShowMediaQualities",
                table: "PlexTvShowMediaQualities",
                column: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowMediaQualities_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowMediaQualities",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowMediaQualities_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowMediaQualities",
                column: "PlexTvShowId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowSeasonMediaQualities_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowSeasonMediaQualities",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowSeasonMediaQualities_PlexTvShowSeason_PlexTvShowSeasonId",
                table: "PlexTvShowSeasonMediaQualities",
                column: "PlexTvShowSeasonId",
                principalTable: "PlexTvShowSeason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowMediaQualities_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowMediaQualities"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowMediaQualities_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowMediaQualities"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowSeasonMediaQualities_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowSeasonMediaQualities"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowSeasonMediaQualities_PlexTvShowSeason_PlexTvShowSeasonId",
                table: "PlexTvShowSeasonMediaQualities"
            );

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlexTvShowSeasonMediaQualities",
                table: "PlexTvShowSeasonMediaQualities"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_PlexTvShowMediaQualities", table: "PlexTvShowMediaQualities");

            migrationBuilder.RenameTable(
                name: "PlexTvShowSeasonMediaQualities",
                newName: "PlexTvShowSeasonMediaQuality"
            );

            migrationBuilder.RenameTable(name: "PlexTvShowMediaQualities", newName: "PlexTvShowMediaQuality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowSeasonMediaQualities_PlexTvShowSeasonId",
                table: "PlexTvShowSeasonMediaQuality",
                newName: "IX_PlexTvShowSeasonMediaQuality_PlexTvShowSeasonId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowSeasonMediaQualities_PlexLibraryId",
                table: "PlexTvShowSeasonMediaQuality",
                newName: "IX_PlexTvShowSeasonMediaQuality_PlexLibraryId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowMediaQualities_PlexTvShowId",
                table: "PlexTvShowMediaQuality",
                newName: "IX_PlexTvShowMediaQuality_PlexTvShowId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowMediaQualities_PlexLibraryId",
                table: "PlexTvShowMediaQuality",
                newName: "IX_PlexTvShowMediaQuality_PlexLibraryId"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexTvShowSeasonMediaQuality",
                table: "PlexTvShowSeasonMediaQuality",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexTvShowMediaQuality",
                table: "PlexTvShowMediaQuality",
                column: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowMediaQuality_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowMediaQuality",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowMediaQuality_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowMediaQuality",
                column: "PlexTvShowId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowSeasonMediaQuality_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowSeasonMediaQuality",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowSeasonMediaQuality_PlexTvShowSeason_PlexTvShowSeasonId",
                table: "PlexTvShowSeasonMediaQuality",
                column: "PlexTvShowSeasonId",
                principalTable: "PlexTvShowSeason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
