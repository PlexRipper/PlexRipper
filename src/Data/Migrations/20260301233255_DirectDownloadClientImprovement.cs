using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class DirectDownloadClientImprovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DownloadWorkerTasksLogs",
                table: "DownloadWorkerTasksLogs");

            migrationBuilder.RenameTable(
                name: "DownloadWorkerTasksLogs",
                newName: "DownloadTasksLogs");

            migrationBuilder.AddColumn<bool>(
                name: "IsDownloadsPausedByUser",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 16);

            migrationBuilder.AddColumn<string>(
                name: "DirectDownloadSnapshot",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 19);

            migrationBuilder.AddColumn<string>(
                name: "DownloadClientType",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "TEXT",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: "Direct");

            migrationBuilder.AddColumn<string>(
                name: "DirectDownloadSnapshot",
                table: "DownloadTaskMovieFile",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 19);

            migrationBuilder.AddColumn<string>(
                name: "DownloadClientType",
                table: "DownloadTaskMovieFile",
                type: "TEXT",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: "Direct");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "DownloadTasksLogs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "LogLevel",
                table: "DownloadTasksLogs",
                type: "TEXT",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "None",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldUnicode: false,
                oldMaxLength: 20)
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "DownloadTasksLogs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "DownloadTasksLogs",
                type: "TEXT",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "Unknown")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DownloadTasksLogs",
                table: "DownloadTasksLogs",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DownloadTasksLogs",
                table: "DownloadTasksLogs");

            migrationBuilder.DropColumn(
                name: "IsDownloadsPausedByUser",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "DirectDownloadSnapshot",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(
                name: "DownloadClientType",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(
                name: "DirectDownloadSnapshot",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(
                name: "DownloadClientType",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DownloadTasksLogs");

            migrationBuilder.RenameTable(
                name: "DownloadTasksLogs",
                newName: "DownloadWorkerTasksLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "DownloadWorkerTasksLogs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "LogLevel",
                table: "DownloadWorkerTasksLogs",
                type: "TEXT",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldUnicode: false,
                oldMaxLength: 20,
                oldDefaultValue: "None")
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "DownloadWorkerTasksLogs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DownloadWorkerTasksLogs",
                table: "DownloadWorkerTasksLogs",
                column: "Id");
        }
    }
}
