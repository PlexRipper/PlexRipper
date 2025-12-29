using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioChannels",
                table: "PlexTvShowEpisodeData");

            migrationBuilder.DropColumn(
                name: "FrameRate",
                table: "PlexTvShowEpisodeData");

            migrationBuilder.DropColumn(
                name: "AudioChannels",
                table: "PlexMovieData");

            migrationBuilder.DropColumn(
                name: "FrameRate",
                table: "PlexMovieData");

            migrationBuilder.RenameColumn(
                name: "LastSyncedAt",
                table: "PlexTvShowEpisodeData",
                newName: "GeneratedNameSyncedAt");

            migrationBuilder.RenameColumn(
                name: "HasMetadata",
                table: "PlexTvShowEpisodeData",
                newName: "NeedsGeneratedName");

            migrationBuilder.RenameColumn(
                name: "LastSyncedAt",
                table: "PlexMovieData",
                newName: "GeneratedNameSyncedAt");

            migrationBuilder.RenameColumn(
                name: "HasMetadata",
                table: "PlexMovieData",
                newName: "NeedsGeneratedName");

            migrationBuilder.AlterColumn<string>(
                name: "VideoResolution",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder.AlterColumn<int>(
                name: "Source",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<int>(
                name: "PlexServerId",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 19);

            migrationBuilder.AlterColumn<int>(
                name: "PlexLibraryId",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 18);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFilename",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 10)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "GeneratedFilename",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<string>(
                name: "Container",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<string>(
                name: "AudioCodec",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 13)
                .OldAnnotation("Relational:ColumnOrder", 16);

            migrationBuilder.AlterColumn<DateTime>(
                name: "GeneratedNameSyncedAt",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 15)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<bool>(
                name: "NeedsGeneratedName",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 14)
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<string>(
                name: "VideoResolution",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder.AlterColumn<int>(
                name: "Source",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<int>(
                name: "PlexServerId",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 19);

            migrationBuilder.AlterColumn<int>(
                name: "PlexLibraryId",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 18);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFilename",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 10)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "GeneratedFilename",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<string>(
                name: "Container",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<string>(
                name: "AudioCodec",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 13)
                .OldAnnotation("Relational:ColumnOrder", 16);

            migrationBuilder.AlterColumn<DateTime>(
                name: "GeneratedNameSyncedAt",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 15)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<bool>(
                name: "NeedsGeneratedName",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 14)
                .OldAnnotation("Relational:ColumnOrder", 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NeedsGeneratedName",
                table: "PlexTvShowEpisodeData",
                newName: "HasMetadata");

            migrationBuilder.RenameColumn(
                name: "GeneratedNameSyncedAt",
                table: "PlexTvShowEpisodeData",
                newName: "LastSyncedAt");

            migrationBuilder.RenameColumn(
                name: "NeedsGeneratedName",
                table: "PlexMovieData",
                newName: "HasMetadata");

            migrationBuilder.RenameColumn(
                name: "GeneratedNameSyncedAt",
                table: "PlexMovieData",
                newName: "LastSyncedAt");

            migrationBuilder.AlterColumn<string>(
                name: "VideoResolution",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 14)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<int>(
                name: "Source",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 15)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<int>(
                name: "PlexServerId",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 19);

            migrationBuilder.AlterColumn<int>(
                name: "PlexLibraryId",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 18);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFilename",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<string>(
                name: "GeneratedFilename",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<string>(
                name: "Container",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<string>(
                name: "AudioCodec",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 16)
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<bool>(
                name: "HasMetadata",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 10)
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSyncedAt",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AddColumn<int>(
                name: "AudioChannels",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder.AddColumn<string>(
                name: "FrameRate",
                table: "PlexTvShowEpisodeData",
                type: "TEXT",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<string>(
                name: "VideoResolution",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 14)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<int>(
                name: "Source",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 15)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<int>(
                name: "PlexServerId",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 19);

            migrationBuilder.AlterColumn<int>(
                name: "PlexLibraryId",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 18);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFilename",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<string>(
                name: "GeneratedFilename",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<string>(
                name: "Container",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<string>(
                name: "AudioCodec",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 16)
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<bool>(
                name: "HasMetadata",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 10)
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSyncedAt",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AddColumn<int>(
                name: "AudioChannels",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder.AddColumn<string>(
                name: "FrameRate",
                table: "PlexMovieData",
                type: "TEXT",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 13);
        }
    }
}
