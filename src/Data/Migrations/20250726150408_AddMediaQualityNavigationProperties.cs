using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaQualityNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlexTvShowEpisodeMediaDataId",
                table: "PlexTvShowEpisodeMediaQuality",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AddColumn<int>(
                name: "PlexMovieMediaDataId",
                table: "PlexMovieMediaQuality",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeMediaQuality_PlexLibraryId",
                table: "PlexTvShowEpisodeMediaQuality",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeMediaQuality_PlexTvShowEpisodeMediaDataId",
                table: "PlexTvShowEpisodeMediaQuality",
                column: "PlexTvShowEpisodeMediaDataId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieMediaQuality_PlexLibraryId",
                table: "PlexMovieMediaQuality",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieMediaQuality_PlexMovieMediaDataId",
                table: "PlexMovieMediaQuality",
                column: "PlexMovieMediaDataId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexMovieMediaQuality_PlexLibraries_PlexLibraryId",
                table: "PlexMovieMediaQuality",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexMovieMediaQuality_PlexMovieData_PlexMovieMediaDataId",
                table: "PlexMovieMediaQuality",
                column: "PlexMovieMediaDataId",
                principalTable: "PlexMovieData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowEpisodeMediaQuality_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowEpisodeMediaQuality",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowEpisodeMediaQuality_PlexTvShowEpisodeData_PlexTvShowEpisodeMediaDataId",
                table: "PlexTvShowEpisodeMediaQuality",
                column: "PlexTvShowEpisodeMediaDataId",
                principalTable: "PlexTvShowEpisodeData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexMovieMediaQuality_PlexLibraries_PlexLibraryId",
                table: "PlexMovieMediaQuality");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexMovieMediaQuality_PlexMovieData_PlexMovieMediaDataId",
                table: "PlexMovieMediaQuality");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowEpisodeMediaQuality_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowEpisodeMediaQuality");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowEpisodeMediaQuality_PlexTvShowEpisodeData_PlexTvShowEpisodeMediaDataId",
                table: "PlexTvShowEpisodeMediaQuality");

            migrationBuilder.DropIndex(
                name: "IX_PlexTvShowEpisodeMediaQuality_PlexLibraryId",
                table: "PlexTvShowEpisodeMediaQuality");

            migrationBuilder.DropIndex(
                name: "IX_PlexTvShowEpisodeMediaQuality_PlexTvShowEpisodeMediaDataId",
                table: "PlexTvShowEpisodeMediaQuality");

            migrationBuilder.DropIndex(
                name: "IX_PlexMovieMediaQuality_PlexLibraryId",
                table: "PlexMovieMediaQuality");

            migrationBuilder.DropIndex(
                name: "IX_PlexMovieMediaQuality_PlexMovieMediaDataId",
                table: "PlexMovieMediaQuality");

            migrationBuilder.DropColumn(
                name: "PlexTvShowEpisodeMediaDataId",
                table: "PlexTvShowEpisodeMediaQuality");

            migrationBuilder.DropColumn(
                name: "PlexMovieMediaDataId",
                table: "PlexMovieMediaQuality");
        }
    }
}
