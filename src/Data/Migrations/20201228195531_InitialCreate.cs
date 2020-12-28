using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FolderPaths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    DirectoryPath = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderPaths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Level = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    Hidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlexAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    Username = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsValidated = table.Column<bool>(type: "INTEGER", nullable: false),
                    ValidatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlexId = table.Column<long>(type: "INTEGER", nullable: false),
                    Uuid = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    HasPassword = table.Column<bool>(type: "INTEGER", nullable: false),
                    AuthenticationToken = table.Column<string>(type: "TEXT", nullable: true),
                    IsMain = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlexGenres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tag = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexGenres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlexRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tag = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlexServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Scheme = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    Host = table.Column<string>(type: "TEXT", nullable: true),
                    LocalAddresses = table.Column<string>(type: "TEXT", nullable: true),
                    MachineIdentifier = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<long>(type: "INTEGER", nullable: false),
                    ServerFixApplyDNSFix = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlexAccountServers",
                columns: table => new
                {
                    PlexAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Owned = table.Column<bool>(type: "INTEGER", nullable: false),
                    AuthToken = table.Column<string>(type: "TEXT", nullable: true),
                    AuthTokenCreationDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexAccountServers", x => new { x.PlexAccountId, x.PlexServerId });
                    table.ForeignKey(
                        name: "FK_PlexAccountServers_PlexAccounts_PlexAccountId",
                        column: x => x.PlexAccountId,
                        principalTable: "PlexAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexAccountServers_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexLibraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScannedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ContentChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaSize = table.Column<long>(type: "INTEGER", nullable: false),
                    LibraryLocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    LibraryLocationPath = table.Column<string>(type: "TEXT", nullable: true),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexLibraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexLibraries_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexServerStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSuccessful = table.Column<bool>(type: "INTEGER", nullable: false),
                    StatusMessage = table.Column<string>(type: "TEXT", nullable: true),
                    LastChecked = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexServerStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexServerStatuses_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DownloadTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileLocationUrl = table.Column<string>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    TitleTvShow = table.Column<string>(type: "TEXT", nullable: true),
                    TitleTvShowSeason = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseYear = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    DownloadStatus = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RatingKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<long>(type: "INTEGER", nullable: false),
                    DataReceived = table.Column<long>(type: "INTEGER", nullable: false),
                    DataTotal = table.Column<long>(type: "INTEGER", nullable: false),
                    ServerToken = table.Column<string>(type: "TEXT", nullable: true),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    DestinationFolderId = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadFolderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTasks", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "PlexAccountLibraries",
                columns: table => new
                {
                    PlexAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexAccountLibraries", x => new { x.PlexAccountId, x.PlexLibraryId, x.PlexServerId });
                    table.ForeignKey(
                        name: "FK_PlexAccountLibraries_PlexAccounts_PlexAccountId",
                        column: x => x.PlexAccountId,
                        principalTable: "PlexAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexAccountLibraries_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexAccountLibraries_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexMovie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RatingKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Guid = table.Column<string>(type: "TEXT", nullable: true),
                    Studio = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    ContentRating = table.Column<string>(type: "TEXT", nullable: true),
                    Summary = table.Column<string>(type: "TEXT", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<double>(type: "REAL", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Thumb = table.Column<string>(type: "TEXT", nullable: true),
                    Art = table.Column<string>(type: "TEXT", nullable: true),
                    Banner = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginallyAvailableAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LeafCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ViewedLeafCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ChildCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaSize = table.Column<long>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ViewCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Theme = table.Column<string>(type: "TEXT", nullable: true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexMovie_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexTvShows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RatingKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Guid = table.Column<string>(type: "TEXT", nullable: true),
                    Studio = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    ContentRating = table.Column<string>(type: "TEXT", nullable: true),
                    Summary = table.Column<string>(type: "TEXT", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<double>(type: "REAL", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Thumb = table.Column<string>(type: "TEXT", nullable: true),
                    Art = table.Column<string>(type: "TEXT", nullable: true),
                    Banner = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginallyAvailableAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LeafCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ViewedLeafCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ChildCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaSize = table.Column<long>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ViewCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Theme = table.Column<string>(type: "TEXT", nullable: true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShows_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DownloadWorkerTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    PartIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    StartByte = table.Column<long>(type: "INTEGER", nullable: false),
                    EndByte = table.Column<long>(type: "INTEGER", nullable: false),
                    BytesReceived = table.Column<long>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: true),
                    TempDirectory = table.Column<string>(type: "TEXT", nullable: true),
                    ElapsedTime = table.Column<long>(type: "INTEGER", nullable: false),
                    DownloadTaskId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadWorkerTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadWorkerTasks_DownloadTasks_DownloadTaskId",
                        column: x => x.DownloadTaskId,
                        principalTable: "DownloadTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DownloadTaskId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileTasks_DownloadTasks_DownloadTaskId",
                        column: x => x.DownloadTaskId,
                        principalTable: "DownloadTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexMovieData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaFormat = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<long>(type: "INTEGER", nullable: false),
                    VideoResolution = table.Column<string>(type: "TEXT", nullable: true),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    Bitrate = table.Column<int>(type: "INTEGER", nullable: false),
                    VideoCodec = table.Column<string>(type: "TEXT", nullable: true),
                    VideoFrameRate = table.Column<string>(type: "TEXT", nullable: true),
                    AspectRatio = table.Column<double>(type: "REAL", nullable: false),
                    VideoProfile = table.Column<string>(type: "TEXT", nullable: true),
                    AudioProfile = table.Column<string>(type: "TEXT", nullable: true),
                    AudioCodec = table.Column<string>(type: "TEXT", nullable: true),
                    AudioChannels = table.Column<int>(type: "INTEGER", nullable: false),
                    OptimizedForStreaming = table.Column<bool>(type: "INTEGER", nullable: false),
                    Protocol = table.Column<string>(type: "TEXT", nullable: true),
                    Selected = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexMovieData_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexMovieGenres",
                columns: table => new
                {
                    PlexGenreId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMoviesId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieGenres", x => new { x.PlexMoviesId, x.PlexGenreId });
                    table.ForeignKey(
                        name: "FK_PlexMovieGenres_PlexGenres_PlexGenreId",
                        column: x => x.PlexGenreId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexMovieGenres_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlexMovieRoles",
                columns: table => new
                {
                    PlexGenreId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMoviesId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlexRoleId = table.Column<int>(type: "INTEGER", nullable: true),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieRoles", x => new { x.PlexMoviesId, x.PlexGenreId });
                    table.ForeignKey(
                        name: "FK_PlexMovieRoles_PlexGenres_PlexGenreId",
                        column: x => x.PlexGenreId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexMovieRoles_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlexMovieRoles_PlexRoles_PlexRoleId",
                        column: x => x.PlexRoleId,
                        principalTable: "PlexRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlexTvShowGenre",
                columns: table => new
                {
                    PlexGenreId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowGenre", x => new { x.PlexTvShowId, x.PlexGenreId });
                    table.ForeignKey(
                        name: "FK_PlexTvShowGenre_PlexGenres_PlexGenreId",
                        column: x => x.PlexGenreId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexTvShowGenre_PlexTvShows_PlexTvShowId",
                        column: x => x.PlexTvShowId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexTvShowRole",
                columns: table => new
                {
                    PlexGenreId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowRole", x => new { x.PlexTvShowId, x.PlexGenreId });
                    table.ForeignKey(
                        name: "FK_PlexTvShowRole_PlexGenres_PlexGenreId",
                        column: x => x.PlexGenreId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexTvShowRole_PlexTvShows_PlexTvShowId",
                        column: x => x.PlexTvShowId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexTvShowSeason",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RatingKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Guid = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Summary = table.Column<string>(type: "TEXT", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    LeafCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ViewedLeafCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ChildCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaSize = table.Column<long>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OriginallyAvailableAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TvShowId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowSeason", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShowSeason_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexTvShowSeason_PlexTvShows_TvShowId",
                        column: x => x.TvShowId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexMovieDataParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlexMovieDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    ObfuscatedFilePath = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    File = table.Column<string>(type: "TEXT", nullable: true),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Container = table.Column<string>(type: "TEXT", nullable: true),
                    VideoProfile = table.Column<string>(type: "TEXT", nullable: true),
                    AudioProfile = table.Column<string>(type: "TEXT", nullable: true),
                    HasThumbnail = table.Column<string>(type: "TEXT", nullable: true),
                    Indexes = table.Column<string>(type: "TEXT", nullable: true),
                    HasChapterTextStream = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieDataParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexMovieDataParts_PlexMovieData_PlexMovieDataId",
                        column: x => x.PlexMovieDataId,
                        principalTable: "PlexMovieData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexTvShowEpisodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TvShowSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    RatingKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Guid = table.Column<string>(type: "TEXT", nullable: true),
                    Studio = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    ContentRating = table.Column<string>(type: "TEXT", nullable: true),
                    Summary = table.Column<string>(type: "TEXT", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<double>(type: "REAL", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Thumb = table.Column<string>(type: "TEXT", nullable: true),
                    Art = table.Column<string>(type: "TEXT", nullable: true),
                    Banner = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginallyAvailableAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LeafCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ViewedLeafCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ChildCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaSize = table.Column<long>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ViewCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Theme = table.Column<string>(type: "TEXT", nullable: true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowEpisodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodes_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodes_PlexTvShowSeason_TvShowSeasonId",
                        column: x => x.TvShowSeasonId,
                        principalTable: "PlexTvShowSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexTvShowEpisodeData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlexTvShowEpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaFormat = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<long>(type: "INTEGER", nullable: false),
                    VideoResolution = table.Column<string>(type: "TEXT", nullable: true),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    Bitrate = table.Column<int>(type: "INTEGER", nullable: false),
                    VideoCodec = table.Column<string>(type: "TEXT", nullable: true),
                    VideoFrameRate = table.Column<string>(type: "TEXT", nullable: true),
                    AspectRatio = table.Column<double>(type: "REAL", nullable: false),
                    VideoProfile = table.Column<string>(type: "TEXT", nullable: true),
                    AudioProfile = table.Column<string>(type: "TEXT", nullable: true),
                    AudioCodec = table.Column<string>(type: "TEXT", nullable: true),
                    AudioChannels = table.Column<int>(type: "INTEGER", nullable: false),
                    OptimizedForStreaming = table.Column<bool>(type: "INTEGER", nullable: false),
                    Protocol = table.Column<string>(type: "TEXT", nullable: true),
                    Selected = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowEpisodeData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeData_PlexTvShowEpisodes_PlexTvShowEpisodeId",
                        column: x => x.PlexTvShowEpisodeId,
                        principalTable: "PlexTvShowEpisodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexTvShowEpisodeDataParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlexTvShowEpisodeDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    ObfuscatedFilePath = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    File = table.Column<string>(type: "TEXT", nullable: true),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Container = table.Column<string>(type: "TEXT", nullable: true),
                    VideoProfile = table.Column<string>(type: "TEXT", nullable: true),
                    AudioProfile = table.Column<string>(type: "TEXT", nullable: true),
                    HasThumbnail = table.Column<string>(type: "TEXT", nullable: true),
                    Indexes = table.Column<string>(type: "TEXT", nullable: true),
                    HasChapterTextStream = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowEpisodeDataParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeDataParts_PlexTvShowEpisodeData_PlexTvShowEpisodeDataId",
                        column: x => x.PlexTvShowEpisodeDataId,
                        principalTable: "PlexTvShowEpisodeData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FolderPaths",
                columns: new[] { "Id", "DirectoryPath", "DisplayName", "Type" },
                values: new object[] { 1, "/downloads", "Download Path", "DownloadFolder" });

            migrationBuilder.InsertData(
                table: "FolderPaths",
                columns: new[] { "Id", "DirectoryPath", "DisplayName", "Type" },
                values: new object[] { 2, "/movies", "Movie Destination Path", "MovieFolder" });

            migrationBuilder.InsertData(
                table: "FolderPaths",
                columns: new[] { "Id", "DirectoryPath", "DisplayName", "Type" },
                values: new object[] { 3, "/tvshows", "Tv Show Destination Path", "TvShowFolder" });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTasks_DestinationFolderId",
                table: "DownloadTasks",
                column: "DestinationFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTasks_DownloadFolderId",
                table: "DownloadTasks",
                column: "DownloadFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTasks_PlexLibraryId",
                table: "DownloadTasks",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTasks_PlexServerId",
                table: "DownloadTasks",
                column: "PlexServerId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadWorkerTasks_DownloadTaskId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_FileTasks_DownloadTaskId",
                table: "FileTasks",
                column: "DownloadTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexAccountLibraries_PlexLibraryId",
                table: "PlexAccountLibraries",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexAccountLibraries_PlexServerId",
                table: "PlexAccountLibraries",
                column: "PlexServerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexAccountServers_PlexServerId",
                table: "PlexAccountServers",
                column: "PlexServerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexLibraries_PlexServerId",
                table: "PlexLibraries",
                column: "PlexServerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovie_PlexLibraryId",
                table: "PlexMovie",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieData_PlexMovieId",
                table: "PlexMovieData",
                column: "PlexMovieId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieDataParts_PlexMovieDataId",
                table: "PlexMovieDataParts",
                column: "PlexMovieDataId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieGenres_PlexGenreId",
                table: "PlexMovieGenres",
                column: "PlexGenreId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieGenres_PlexMovieId",
                table: "PlexMovieGenres",
                column: "PlexMovieId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieRoles_PlexGenreId",
                table: "PlexMovieRoles",
                column: "PlexGenreId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieRoles_PlexMovieId",
                table: "PlexMovieRoles",
                column: "PlexMovieId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieRoles_PlexRoleId",
                table: "PlexMovieRoles",
                column: "PlexRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexServerStatuses_PlexServerId",
                table: "PlexServerStatuses",
                column: "PlexServerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeData_PlexTvShowEpisodeId",
                table: "PlexTvShowEpisodeData",
                column: "PlexTvShowEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeDataParts_PlexTvShowEpisodeDataId",
                table: "PlexTvShowEpisodeDataParts",
                column: "PlexTvShowEpisodeDataId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodes_PlexLibraryId",
                table: "PlexTvShowEpisodes",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodes_TvShowSeasonId",
                table: "PlexTvShowEpisodes",
                column: "TvShowSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowGenre_PlexGenreId",
                table: "PlexTvShowGenre",
                column: "PlexGenreId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowRole_PlexGenreId",
                table: "PlexTvShowRole",
                column: "PlexGenreId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShows_PlexLibraryId",
                table: "PlexTvShows",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowSeason_PlexLibraryId",
                table: "PlexTvShowSeason",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowSeason_TvShowId",
                table: "PlexTvShowSeason",
                column: "TvShowId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadWorkerTasks");

            migrationBuilder.DropTable(
                name: "FileTasks");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PlexAccountLibraries");

            migrationBuilder.DropTable(
                name: "PlexAccountServers");

            migrationBuilder.DropTable(
                name: "PlexMovieDataParts");

            migrationBuilder.DropTable(
                name: "PlexMovieGenres");

            migrationBuilder.DropTable(
                name: "PlexMovieRoles");

            migrationBuilder.DropTable(
                name: "PlexServerStatuses");

            migrationBuilder.DropTable(
                name: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropTable(
                name: "PlexTvShowGenre");

            migrationBuilder.DropTable(
                name: "PlexTvShowRole");

            migrationBuilder.DropTable(
                name: "DownloadTasks");

            migrationBuilder.DropTable(
                name: "PlexAccounts");

            migrationBuilder.DropTable(
                name: "PlexMovieData");

            migrationBuilder.DropTable(
                name: "PlexRoles");

            migrationBuilder.DropTable(
                name: "PlexTvShowEpisodeData");

            migrationBuilder.DropTable(
                name: "PlexGenres");

            migrationBuilder.DropTable(
                name: "FolderPaths");

            migrationBuilder.DropTable(
                name: "PlexMovie");

            migrationBuilder.DropTable(
                name: "PlexTvShowEpisodes");

            migrationBuilder.DropTable(
                name: "PlexTvShowSeason");

            migrationBuilder.DropTable(
                name: "PlexTvShows");

            migrationBuilder.DropTable(
                name: "PlexLibraries");

            migrationBuilder.DropTable(
                name: "PlexServers");
        }
    }
}
