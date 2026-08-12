using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToTinkerQTableLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackgroundJobLibraryComparisonJobQueues");

            migrationBuilder.DropTable(
                name: "QRTZ_BLOB_TRIGGERS");

            migrationBuilder.DropTable(
                name: "QRTZ_CALENDARS");

            migrationBuilder.DropTable(
                name: "QRTZ_CRON_TRIGGERS");

            migrationBuilder.DropTable(
                name: "QRTZ_FIRED_TRIGGERS");

            migrationBuilder.DropTable(
                name: "QRTZ_LOCKS");

            migrationBuilder.DropTable(
                name: "QRTZ_PAUSED_TRIGGER_GRPS");

            migrationBuilder.DropTable(
                name: "QRTZ_SCHEDULER_STATE");

            migrationBuilder.DropTable(
                name: "QRTZ_SIMPLE_TRIGGERS");

            migrationBuilder.DropTable(
                name: "QRTZ_SIMPROP_TRIGGERS");

            migrationBuilder.DropTable(
                name: "QRTZ_TRIGGERS");

            migrationBuilder.DropTable(
                name: "QRTZ_JOB_DETAILS");

            migrationBuilder.EnsureSchema(
                name: "ticker");

            migrationBuilder.CreateTable(
                name: "CronTickers",
                schema: "ticker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobKey = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    JobType = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: false, defaultValue: "None"),
                    Function = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    InitIdentifier = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Expression = table.Column<string>(type: "TEXT", nullable: true),
                    Request = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Retries = table.Column<int>(type: "INTEGER", nullable: false),
                    RetryIntervals = table.Column<string>(type: "TEXT", nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSystemPaused = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CronTickers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeTickers",
                schema: "ticker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobKey = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    JobType = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: false, defaultValue: "None"),
                    Function = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    InitIdentifier = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LockHolder = table.Column<string>(type: "TEXT", nullable: true),
                    Request = table.Column<byte[]>(type: "BLOB", nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LockedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExceptionMessage = table.Column<string>(type: "TEXT", nullable: true),
                    SkippedReason = table.Column<string>(type: "TEXT", nullable: true),
                    ElapsedTime = table.Column<long>(type: "INTEGER", nullable: false),
                    Retries = table.Column<int>(type: "INTEGER", nullable: false),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    RetryIntervals = table.Column<string>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RunCondition = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTickers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeTickers_TimeTickers_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "ticker",
                        principalTable: "TimeTickers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CronTickerOccurrences",
                schema: "ticker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LockHolder = table.Column<string>(type: "TEXT", nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CronTickerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExceptionMessage = table.Column<string>(type: "TEXT", nullable: true),
                    SkippedReason = table.Column<string>(type: "TEXT", nullable: true),
                    ElapsedTime = table.Column<long>(type: "INTEGER", nullable: false),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CronTickerOccurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CronTickerOccurrences_CronTickers_CronTickerId",
                        column: x => x.CronTickerId,
                        principalSchema: "ticker",
                        principalTable: "CronTickers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_CronTickerId",
                schema: "ticker",
                table: "CronTickerOccurrences",
                column: "CronTickerId");

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_ExecutionTime",
                schema: "ticker",
                table: "CronTickerOccurrences",
                column: "ExecutionTime");

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_Status_ExecutionTime",
                schema: "ticker",
                table: "CronTickerOccurrences",
                columns: new[] { "Status", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "UQ_CronTickerId_ExecutionTime",
                schema: "ticker",
                table: "CronTickerOccurrences",
                columns: new[] { "CronTickerId", "ExecutionTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CronTickers_Expression",
                schema: "ticker",
                table: "CronTickers",
                column: "Expression");

            migrationBuilder.CreateIndex(
                name: "IX_CronTickers_JobKey",
                schema: "ticker",
                table: "CronTickers",
                column: "JobKey");

            migrationBuilder.CreateIndex(
                name: "IX_CronTickers_JobType",
                schema: "ticker",
                table: "CronTickers",
                column: "JobType");

            migrationBuilder.CreateIndex(
                name: "IX_Function_Expression",
                schema: "ticker",
                table: "CronTickers",
                columns: new[] { "Function", "Expression" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers",
                column: "ExecutionTime");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_Status_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers",
                columns: new[] { "Status", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeTickers_JobKey",
                schema: "ticker",
                table: "TimeTickers",
                column: "JobKey");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTickers_JobType",
                schema: "ticker",
                table: "TimeTickers",
                column: "JobType");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTickers_ParentId",
                schema: "ticker",
                table: "TimeTickers",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CronTickerOccurrences",
                schema: "ticker");

            migrationBuilder.DropTable(
                name: "TimeTickers",
                schema: "ticker");

            migrationBuilder.DropTable(
                name: "CronTickers",
                schema: "ticker");

            migrationBuilder.CreateTable(
                name: "BackgroundJobLibraryComparisonJobQueues",
                columns: table => new
                {
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    RemotePlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnedPlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaType = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundJobLibraryComparisonJobQueues", x => new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.MediaType });
                    table.ForeignKey(
                        name: "FK_BackgroundJobLibraryComparisonJobQueues_PlexLibraries_OwnedPlexLibraryId",
                        column: x => x.OwnedPlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BackgroundJobLibraryComparisonJobQueues_PlexLibraries_RemotePlexLibraryId",
                        column: x => x.RemotePlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_FIRED_TRIGGERS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    ENTRY_ID = table.Column<string>(type: "text", nullable: false),
                    FIRED_TIME = table.Column<long>(type: "bigint", nullable: false),
                    INSTANCE_NAME = table.Column<string>(type: "text", nullable: false),
                    IS_NONCONCURRENT = table.Column<bool>(type: "bool", nullable: false),
                    JOB_GROUP = table.Column<string>(type: "text", nullable: true),
                    JOB_NAME = table.Column<string>(type: "text", nullable: true),
                    PRIORITY = table.Column<int>(type: "integer", nullable: false),
                    REQUESTS_RECOVERY = table.Column<bool>(type: "bool", nullable: true),
                    SCHED_TIME = table.Column<long>(type: "bigint", nullable: false),
                    STATE = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_FIRED_TRIGGERS", x => new { x.SCHED_NAME, x.ENTRY_ID });
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_JOB_DETAILS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    JOB_NAME = table.Column<string>(type: "text", nullable: false),
                    JOB_GROUP = table.Column<string>(type: "text", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "text", nullable: true),
                    IS_DURABLE = table.Column<bool>(type: "bool", nullable: false),
                    IS_NONCONCURRENT = table.Column<bool>(type: "bool", nullable: false),
                    IS_UPDATE_DATA = table.Column<bool>(type: "bool", nullable: false),
                    JOB_CLASS_NAME = table.Column<string>(type: "text", nullable: false),
                    JOB_DATA = table.Column<byte[]>(type: "bytea", nullable: true),
                    REQUESTS_RECOVERY = table.Column<bool>(type: "bool", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_JOB_DETAILS", x => new { x.SCHED_NAME, x.JOB_NAME, x.JOB_GROUP });
                });

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
                });

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
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_SCHEDULER_STATE",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    INSTANCE_NAME = table.Column<string>(type: "text", nullable: false),
                    CHECKIN_INTERVAL = table.Column<long>(type: "bigint", nullable: false),
                    LAST_CHECKIN_TIME = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_SCHEDULER_STATE", x => new { x.SCHED_NAME, x.INSTANCE_NAME });
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_TRIGGERS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "text", nullable: false),
                    JOB_NAME = table.Column<string>(type: "text", nullable: false),
                    JOB_GROUP = table.Column<string>(type: "text", nullable: false),
                    CALENDAR_NAME = table.Column<string>(type: "text", nullable: true),
                    DESCRIPTION = table.Column<string>(type: "text", nullable: true),
                    END_TIME = table.Column<long>(type: "bigint", nullable: true),
                    JOB_DATA = table.Column<byte[]>(type: "bytea", nullable: true),
                    MISFIRE_INSTR = table.Column<short>(type: "smallint", nullable: true),
                    MISFIRE_ORIG_FIRE_TIME = table.Column<long>(type: "bigint", nullable: true),
                    NEXT_FIRE_TIME = table.Column<long>(type: "bigint", nullable: true),
                    PREV_FIRE_TIME = table.Column<long>(type: "bigint", nullable: true),
                    PRIORITY = table.Column<int>(type: "integer", nullable: true),
                    START_TIME = table.Column<long>(type: "bigint", nullable: false),
                    TRIGGER_STATE = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_TYPE = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_TRIGGERS", x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP });
                    table.ForeignKey(
                        name: "FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS_SCHED_NAME_JOB_NAME_JOB_GROUP",
                        columns: x => new { x.SCHED_NAME, x.JOB_NAME, x.JOB_GROUP },
                        principalTable: "QRTZ_JOB_DETAILS",
                        principalColumns: new[] { "SCHED_NAME", "JOB_NAME", "JOB_GROUP" },
                        onDelete: ReferentialAction.Cascade);
                });

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
                    table.PrimaryKey("PK_QRTZ_BLOB_TRIGGERS", x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP });
                    table.ForeignKey(
                        name: "FK_QRTZ_BLOB_TRIGGERS_QRTZ_TRIGGERS_SCHED_NAME_TRIGGER_NAME_TRIGGER_GROUP",
                        columns: x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP },
                        principalTable: "QRTZ_TRIGGERS",
                        principalColumns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" },
                        onDelete: ReferentialAction.Cascade);
                });

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
                    table.PrimaryKey("PK_QRTZ_CRON_TRIGGERS", x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP });
                    table.ForeignKey(
                        name: "FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS_SCHED_NAME_TRIGGER_NAME_TRIGGER_GROUP",
                        columns: x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP },
                        principalTable: "QRTZ_TRIGGERS",
                        principalColumns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" },
                        onDelete: ReferentialAction.Cascade);
                });

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
                    table.PrimaryKey("PK_QRTZ_SIMPLE_TRIGGERS", x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP });
                    table.ForeignKey(
                        name: "FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS_SCHED_NAME_TRIGGER_NAME_TRIGGER_GROUP",
                        columns: x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP },
                        principalTable: "QRTZ_TRIGGERS",
                        principalColumns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QRTZ_SIMPROP_TRIGGERS",
                columns: table => new
                {
                    SCHED_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_NAME = table.Column<string>(type: "text", nullable: false),
                    TRIGGER_GROUP = table.Column<string>(type: "text", nullable: false),
                    BOOL_PROP_1 = table.Column<bool>(type: "bool", nullable: true),
                    BOOL_PROP_2 = table.Column<bool>(type: "bool", nullable: true),
                    DEC_PROP_1 = table.Column<decimal>(type: "numeric", nullable: true),
                    DEC_PROP_2 = table.Column<decimal>(type: "numeric", nullable: true),
                    INT_PROP_1 = table.Column<int>(type: "integer", nullable: true),
                    INT_PROP_2 = table.Column<int>(type: "integer", nullable: true),
                    LONG_PROP_1 = table.Column<long>(type: "bigint", nullable: true),
                    LONG_PROP_2 = table.Column<long>(type: "bigint", nullable: true),
                    STR_PROP_1 = table.Column<string>(type: "text", nullable: true),
                    STR_PROP_2 = table.Column<string>(type: "text", nullable: true),
                    STR_PROP_3 = table.Column<string>(type: "text", nullable: true),
                    TIME_ZONE_ID = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRTZ_SIMPROP_TRIGGERS", x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP });
                    table.ForeignKey(
                        name: "FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS_SCHED_NAME_TRIGGER_NAME_TRIGGER_GROUP",
                        columns: x => new { x.SCHED_NAME, x.TRIGGER_NAME, x.TRIGGER_GROUP },
                        principalTable: "QRTZ_TRIGGERS",
                        principalColumns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobLibraryComparisonJobQueues_OwnedPlexLibraryId",
                table: "BackgroundJobLibraryComparisonJobQueues",
                column: "OwnedPlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobLibraryComparisonJobQueues_Status_Priority_CreatedAt",
                table: "BackgroundJobLibraryComparisonJobQueues",
                columns: new[] { "Status", "Priority", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_JOB_GROUP",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "JOB_GROUP");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_JOB_NAME",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "JOB_NAME");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_JOB_REQ_RECOVERY",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "REQUESTS_RECOVERY");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_TRIG_GROUP",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "TRIGGER_GROUP");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_TRIG_INST_NAME",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "INSTANCE_NAME");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_TRIG_NAME",
                table: "QRTZ_FIRED_TRIGGERS",
                column: "TRIGGER_NAME");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_FT_TRIG_NM_GP",
                table: "QRTZ_FIRED_TRIGGERS",
                columns: new[] { "SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP" });

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_J_REQ_RECOVERY",
                table: "QRTZ_JOB_DETAILS",
                column: "REQUESTS_RECOVERY");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_T_NEXT_FIRE_TIME",
                table: "QRTZ_TRIGGERS",
                column: "NEXT_FIRE_TIME");

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_T_NFT_ST",
                table: "QRTZ_TRIGGERS",
                columns: new[] { "NEXT_FIRE_TIME", "TRIGGER_STATE" });

            migrationBuilder.CreateIndex(
                name: "IDX_QRTZ_T_STATE",
                table: "QRTZ_TRIGGERS",
                column: "TRIGGER_STATE");

            migrationBuilder.CreateIndex(
                name: "IX_QRTZ_TRIGGERS_SCHED_NAME_JOB_NAME_JOB_GROUP",
                table: "QRTZ_TRIGGERS",
                columns: new[] { "SCHED_NAME", "JOB_NAME", "JOB_GROUP" });
        }
    }
}
