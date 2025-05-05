using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizedPlexMediaData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "MediaData", table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(name: "MediaData", table: "PlexTvShows");

            migrationBuilder.DropColumn(name: "MediaData", table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(name: "MediaData", table: "PlexMovie");

            migrationBuilder.CreateTable(
                name: "PlexMovieData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexId = table.Column<long>(type: "INTEGER", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    Bitrate = table.Column<int>(type: "INTEGER", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    AspectRatio = table.Column<float>(type: "REAL", nullable: false),
                    AudioChannels = table.Column<int>(type: "INTEGER", nullable: false),
                    AudioCodec = table.Column<string>(type: "TEXT", nullable: false),
                    VideoCodec = table.Column<string>(type: "TEXT", nullable: false),
                    VideoResolution = table.Column<string>(type: "TEXT", nullable: false),
                    Container = table.Column<string>(type: "TEXT", nullable: false),
                    VideoFrameRate = table.Column<string>(type: "TEXT", nullable: false),
                    VideoProfile = table.Column<string>(type: "TEXT", nullable: false),
                    AudioProfile = table.Column<string>(type: "TEXT", nullable: false),
                    HasVoiceActivity = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexMovieData_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieData_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieData_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowEpisodeData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexId = table.Column<long>(type: "INTEGER", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    Bitrate = table.Column<int>(type: "INTEGER", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    AspectRatio = table.Column<float>(type: "REAL", nullable: false),
                    AudioChannels = table.Column<int>(type: "INTEGER", nullable: false),
                    AudioCodec = table.Column<string>(type: "TEXT", nullable: false),
                    VideoCodec = table.Column<string>(type: "TEXT", nullable: false),
                    VideoResolution = table.Column<string>(type: "TEXT", nullable: false),
                    Container = table.Column<string>(type: "TEXT", nullable: false),
                    VideoFrameRate = table.Column<string>(type: "TEXT", nullable: false),
                    VideoProfile = table.Column<string>(type: "TEXT", nullable: false),
                    AudioProfile = table.Column<string>(type: "TEXT", nullable: false),
                    HasVoiceActivity = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowEpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowEpisodeData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeData_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeData_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeData_PlexTvShowEpisodes_PlexTvShowEpisodeId",
                        column: x => x.PlexTvShowEpisodeId,
                        principalTable: "PlexTvShowEpisodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexMovieDataParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexId = table.Column<long>(type: "INTEGER", nullable: false),
                    Accessible = table.Column<bool>(type: "INTEGER", nullable: true),
                    Exists = table.Column<bool>(type: "INTEGER", nullable: true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Indexes = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    File = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Container = table.Column<string>(type: "TEXT", nullable: false),
                    VideoProfile = table.Column<string>(type: "TEXT", nullable: false),
                    AudioProfile = table.Column<string>(type: "TEXT", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieMediaDataId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieDataParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexMovieDataParts_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieDataParts_PlexMovieData_PlexMovieMediaDataId",
                        column: x => x.PlexMovieMediaDataId,
                        principalTable: "PlexMovieData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieDataParts_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieDataParts_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowEpisodeDataParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexId = table.Column<long>(type: "INTEGER", nullable: false),
                    Accessible = table.Column<bool>(type: "INTEGER", nullable: true),
                    Exists = table.Column<bool>(type: "INTEGER", nullable: true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Indexes = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    File = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Container = table.Column<string>(type: "TEXT", nullable: false),
                    VideoProfile = table.Column<string>(type: "TEXT", nullable: false),
                    AudioProfile = table.Column<string>(type: "TEXT", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowEpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowEpisodeMediaDataId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowEpisodeDataParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeDataParts_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeDataParts_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeDataParts_PlexTvShowEpisodeData_PlexTvShowEpisodeMediaDataId",
                        column: x => x.PlexTvShowEpisodeMediaDataId,
                        principalTable: "PlexTvShowEpisodeData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeDataParts_PlexTvShowEpisodes_PlexTvShowEpisodeId",
                        column: x => x.PlexTvShowEpisodeId,
                        principalTable: "PlexTvShowEpisodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexMovieDataStreams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexId = table.Column<long>(type: "INTEGER", nullable: false),
                    StreamType = table.Column<int>(type: "INTEGER", nullable: false),
                    Default = table.Column<bool>(type: "INTEGER", nullable: true),
                    Codec = table.Column<string>(type: "TEXT", nullable: false),
                    Index = table.Column<int>(type: "INTEGER", nullable: true),
                    Bitrate = table.Column<int>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageTag = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", nullable: false),
                    DOVIBLCompatID = table.Column<int>(type: "INTEGER", nullable: true),
                    DOVIBLPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVIELPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVILevel = table.Column<int>(type: "INTEGER", nullable: true),
                    DOVIPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVIProfile = table.Column<int>(type: "INTEGER", nullable: true),
                    DOVIRPUPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVIVersion = table.Column<string>(type: "TEXT", nullable: true),
                    BitDepth = table.Column<int>(type: "INTEGER", nullable: true),
                    ChromaLocation = table.Column<string>(type: "TEXT", nullable: true),
                    ChromaSubsampling = table.Column<string>(type: "TEXT", nullable: true),
                    CodedHeight = table.Column<int>(type: "INTEGER", nullable: true),
                    CodedWidth = table.Column<int>(type: "INTEGER", nullable: true),
                    ColorPrimaries = table.Column<string>(type: "TEXT", nullable: true),
                    ColorRange = table.Column<string>(type: "TEXT", nullable: true),
                    ColorSpace = table.Column<string>(type: "TEXT", nullable: true),
                    ColorTrc = table.Column<string>(type: "TEXT", nullable: true),
                    FrameRate = table.Column<float>(type: "REAL", nullable: true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    Level = table.Column<int>(type: "INTEGER", nullable: true),
                    Original = table.Column<bool>(type: "INTEGER", nullable: true),
                    HasScalingMatrix = table.Column<bool>(type: "INTEGER", nullable: true),
                    Profile = table.Column<string>(type: "TEXT", nullable: true),
                    ScanType = table.Column<string>(type: "TEXT", nullable: true),
                    RefFrames = table.Column<int>(type: "INTEGER", nullable: true),
                    Width = table.Column<int>(type: "INTEGER", nullable: true),
                    DisplayTitle = table.Column<string>(type: "TEXT", nullable: false),
                    ExtendedDisplayTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Selected = table.Column<bool>(type: "INTEGER", nullable: true),
                    Forced = table.Column<bool>(type: "INTEGER", nullable: true),
                    Channels = table.Column<int>(type: "INTEGER", nullable: true),
                    AudioChannelLayout = table.Column<string>(type: "TEXT", nullable: true),
                    SamplingRate = table.Column<int>(type: "INTEGER", nullable: true),
                    CanAutoSync = table.Column<bool>(type: "INTEGER", nullable: true),
                    HearingImpaired = table.Column<bool>(type: "INTEGER", nullable: true),
                    Dub = table.Column<bool>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieMediaDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieMediaDataPartId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieDataStreams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexMovieDataStreams_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieDataStreams_PlexMovieDataParts_PlexMovieMediaDataPartId",
                        column: x => x.PlexMovieMediaDataPartId,
                        principalTable: "PlexMovieDataParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieDataStreams_PlexMovieData_PlexMovieMediaDataId",
                        column: x => x.PlexMovieMediaDataId,
                        principalTable: "PlexMovieData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieDataStreams_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieDataStreams_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowEpisodeDataStreams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexId = table.Column<long>(type: "INTEGER", nullable: false),
                    StreamType = table.Column<int>(type: "INTEGER", nullable: false),
                    Default = table.Column<bool>(type: "INTEGER", nullable: true),
                    Codec = table.Column<string>(type: "TEXT", nullable: false),
                    Index = table.Column<int>(type: "INTEGER", nullable: true),
                    Bitrate = table.Column<int>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageTag = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", nullable: false),
                    DOVIBLCompatID = table.Column<int>(type: "INTEGER", nullable: true),
                    DOVIBLPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVIELPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVILevel = table.Column<int>(type: "INTEGER", nullable: true),
                    DOVIPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVIProfile = table.Column<int>(type: "INTEGER", nullable: true),
                    DOVIRPUPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVIVersion = table.Column<string>(type: "TEXT", nullable: true),
                    BitDepth = table.Column<int>(type: "INTEGER", nullable: true),
                    ChromaLocation = table.Column<string>(type: "TEXT", nullable: true),
                    ChromaSubsampling = table.Column<string>(type: "TEXT", nullable: true),
                    CodedHeight = table.Column<int>(type: "INTEGER", nullable: true),
                    CodedWidth = table.Column<int>(type: "INTEGER", nullable: true),
                    ColorPrimaries = table.Column<string>(type: "TEXT", nullable: true),
                    ColorRange = table.Column<string>(type: "TEXT", nullable: true),
                    ColorSpace = table.Column<string>(type: "TEXT", nullable: true),
                    ColorTrc = table.Column<string>(type: "TEXT", nullable: true),
                    FrameRate = table.Column<float>(type: "REAL", nullable: true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    Level = table.Column<int>(type: "INTEGER", nullable: true),
                    Original = table.Column<bool>(type: "INTEGER", nullable: true),
                    HasScalingMatrix = table.Column<bool>(type: "INTEGER", nullable: true),
                    Profile = table.Column<string>(type: "TEXT", nullable: true),
                    ScanType = table.Column<string>(type: "TEXT", nullable: true),
                    RefFrames = table.Column<int>(type: "INTEGER", nullable: true),
                    Width = table.Column<int>(type: "INTEGER", nullable: true),
                    DisplayTitle = table.Column<string>(type: "TEXT", nullable: false),
                    ExtendedDisplayTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Selected = table.Column<bool>(type: "INTEGER", nullable: true),
                    Forced = table.Column<bool>(type: "INTEGER", nullable: true),
                    Channels = table.Column<int>(type: "INTEGER", nullable: true),
                    AudioChannelLayout = table.Column<string>(type: "TEXT", nullable: true),
                    SamplingRate = table.Column<int>(type: "INTEGER", nullable: true),
                    CanAutoSync = table.Column<bool>(type: "INTEGER", nullable: true),
                    HearingImpaired = table.Column<bool>(type: "INTEGER", nullable: true),
                    Dub = table.Column<bool>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowEpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowEpisodeMediaDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowEpisodeMediaDataPartId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowEpisodeDataStreams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeDataStreams_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeDataStreams_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeDataStreams_PlexTvShowEpisodeDataParts_PlexTvShowEpisodeMediaDataPartId",
                        column: x => x.PlexTvShowEpisodeMediaDataPartId,
                        principalTable: "PlexTvShowEpisodeDataParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeDataStreams_PlexTvShowEpisodeData_PlexTvShowEpisodeMediaDataId",
                        column: x => x.PlexTvShowEpisodeMediaDataId,
                        principalTable: "PlexTvShowEpisodeData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeDataStreams_PlexTvShowEpisodes_PlexTvShowEpisodeId",
                        column: x => x.PlexTvShowEpisodeId,
                        principalTable: "PlexTvShowEpisodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieData_PlexLibraryId",
                table: "PlexMovieData",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieData_PlexMovieId",
                table: "PlexMovieData",
                column: "PlexMovieId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieData_PlexServerId",
                table: "PlexMovieData",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieDataParts_PlexLibraryId",
                table: "PlexMovieDataParts",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieDataParts_PlexMovieId",
                table: "PlexMovieDataParts",
                column: "PlexMovieId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieDataParts_PlexMovieMediaDataId",
                table: "PlexMovieDataParts",
                column: "PlexMovieMediaDataId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieDataParts_PlexServerId",
                table: "PlexMovieDataParts",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieDataStreams_PlexLibraryId",
                table: "PlexMovieDataStreams",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieDataStreams_PlexMovieId",
                table: "PlexMovieDataStreams",
                column: "PlexMovieId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieDataStreams_PlexMovieMediaDataId",
                table: "PlexMovieDataStreams",
                column: "PlexMovieMediaDataId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieDataStreams_PlexMovieMediaDataPartId",
                table: "PlexMovieDataStreams",
                column: "PlexMovieMediaDataPartId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieDataStreams_PlexServerId",
                table: "PlexMovieDataStreams",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeData_PlexLibraryId",
                table: "PlexTvShowEpisodeData",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeData_PlexServerId",
                table: "PlexTvShowEpisodeData",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeData_PlexTvShowEpisodeId",
                table: "PlexTvShowEpisodeData",
                column: "PlexTvShowEpisodeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeDataParts_PlexLibraryId",
                table: "PlexTvShowEpisodeDataParts",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeDataParts_PlexServerId",
                table: "PlexTvShowEpisodeDataParts",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeDataParts_PlexTvShowEpisodeId",
                table: "PlexTvShowEpisodeDataParts",
                column: "PlexTvShowEpisodeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeDataParts_PlexTvShowEpisodeMediaDataId",
                table: "PlexTvShowEpisodeDataParts",
                column: "PlexTvShowEpisodeMediaDataId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeDataStreams_PlexLibraryId",
                table: "PlexTvShowEpisodeDataStreams",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeDataStreams_PlexServerId",
                table: "PlexTvShowEpisodeDataStreams",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeDataStreams_PlexTvShowEpisodeId",
                table: "PlexTvShowEpisodeDataStreams",
                column: "PlexTvShowEpisodeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeDataStreams_PlexTvShowEpisodeMediaDataId",
                table: "PlexTvShowEpisodeDataStreams",
                column: "PlexTvShowEpisodeMediaDataId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeDataStreams_PlexTvShowEpisodeMediaDataPartId",
                table: "PlexTvShowEpisodeDataStreams",
                column: "PlexTvShowEpisodeMediaDataPartId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PlexMovieDataStreams");

            migrationBuilder.DropTable(name: "PlexTvShowEpisodeDataStreams");

            migrationBuilder.DropTable(name: "PlexMovieDataParts");

            migrationBuilder.DropTable(name: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropTable(name: "PlexMovieData");

            migrationBuilder.DropTable(name: "PlexTvShowEpisodeData");

            migrationBuilder.AddColumn<string>(
                name: "MediaData",
                table: "PlexTvShowSeason",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "MediaData",
                table: "PlexTvShows",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "MediaData",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "MediaData",
                table: "PlexMovie",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );
        }
    }
}
