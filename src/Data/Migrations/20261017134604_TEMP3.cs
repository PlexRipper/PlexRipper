using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class TEMP3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StreamLanguages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LanguageCode = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageTag = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamLanguages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlexMovieDataStreams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StreamType = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexApiStreamId = table.Column<int>(type: "INTEGER", nullable: false),
                    LanguageId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsExternal = table.Column<bool>(type: "INTEGER", nullable: true),
                    SidecarFormat = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieDataStreams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexMovieDataStreams_PlexMovieData_MediaDataId",
                        column: x => x.MediaDataId,
                        principalTable: "PlexMovieData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexMovieDataStreams_StreamLanguages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "StreamLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlexMusicTrackDataStreams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StreamType = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexApiStreamId = table.Column<int>(type: "INTEGER", nullable: false),
                    LanguageId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsExternal = table.Column<bool>(type: "INTEGER", nullable: true),
                    SidecarFormat = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMusicTrackDataStreams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexMusicTrackDataStreams_PlexMusicTrackData_MediaDataId",
                        column: x => x.MediaDataId,
                        principalTable: "PlexMusicTrackData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexMusicTrackDataStreams_StreamLanguages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "StreamLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlexOtherVideoDataStreams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StreamType = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexApiStreamId = table.Column<int>(type: "INTEGER", nullable: false),
                    LanguageId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsExternal = table.Column<bool>(type: "INTEGER", nullable: true),
                    SidecarFormat = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexOtherVideoDataStreams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexOtherVideoDataStreams_PlexOtherVideoData_MediaDataId",
                        column: x => x.MediaDataId,
                        principalTable: "PlexOtherVideoData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexOtherVideoDataStreams_StreamLanguages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "StreamLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlexTvShowEpisodeDataStreams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StreamType = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowEpisodeDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexApiStreamId = table.Column<int>(type: "INTEGER", nullable: false),
                    LanguageId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsExternal = table.Column<bool>(type: "INTEGER", nullable: true),
                    SidecarFormat = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowEpisodeDataStreams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeDataStreams_PlexTvShowEpisodeData_PlexTvShowEpisodeDataId",
                        column: x => x.PlexTvShowEpisodeDataId,
                        principalTable: "PlexTvShowEpisodeData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeDataStreams_StreamLanguages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "StreamLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieDataStreams_LanguageId_StreamType_MediaDataId",
                table: "PlexMovieDataStreams",
                columns: new[] { "LanguageId", "StreamType", "MediaDataId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieDataStreams_MediaDataId_PlexApiStreamId",
                table: "PlexMovieDataStreams",
                columns: new[] { "MediaDataId", "PlexApiStreamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexMusicTrackDataStreams_LanguageId_StreamType_MediaDataId",
                table: "PlexMusicTrackDataStreams",
                columns: new[] { "LanguageId", "StreamType", "MediaDataId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexMusicTrackDataStreams_MediaDataId_PlexApiStreamId",
                table: "PlexMusicTrackDataStreams",
                columns: new[] { "MediaDataId", "PlexApiStreamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexOtherVideoDataStreams_LanguageId_StreamType_MediaDataId",
                table: "PlexOtherVideoDataStreams",
                columns: new[] { "LanguageId", "StreamType", "MediaDataId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexOtherVideoDataStreams_MediaDataId_PlexApiStreamId",
                table: "PlexOtherVideoDataStreams",
                columns: new[] { "MediaDataId", "PlexApiStreamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeDataStreams_LanguageId_StreamType_PlexTvShowEpisodeDataId",
                table: "PlexTvShowEpisodeDataStreams",
                columns: new[] { "LanguageId", "StreamType", "PlexTvShowEpisodeDataId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeDataStreams_PlexTvShowEpisodeDataId_PlexApiStreamId",
                table: "PlexTvShowEpisodeDataStreams",
                columns: new[] { "PlexTvShowEpisodeDataId", "PlexApiStreamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StreamLanguages_LanguageCode_LanguageTag",
                table: "StreamLanguages",
                columns: new[] { "LanguageCode", "LanguageTag" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlexMovieDataStreams");

            migrationBuilder.DropTable(
                name: "PlexMusicTrackDataStreams");

            migrationBuilder.DropTable(
                name: "PlexOtherVideoDataStreams");

            migrationBuilder.DropTable(
                name: "PlexTvShowEpisodeDataStreams");

            migrationBuilder.DropTable(
                name: "StreamLanguages");
        }
    }
}
