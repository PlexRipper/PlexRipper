using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FolderPaths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    FolderType = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: false),
                    MediaType = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: false),
                    DirectoryPath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderPaths", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Level = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Hidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsValidated = table.Column<bool>(type: "INTEGER", nullable: false),
                    ValidatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlexId = table.Column<long>(type: "INTEGER", nullable: false),
                    Uuid = table.Column<string>(type: "TEXT", nullable: false),
                    ClientId = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    HasPassword = table.Column<bool>(type: "INTEGER", nullable: false),
                    AuthenticationToken = table.Column<string>(type: "TEXT", nullable: false),
                    IsMain = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexAccounts", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexGenres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Tag = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexGenres", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Tag = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexRoles", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlexServerOwnerUsername = table.Column<string>(type: "TEXT", nullable: false),
                    Device = table.Column<string>(type: "TEXT", nullable: false),
                    Platform = table.Column<string>(type: "TEXT", nullable: false),
                    PlatformVersion = table.Column<string>(type: "TEXT", nullable: false),
                    Product = table.Column<string>(type: "TEXT", nullable: false),
                    ProductVersion = table.Column<string>(type: "TEXT", nullable: false),
                    Provides = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MachineIdentifier = table.Column<string>(type: "TEXT", nullable: false),
                    PublicAddress = table.Column<string>(type: "TEXT", nullable: false),
                    PreferredConnectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Owned = table.Column<bool>(type: "INTEGER", nullable: false),
                    Home = table.Column<bool>(type: "INTEGER", nullable: false),
                    Synced = table.Column<bool>(type: "INTEGER", nullable: false),
                    Relay = table.Column<bool>(type: "INTEGER", nullable: false),
                    Presence = table.Column<bool>(type: "INTEGER", nullable: false),
                    HttpsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    PublicAddressMatches = table.Column<bool>(type: "INTEGER", nullable: false),
                    DnsRebindingProtection = table.Column<bool>(type: "INTEGER", nullable: false),
                    NatLoopbackSupported = table.Column<bool>(type: "INTEGER", nullable: false),
                    ServerFixApplyDNSFix = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexServers", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "QRTZ_CALENDARS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    CALENDAR_NAME = table.Column<string>(type: "text", nullable: false),
                    CALENDAR = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_CALENDARS", x => new { x.SCHED_NAME, x.CALENDAR_NAME });
                }
            );

            migrationBuilder.CreateTable(
                name: "QRTZ_FIRED_TRIGGERS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    ENTRY_ID = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "text", nullable: false),
                    INSTANCE_NAME = table.Column<string>(type: "text", nullable: false),
                    FIRED_TIME = table.Column<long>(type: "bigint", nullable: false),
                    SCHED_TIME = table.Column<long>(type: "bigint", nullable: false),
                    PRIORITY = table.Column<int>(type: "integer", nullable: false),
                    STATE = table.Column<string>(type: "text", nullable: false),
                    JOB_NAME = table.Column<string>(type: "text", nullable: true),
                    JOB_GROUP = table.Column<string>(type: "text", nullable: true),
                    IS_NONCONCURRENT = table.Column<bool>(type: "bool", nullable: false),
                    REQUESTS_RECOVERY = table.Column<bool>(type: "bool", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_FIRED_TRIGGERS", x => new { x.SCHED_NAME, x.ENTRY_ID });
                }
            );

            migrationBuilder.CreateTable(
                name: "QRTZ_JOB_DETAILS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    JOB_NAME = table.Column<string>(type: "text", nullable: false),
                    JOB_GROUP = table.Column<string>(type: "text", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "text", nullable: true),
                    JOB_CLASS_NAME = table.Column<string>(type: "text", nullable: false),
                    IS_DURABLE = table.Column<bool>(type: "bool", nullable: false),
                    IS_NONCONCURRENT = table.Column<bool>(type: "bool", nullable: false),
                    IS_UPDATE_DATA = table.Column<bool>(type: "bool", nullable: false),
                    REQUESTS_RECOVERY = table.Column<bool>(type: "bool", nullable: false),
                    JOB_DATA = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_QRTZ_JOB_DETAILS",
                        x => new
                        {
                            x.SCHED_NAME,
                            x.JOB_NAME,
                            x.JOB_GROUP
                        }
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "QRTZ_LOCKS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    LOCK_NAME = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_LOCKS", x => new { x.SCHED_NAME, x.LOCK_NAME });
                }
            );

            migrationBuilder.CreateTable(
                name: "QRTZ_PAUSED_TRIGGER_GRPS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_PAUSED_TRIGGER_GRPS", x => new { x.SCHED_NAME, x.TRIGGER_GROUP });
                }
            );

            migrationBuilder.CreateTable(
                name: "QRTZ_SCHEDULER_STATE",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    INSTANCE_NAME = table.Column<string>(type: "text", nullable: false),
                    LAST_CHECKIN_TIME = table.Column<long>(type: "bigint", nullable: false),
                    CHECKIN_INTERVAL = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_SCHEDULER_STATE", x => new { x.SCHED_NAME, x.INSTANCE_NAME });
                }
            );

            migrationBuilder.CreateTable(
                name: "DownloadWorkerTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    PartIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    StartByte = table.Column<long>(type: "INTEGER", nullable: false),
                    EndByte = table.Column<long>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    BytesReceived = table.Column<long>(type: "INTEGER", nullable: false),
                    DownloadDirectory = table.Column<string>(type: "TEXT", nullable: false),
                    ElapsedTime = table.Column<long>(type: "INTEGER", nullable: false),
                    FileLocationUrl = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadTaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadWorkerTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadWorkerTasks_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexAccountServers",
                columns: table => new
                {
                    PlexAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthToken = table.Column<string>(type: "TEXT", nullable: false),
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
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexAccountServers_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexLibraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    LibraryLocationPath = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScannedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryLocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    MetaData = table.Column<string>(type: "TEXT", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultDestinationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexLibraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexLibraries_FolderPaths_DefaultDestinationId",
                        column: x => x.DefaultDestinationId,
                        principalTable: "FolderPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_PlexLibraries_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexServerConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Protocol = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    Local = table.Column<bool>(type: "INTEGER", nullable: false),
                    Relay = table.Column<bool>(type: "INTEGER", nullable: false),
                    IPv4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    IPv6 = table.Column<bool>(type: "INTEGER", nullable: false),
                    PortFix = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexServerConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexServerConnections_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "QRTZ_TRIGGERS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "text", nullable: false),
                    JOB_NAME = table.Column<string>(type: "text", nullable: false),
                    JOB_GROUP = table.Column<string>(type: "text", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "text", nullable: true),
                    NEXT_FIRE_TIME = table.Column<long>(type: "bigint", nullable: true),
                    PREV_FIRE_TIME = table.Column<long>(type: "bigint", nullable: true),
                    PRIORITY = table.Column<int>(type: "integer", nullable: true),
                    TRIGGER_STATE = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_TYPE = table.Column<string>(type: "text", nullable: false),
                    START_TIME = table.Column<long>(type: "bigint", nullable: false),
                    END_TIME = table.Column<long>(type: "bigint", nullable: true),
                    CALENDAR_NAME = table.Column<string>(type: "text", nullable: true),
                    MISFIRE_INSTR = table.Column<short>(type: "smallint", nullable: true),
                    JOB_DATA = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_QRTZ_TRIGGERS",
                        x => new
                        {
                            x.SCHED_NAME,
                            x.TRIGGER_NAME,
                            x.TRIGGER_GROUP
                        }
                    );
                    table.ForeignKey(
                        name: "FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS_SCHED_NAME_JOB_NAME_JOB_GROUP",
                        columns: x => new
                        {
                            x.SCHED_NAME,
                            x.JOB_NAME,
                            x.JOB_GROUP
                        },
                        principalTable: "QRTZ_JOB_DETAILS",
                        principalColumns: new[] { "SCHED_NAME", "JOB_NAME", "JOB_GROUP" },
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DownloadWorkerTasksLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    LogLevel = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    DownloadWorkerTaskId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadWorkerTasksLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadWorkerTasksLogs_DownloadWorkerTasks_DownloadWorkerTaskId",
                        column: x => x.DownloadWorkerTaskId,
                        principalTable: "DownloadWorkerTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DownloadTaskMovie",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
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
                name: "FileTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DestinationDirectory = table.Column<string>(type: "TEXT", nullable: false),
                    FilePathsCompressed = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    DownloadTaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DownloadTaskType = table.Column<string>(
                        type: "TEXT",
                        unicode: false,
                        maxLength: 50,
                        nullable: false
                    ),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileTasks_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_FileTasks_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

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
                    table.PrimaryKey(
                        "PK_PlexAccountLibraries",
                        x => new
                        {
                            x.PlexAccountId,
                            x.PlexLibraryId,
                            x.PlexServerId
                        }
                    );
                    table.ForeignKey(
                        name: "FK_PlexAccountLibraries_PlexAccounts_PlexAccountId",
                        column: x => x.PlexAccountId,
                        principalTable: "PlexAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexAccountLibraries_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexAccountLibraries_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexMovie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    SortTitle = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaSize = table.Column<long>(type: "INTEGER", nullable: false),
                    MetaDataKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Studio = table.Column<string>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    ContentRating = table.Column<string>(type: "TEXT", nullable: false),
                    Rating = table.Column<double>(type: "REAL", nullable: false),
                    ChildCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OriginallyAvailableAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    HasThumb = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasArt = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasBanner = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasTheme = table.Column<bool>(type: "INTEGER", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
                    MediaData = table.Column<string>(type: "TEXT", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexMovie_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovie_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    SortTitle = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaSize = table.Column<long>(type: "INTEGER", nullable: false),
                    MetaDataKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Studio = table.Column<string>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    ContentRating = table.Column<string>(type: "TEXT", nullable: false),
                    Rating = table.Column<double>(type: "REAL", nullable: false),
                    ChildCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OriginallyAvailableAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    HasThumb = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasArt = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasBanner = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasTheme = table.Column<bool>(type: "INTEGER", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
                    MediaData = table.Column<string>(type: "TEXT", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShows_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShows_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexServerStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    IsSuccessful = table.Column<bool>(type: "INTEGER", nullable: false),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusMessage = table.Column<string>(type: "TEXT", nullable: false),
                    LastChecked = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerConnectionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexServerStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexServerStatuses_PlexServerConnections_PlexServerConnectionId",
                        column: x => x.PlexServerConnectionId,
                        principalTable: "PlexServerConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexServerStatuses_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "QRTZ_BLOB_TRIGGERS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "text", nullable: false),
                    BLOB_DATA = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_QRTZ_BLOB_TRIGGERS",
                        x => new
                        {
                            x.SCHED_NAME,
                            x.TRIGGER_NAME,
                            x.TRIGGER_GROUP
                        }
                    );
                    table.ForeignKey(
                        name: "FK_QRTZ_BLOB_TRIGGERS_QRTZ_TRIGGERS_SCHED_NAME_TRIGGER_NAME_TRIGGER_GROUP",
                        columns: x => new
                        {
                            x.SCHED_NAME,
                            x.TRIGGER_NAME,
                            x.TRIGGER_GROUP
                        },
                        principalTable: "QRTZ_TRIGGERS",
                        principalColumns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" },
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "QRTZ_CRON_TRIGGERS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "text", nullable: false),
                    CRON_EXPRESSION = table.Column<string>(type: "text", nullable: false),
                    TIME_ZONE_ID = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_QRTZ_CRON_TRIGGERS",
                        x => new
                        {
                            x.SCHED_NAME,
                            x.TRIGGER_NAME,
                            x.TRIGGER_GROUP
                        }
                    );
                    table.ForeignKey(
                        name: "FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS_SCHED_NAME_TRIGGER_NAME_TRIGGER_GROUP",
                        columns: x => new
                        {
                            x.SCHED_NAME,
                            x.TRIGGER_NAME,
                            x.TRIGGER_GROUP
                        },
                        principalTable: "QRTZ_TRIGGERS",
                        principalColumns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" },
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "QRTZ_SIMPLE_TRIGGERS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "text", nullable: false),
                    REPEAT_COUNT = table.Column<long>(type: "bigint", nullable: false),
                    REPEAT_INTERVAL = table.Column<long>(type: "bigint", nullable: false),
                    TIMES_TRIGGERED = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_QRTZ_SIMPLE_TRIGGERS",
                        x => new
                        {
                            x.SCHED_NAME,
                            x.TRIGGER_NAME,
                            x.TRIGGER_GROUP
                        }
                    );
                    table.ForeignKey(
                        name: "FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS_SCHED_NAME_TRIGGER_NAME_TRIGGER_GROUP",
                        columns: x => new
                        {
                            x.SCHED_NAME,
                            x.TRIGGER_NAME,
                            x.TRIGGER_GROUP
                        },
                        principalTable: "QRTZ_TRIGGERS",
                        principalColumns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" },
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "QRTZ_SIMPROP_TRIGGERS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "text", nullable: false),
                    STR_PROP_1 = table.Column<string>(type: "text", nullable: true),
                    STR_PROP_2 = table.Column<string>(type: "text", nullable: true),
                    STR_PROP_3 = table.Column<string>(type: "text", nullable: true),
                    INT_PROP_1 = table.Column<int>(type: "integer", nullable: true),
                    INT_PROP_2 = table.Column<int>(type: "integer", nullable: true),
                    LONG_PROP_1 = table.Column<long>(type: "bigint", nullable: true),
                    LONG_PROP_2 = table.Column<long>(type: "bigint", nullable: true),
                    DEC_PROP_1 = table.Column<decimal>(type: "numeric", nullable: true),
                    DEC_PROP_2 = table.Column<decimal>(type: "numeric", nullable: true),
                    BOOL_PROP_1 = table.Column<bool>(type: "bool", nullable: true),
                    BOOL_PROP_2 = table.Column<bool>(type: "bool", nullable: true),
                    TIME_ZONE_ID = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_QRTZ_SIMPROP_TRIGGERS",
                        x => new
                        {
                            x.SCHED_NAME,
                            x.TRIGGER_NAME,
                            x.TRIGGER_GROUP
                        }
                    );
                    table.ForeignKey(
                        name: "FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS_SCHED_NAME_TRIGGER_NAME_TRIGGER_GROUP",
                        columns: x => new
                        {
                            x.SCHED_NAME,
                            x.TRIGGER_NAME,
                            x.TRIGGER_GROUP
                        },
                        principalTable: "QRTZ_TRIGGERS",
                        principalColumns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" },
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DownloadTaskMovieFile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    Percentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataReceived = table.Column<long>(type: "INTEGER", nullable: false),
                    DataTotal = table.Column<long>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    FileLocationUrl = table.Column<string>(type: "TEXT", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Quality = table.Column<string>(type: "TEXT", nullable: false),
                    DirectoryMeta = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadSpeed = table.Column<long>(type: "INTEGER", nullable: false),
                    FileTransferSpeed = table.Column<long>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: false)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTaskTvShowSeason", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowSeason_DownloadTaskTvShow_ParentId",
                        column: x => x.ParentId,
                        principalTable: "DownloadTaskTvShow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
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
                name: "PlexMovieGenres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexGenreId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMoviesId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieGenres", x => new { x.PlexMoviesId, x.PlexGenreId });
                    table.ForeignKey(
                        name: "FK_PlexMovieGenres_PlexGenres_PlexGenreId",
                        column: x => x.PlexGenreId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieGenres_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexMovieRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexGenreId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMoviesId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlexRoleId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieRoles", x => new { x.PlexMoviesId, x.PlexGenreId });
                    table.ForeignKey(
                        name: "FK_PlexMovieRoles_PlexGenres_PlexGenreId",
                        column: x => x.PlexGenreId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieRoles_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id"
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieRoles_PlexRoles_PlexRoleId",
                        column: x => x.PlexRoleId,
                        principalTable: "PlexRoles",
                        principalColumn: "Id"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowGenre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexGenreId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowGenre", x => new { x.PlexTvShowId, x.PlexGenreId });
                    table.ForeignKey(
                        name: "FK_PlexTvShowGenre_PlexGenres_PlexGenreId",
                        column: x => x.PlexGenreId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowGenre_PlexTvShows_PlexTvShowId",
                        column: x => x.PlexTvShowId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexGenreId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowRole", x => new { x.PlexTvShowId, x.PlexGenreId });
                    table.ForeignKey(
                        name: "FK_PlexTvShowRole_PlexGenres_PlexGenreId",
                        column: x => x.PlexGenreId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowRole_PlexTvShows_PlexTvShowId",
                        column: x => x.PlexTvShowId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowSeason",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    SortTitle = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaSize = table.Column<long>(type: "INTEGER", nullable: false),
                    MetaDataKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Studio = table.Column<string>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    ContentRating = table.Column<string>(type: "TEXT", nullable: false),
                    Rating = table.Column<double>(type: "REAL", nullable: false),
                    ChildCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OriginallyAvailableAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    HasThumb = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasArt = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasBanner = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasTheme = table.Column<bool>(type: "INTEGER", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
                    MediaData = table.Column<string>(type: "TEXT", nullable: false),
                    ParentKey = table.Column<int>(type: "INTEGER", nullable: false),
                    TvShowId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowSeason", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShowSeason_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowSeason_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowSeason_PlexTvShows_TvShowId",
                        column: x => x.TvShowId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DownloadTaskTvShowEpisode",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTaskTvShowEpisode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTaskTvShowEpisode_DownloadTaskTvShowSeason_ParentId",
                        column: x => x.ParentId,
                        principalTable: "DownloadTaskTvShowSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
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
                name: "PlexTvShowEpisodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    SortTitle = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaSize = table.Column<long>(type: "INTEGER", nullable: false),
                    MetaDataKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Studio = table.Column<string>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    ContentRating = table.Column<string>(type: "TEXT", nullable: false),
                    Rating = table.Column<double>(type: "REAL", nullable: false),
                    ChildCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OriginallyAvailableAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    HasThumb = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasArt = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasBanner = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasTheme = table.Column<bool>(type: "INTEGER", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
                    MediaData = table.Column<string>(type: "TEXT", nullable: false),
                    ParentKey = table.Column<int>(type: "INTEGER", nullable: false),
                    TvShowId = table.Column<int>(type: "INTEGER", nullable: false),
                    TvShowSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowEpisodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodes_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodes_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodes_PlexTvShowSeason_TvShowSeasonId",
                        column: x => x.TvShowSeasonId,
                        principalTable: "PlexTvShowSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodes_PlexTvShows_TvShowId",
                        column: x => x.TvShowId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DownloadTaskTvShowEpisodeFile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    Percentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataReceived = table.Column<long>(type: "INTEGER", nullable: false),
                    DataTotal = table.Column<long>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    FileLocationUrl = table.Column<string>(type: "TEXT", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Quality = table.Column<string>(type: "TEXT", nullable: false),
                    DirectoryMeta = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadSpeed = table.Column<long>(type: "INTEGER", nullable: false),
                    FileTransferSpeed = table.Column<long>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: false)
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

            migrationBuilder.InsertData(
                table: "FolderPaths",
                columns: new[] { "Id", "DirectoryPath", "DisplayName", "FolderType", "MediaType" },
                values: new object[,]
                {
                    { 1, "/Downloads", "Download Path", "DownloadFolder", "None" },
                    { 2, "/Movies", "Movie Destination Path", "MovieFolder", "Movie" },
                    { 3, "/TvShows", "Tv Show Destination Path", "TvShowFolder", "TvShow" },
                    { 4, "/Music", "Music Destination Path", "MusicFolder", "Music" },
                    { 5, "/Photos", "Photos Destination Path", "PhotosFolder", "Photos" },
                    { 6, "/Other", "Other Videos Destination Path", "OtherVideosFolder", "OtherVideos" },
                    { 7, "/Games", "Games Videos Destination Path", "GamesVideosFolder", "Games" },
                    { 8, "/", "Reserved #1 Destination Path", "None", "None" },
                    { 9, "/", "Reserved #2 Destination Path", "None", "None" },
                    { 10, "/", "Reserved #3 Destination Path", "None", "None" }
                }
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
                name: "IX_DownloadTaskTvShowEpisode_ParentId",
                table: "DownloadTaskTvShowEpisode",
                column: "ParentId"
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
                name: "IX_DownloadTaskTvShowSeason_ParentId",
                table: "DownloadTaskTvShowSeason",
                column: "ParentId"
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

            migrationBuilder.CreateIndex(
                name: "IX_DownloadWorkerTasks_DownloadTaskId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadWorkerTasks_PlexServerId",
                table: "DownloadWorkerTasks",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadWorkerTasksLogs_DownloadWorkerTaskId",
                table: "DownloadWorkerTasksLogs",
                column: "DownloadWorkerTaskId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_FileTasks_PlexLibraryId",
                table: "FileTasks",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(name: "IX_FileTasks_PlexServerId", table: "FileTasks", column: "PlexServerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexAccountLibraries_PlexLibraryId",
                table: "PlexAccountLibraries",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexAccountLibraries_PlexServerId",
                table: "PlexAccountLibraries",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexAccountServers_PlexServerId",
                table: "PlexAccountServers",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexLibraries_DefaultDestinationId",
                table: "PlexLibraries",
                column: "DefaultDestinationId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexLibraries_PlexServerId",
                table: "PlexLibraries",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovie_PlexLibraryId",
                table: "PlexMovie",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(name: "IX_PlexMovie_PlexServerId", table: "PlexMovie", column: "PlexServerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieGenres_PlexGenreId",
                table: "PlexMovieGenres",
                column: "PlexGenreId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieGenres_PlexMovieId",
                table: "PlexMovieGenres",
                column: "PlexMovieId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieRoles_PlexGenreId",
                table: "PlexMovieRoles",
                column: "PlexGenreId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieRoles_PlexMovieId",
                table: "PlexMovieRoles",
                column: "PlexMovieId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieRoles_PlexRoleId",
                table: "PlexMovieRoles",
                column: "PlexRoleId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexServerConnections_PlexServerId",
                table: "PlexServerConnections",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexServerStatuses_PlexServerConnectionId",
                table: "PlexServerStatuses",
                column: "PlexServerConnectionId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexServerStatuses_PlexServerId",
                table: "PlexServerStatuses",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodes_PlexLibraryId",
                table: "PlexTvShowEpisodes",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodes_PlexServerId",
                table: "PlexTvShowEpisodes",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodes_TvShowId",
                table: "PlexTvShowEpisodes",
                column: "TvShowId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodes_TvShowSeasonId",
                table: "PlexTvShowEpisodes",
                column: "TvShowSeasonId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowGenre_PlexGenreId",
                table: "PlexTvShowGenre",
                column: "PlexGenreId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowRole_PlexGenreId",
                table: "PlexTvShowRole",
                column: "PlexGenreId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShows_PlexLibraryId",
                table: "PlexTvShows",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShows_PlexServerId",
                table: "PlexTvShows",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowSeason_PlexLibraryId",
                table: "PlexTvShowSeason",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowSeason_PlexServerId",
                table: "PlexTvShowSeason",
                column: "PlexServerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowSeason_TvShowId",
                table: "PlexTvShowSeason",
                column: "TvShowId"
            );

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_JOB_GROUP",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "JOB_GROUP"
            );

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_JOB_NAME",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "JOB_NAME"
            );

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_JOB_REQ_RECOVERY",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "REQUESTS_RECOVERY"
            );

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_TRIG_GROUP",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "TRIGGER_GROUP"
            );

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_TRIG_INST_NAME",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "INSTANCE_NAME"
            );

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_TRIG_NAME",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "TRIGGER_NAME"
            );

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_TRIG_NM_GP",
                table: "QRTZ_FIRED_TRIGGERS",
                columns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" }
            );

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_J_REQ_RECOVERY",
                table: "QRTZ_JOB_DETAILS",
                column: "REQUESTS_RECOVERY"
            );

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_T_NEXT_FIRE_TIME",
                table: "QRTZ_TRIGGERS",
                column: "NEXT_FIRE_TIME"
            );

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_T_NFT_ST",
                table: "QRTZ_TRIGGERS",
                columns: new[] { "NEXT_FIRE_TIME", "TRIGGER_STATE" }
            );

            migrationBuilder.CreateIndex(name: "IDX_QRTZ_T_STATE", table: "QRTZ_TRIGGERS", column: "TRIGGER_STATE");

            migrationBuilder.CreateIndex(
                name: "IX_QRTZ_TRIGGERS_SCHED_NAME_JOB_NAME_JOB_GROUP",
                table: "QRTZ_TRIGGERS",
                columns: new[] { "SCHED_NAME", "JOB_NAME", "JOB_GROUP" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DownloadTaskMovieFile");

            migrationBuilder.DropTable(name: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropTable(name: "DownloadWorkerTasksLogs");

            migrationBuilder.DropTable(name: "FileTasks");

            migrationBuilder.DropTable(name: "Notifications");

            migrationBuilder.DropTable(name: "PlexAccountLibraries");

            migrationBuilder.DropTable(name: "PlexAccountServers");

            migrationBuilder.DropTable(name: "PlexMovieGenres");

            migrationBuilder.DropTable(name: "PlexMovieRoles");

            migrationBuilder.DropTable(name: "PlexServerStatuses");

            migrationBuilder.DropTable(name: "PlexTvShowEpisodes");

            migrationBuilder.DropTable(name: "PlexTvShowGenre");

            migrationBuilder.DropTable(name: "PlexTvShowRole");

            migrationBuilder.DropTable(name: "QRTZ_BLOB_TRIGGERS");

            migrationBuilder.DropTable(name: "QRTZ_CALENDARS");

            migrationBuilder.DropTable(name: "QRTZ_CRON_TRIGGERS");

            migrationBuilder.DropTable(name: "QRTZ_FIRED_TRIGGERS");

            migrationBuilder.DropTable(name: "QRTZ_LOCKS");

            migrationBuilder.DropTable(name: "QRTZ_PAUSED_TRIGGER_GRPS");

            migrationBuilder.DropTable(name: "QRTZ_SCHEDULER_STATE");

            migrationBuilder.DropTable(name: "QRTZ_SIMPLE_TRIGGERS");

            migrationBuilder.DropTable(name: "QRTZ_SIMPROP_TRIGGERS");

            migrationBuilder.DropTable(name: "DownloadTaskMovie");

            migrationBuilder.DropTable(name: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropTable(name: "DownloadWorkerTasks");

            migrationBuilder.DropTable(name: "PlexAccounts");

            migrationBuilder.DropTable(name: "PlexMovie");

            migrationBuilder.DropTable(name: "PlexRoles");

            migrationBuilder.DropTable(name: "PlexServerConnections");

            migrationBuilder.DropTable(name: "PlexTvShowSeason");

            migrationBuilder.DropTable(name: "PlexGenres");

            migrationBuilder.DropTable(name: "QRTZ_TRIGGERS");

            migrationBuilder.DropTable(name: "DownloadTaskTvShowSeason");

            migrationBuilder.DropTable(name: "PlexTvShows");

            migrationBuilder.DropTable(name: "QRTZ_JOB_DETAILS");

            migrationBuilder.DropTable(name: "DownloadTaskTvShow");

            migrationBuilder.DropTable(name: "PlexLibraries");

            migrationBuilder.DropTable(name: "FolderPaths");

            migrationBuilder.DropTable(name: "PlexServers");
        }
    }
}
