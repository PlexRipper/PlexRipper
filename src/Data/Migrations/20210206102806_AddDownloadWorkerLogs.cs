using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class AddDownloadWorkerLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DownloadStatus",
                table: "DownloadWorkerTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DownloadWorkerTasksLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LogLevel = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadWorkerTasksLogs_DownloadWorkerTaskId",
                table: "DownloadWorkerTasksLogs",
                column: "DownloadWorkerTaskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadWorkerTasksLogs");

            migrationBuilder.DropColumn(
                name: "DownloadStatus",
                table: "DownloadWorkerTasks");
        }
    }
}
