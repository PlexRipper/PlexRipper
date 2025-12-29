using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PlexMovieDataStreams");

            migrationBuilder.DropTable(name: "PlexTvShowEpisodeDataStreams");

            migrationBuilder.DropColumn(name: "AudioLanguages", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "AudioProfile", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "BitDepth", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "Category", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "ColorSpace", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "File", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "HasAtmos", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "HasForcedSubs", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "HasSdhSubs", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "Height", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "Indexes", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "IsDolbyVision", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "IsHdr", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "IsHdr10", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "MaxAudioChannels", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "SubtitleLanguages", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "VideoBitrate", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "Width", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "AudioLanguages", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "AudioProfile", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "BitDepth", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "Category", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "ColorSpace", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "File", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "HasAtmos", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "HasForcedSubs", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "HasSdhSubs", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "Height", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "Indexes", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "IsDolbyVision", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "IsHdr", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "IsHdr10", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "MaxAudioChannels", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "SubtitleLanguages", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "VideoBitrate", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "Width", table: "PlexMovieDataParts");

            migrationBuilder.RenameColumn(
                name: "VideoProfile",
                table: "PlexTvShowEpisodeDataParts",
                newName: "OriginalFilename"
            );

            migrationBuilder.RenameColumn(
                name: "ReleaseTitle",
                table: "PlexTvShowEpisodeDataParts",
                newName: "GeneratedFilename"
            );

            migrationBuilder.RenameColumn(
                name: "PrimaryAudioCodec",
                table: "PlexTvShowEpisodeDataParts",
                newName: "AudioCodec"
            );

            migrationBuilder.RenameColumn(
                name: "VideoProfile",
                table: "PlexMovieDataParts",
                newName: "OriginalFilename"
            );

            migrationBuilder.RenameColumn(
                name: "ReleaseTitle",
                table: "PlexMovieDataParts",
                newName: "GeneratedFilename"
            );

            migrationBuilder.RenameColumn(
                name: "PrimaryAudioCodec",
                table: "PlexMovieDataParts",
                newName: "AudioCodec"
            );

            migrationBuilder
                .AlterColumn<string>(
                    name: "VideoCodec",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 11);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Source",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 14);

            migrationBuilder
                .AlterColumn<long>(
                    name: "Size",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 7);

            migrationBuilder
                .AlterColumn<string>(
                    name: "Resolution",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 13);

            migrationBuilder
                .AlterColumn<int>(
                    name: "RatingKey",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexServerId",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 18);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexLibraryId",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder
                .AlterColumn<long>(
                    name: "PlexId",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder
                .AlterColumn<DateTime>(
                    name: "LastSyncedAt",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(DateTime),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 10);

            migrationBuilder
                .AlterColumn<string>(
                    name: "Key",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "HasMetadata",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder
                .AlterColumn<decimal>(
                    name: "FrameRate",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(decimal),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 12);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Duration",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AlterColumn<string>(
                    name: "Container",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder
                .AlterColumn<string>(
                    name: "AudioChannels",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 16);

            migrationBuilder
                .AlterColumn<string>(
                    name: "OriginalFilename",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder
                .AlterColumn<string>(
                    name: "GeneratedFilename",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder
                .AlterColumn<string>(
                    name: "AudioCodec",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 15);

            migrationBuilder
                .AlterColumn<string>(
                    name: "VideoCodec",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 11);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Source",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 14);

            migrationBuilder
                .AlterColumn<long>(
                    name: "Size",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 7);

            migrationBuilder
                .AlterColumn<string>(
                    name: "Resolution",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 13);

            migrationBuilder
                .AlterColumn<int>(
                    name: "RatingKey",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexServerId",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 18);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexLibraryId",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder
                .AlterColumn<long>(
                    name: "PlexId",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder
                .AlterColumn<DateTime>(
                    name: "LastSyncedAt",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(DateTime),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 10);

            migrationBuilder
                .AlterColumn<string>(
                    name: "Key",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "HasMetadata",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder
                .AlterColumn<decimal>(
                    name: "FrameRate",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(decimal),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 12);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Duration",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AlterColumn<string>(
                    name: "Container",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder
                .AlterColumn<string>(
                    name: "AudioChannels",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 16);

            migrationBuilder
                .AlterColumn<string>(
                    name: "OriginalFilename",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder
                .AlterColumn<string>(
                    name: "GeneratedFilename",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder
                .AlterColumn<string>(
                    name: "AudioCodec",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .Annotation("Relational:ColumnOrder", 15);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OriginalFilename",
                table: "PlexTvShowEpisodeDataParts",
                newName: "VideoProfile"
            );

            migrationBuilder.RenameColumn(
                name: "GeneratedFilename",
                table: "PlexTvShowEpisodeDataParts",
                newName: "ReleaseTitle"
            );

            migrationBuilder.RenameColumn(
                name: "AudioCodec",
                table: "PlexTvShowEpisodeDataParts",
                newName: "PrimaryAudioCodec"
            );

            migrationBuilder.RenameColumn(
                name: "OriginalFilename",
                table: "PlexMovieDataParts",
                newName: "VideoProfile"
            );

            migrationBuilder.RenameColumn(
                name: "GeneratedFilename",
                table: "PlexMovieDataParts",
                newName: "ReleaseTitle"
            );

            migrationBuilder.RenameColumn(
                name: "AudioCodec",
                table: "PlexMovieDataParts",
                newName: "PrimaryAudioCodec"
            );

            migrationBuilder
                .AlterColumn<string>(
                    name: "VideoCodec",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Source",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder
                .AlterColumn<long>(
                    name: "Size",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder
                .AlterColumn<string>(
                    name: "Resolution",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder
                .AlterColumn<int>(
                    name: "RatingKey",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexServerId",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 18);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexLibraryId",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 17);

            migrationBuilder
                .AlterColumn<long>(
                    name: "PlexId",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder
                .AlterColumn<DateTime>(
                    name: "LastSyncedAt",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(DateTime),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder
                .AlterColumn<string>(
                    name: "Key",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "HasMetadata",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder
                .AlterColumn<decimal>(
                    name: "FrameRate",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(decimal),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Duration",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AlterColumn<string>(
                    name: "Container",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder
                .AlterColumn<string>(
                    name: "AudioChannels",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 16);

            migrationBuilder
                .AlterColumn<string>(
                    name: "VideoProfile",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder
                .AlterColumn<string>(
                    name: "ReleaseTitle",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder
                .AlterColumn<string>(
                    name: "PrimaryAudioCodec",
                    table: "PlexTvShowEpisodeDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AddColumn<string>(
                name: "AudioLanguages",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "AudioProfile",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<int>(
                name: "BitDepth",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "ColorSpace",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "File",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<bool>(
                name: "HasAtmos",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "HasForcedSubs",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "HasSdhSubs",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "Indexes",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDolbyVision",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsHdr",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsHdr10",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<int>(
                name: "MaxAudioChannels",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "SubtitleLanguages",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "VideoBitrate",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder
                .AlterColumn<string>(
                    name: "VideoCodec",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Source",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder
                .AlterColumn<long>(
                    name: "Size",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder
                .AlterColumn<string>(
                    name: "Resolution",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder
                .AlterColumn<int>(
                    name: "RatingKey",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexServerId",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 18);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexLibraryId",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 17);

            migrationBuilder
                .AlterColumn<long>(
                    name: "PlexId",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder
                .AlterColumn<DateTime>(
                    name: "LastSyncedAt",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(DateTime),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder
                .AlterColumn<string>(
                    name: "Key",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "HasMetadata",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder
                .AlterColumn<decimal>(
                    name: "FrameRate",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(decimal),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Duration",
                    table: "PlexMovieDataParts",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AlterColumn<string>(
                    name: "Container",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder
                .AlterColumn<string>(
                    name: "AudioChannels",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 16);

            migrationBuilder
                .AlterColumn<string>(
                    name: "VideoProfile",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder
                .AlterColumn<string>(
                    name: "ReleaseTitle",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder
                .AlterColumn<string>(
                    name: "PrimaryAudioCodec",
                    table: "PlexMovieDataParts",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "TEXT"
                )
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AddColumn<string>(
                name: "AudioLanguages",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "AudioProfile",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<int>(
                name: "BitDepth",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "ColorSpace",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "File",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<bool>(
                name: "HasAtmos",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "HasForcedSubs",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "HasSdhSubs",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "Indexes",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDolbyVision",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsHdr",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsHdr10",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<int>(
                name: "MaxAudioChannels",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "SubtitleLanguages",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "VideoBitrate",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.CreateTable(
                name: "PlexMovieDataStreams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieMediaDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieMediaDataPartId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    AudioChannelLayout = table.Column<string>(type: "TEXT", nullable: true),
                    BitDepth = table.Column<int>(type: "INTEGER", nullable: true),
                    Bitrate = table.Column<int>(type: "INTEGER", nullable: false),
                    CanAutoSync = table.Column<bool>(type: "INTEGER", nullable: true),
                    Channels = table.Column<int>(type: "INTEGER", nullable: true),
                    ChromaLocation = table.Column<string>(type: "TEXT", nullable: true),
                    ChromaSubsampling = table.Column<string>(type: "TEXT", nullable: true),
                    Codec = table.Column<string>(type: "TEXT", nullable: false),
                    CodedHeight = table.Column<int>(type: "INTEGER", nullable: true),
                    CodedWidth = table.Column<int>(type: "INTEGER", nullable: true),
                    ColorPrimaries = table.Column<string>(type: "TEXT", nullable: true),
                    ColorRange = table.Column<string>(type: "TEXT", nullable: true),
                    ColorSpace = table.Column<string>(type: "TEXT", nullable: true),
                    ColorTrc = table.Column<string>(type: "TEXT", nullable: true),
                    DOVIBLCompatID = table.Column<int>(type: "INTEGER", nullable: true),
                    DOVIBLPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVIELPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVILevel = table.Column<int>(type: "INTEGER", nullable: true),
                    DOVIPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVIProfile = table.Column<int>(type: "INTEGER", nullable: true),
                    DOVIRPUPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVIVersion = table.Column<string>(type: "TEXT", nullable: true),
                    Default = table.Column<bool>(type: "INTEGER", nullable: true),
                    DisplayTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Dub = table.Column<bool>(type: "INTEGER", nullable: true),
                    ExtendedDisplayTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Forced = table.Column<bool>(type: "INTEGER", nullable: true),
                    FrameRate = table.Column<float>(type: "REAL", nullable: true),
                    HasScalingMatrix = table.Column<bool>(type: "INTEGER", nullable: true),
                    HearingImpaired = table.Column<bool>(type: "INTEGER", nullable: true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageTag = table.Column<string>(type: "TEXT", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: true),
                    Original = table.Column<bool>(type: "INTEGER", nullable: true),
                    PlexId = table.Column<long>(type: "INTEGER", nullable: false),
                    Profile = table.Column<string>(type: "TEXT", nullable: true),
                    RefFrames = table.Column<int>(type: "INTEGER", nullable: true),
                    SamplingRate = table.Column<int>(type: "INTEGER", nullable: true),
                    ScanType = table.Column<string>(type: "TEXT", nullable: true),
                    Selected = table.Column<bool>(type: "INTEGER", nullable: true),
                    StreamType = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Width = table.Column<int>(type: "INTEGER", nullable: true),
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowEpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowEpisodeMediaDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowEpisodeMediaDataPartId = table.Column<int>(type: "INTEGER", nullable: false),
                    AudioChannelLayout = table.Column<string>(type: "TEXT", nullable: true),
                    BitDepth = table.Column<int>(type: "INTEGER", nullable: true),
                    Bitrate = table.Column<int>(type: "INTEGER", nullable: false),
                    CanAutoSync = table.Column<bool>(type: "INTEGER", nullable: true),
                    Channels = table.Column<int>(type: "INTEGER", nullable: true),
                    ChromaLocation = table.Column<string>(type: "TEXT", nullable: true),
                    ChromaSubsampling = table.Column<string>(type: "TEXT", nullable: true),
                    Codec = table.Column<string>(type: "TEXT", nullable: false),
                    CodedHeight = table.Column<int>(type: "INTEGER", nullable: true),
                    CodedWidth = table.Column<int>(type: "INTEGER", nullable: true),
                    ColorPrimaries = table.Column<string>(type: "TEXT", nullable: true),
                    ColorRange = table.Column<string>(type: "TEXT", nullable: true),
                    ColorSpace = table.Column<string>(type: "TEXT", nullable: true),
                    ColorTrc = table.Column<string>(type: "TEXT", nullable: true),
                    DOVIBLCompatID = table.Column<int>(type: "INTEGER", nullable: true),
                    DOVIBLPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVIELPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVILevel = table.Column<int>(type: "INTEGER", nullable: true),
                    DOVIPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVIProfile = table.Column<int>(type: "INTEGER", nullable: true),
                    DOVIRPUPresent = table.Column<bool>(type: "INTEGER", nullable: true),
                    DOVIVersion = table.Column<string>(type: "TEXT", nullable: true),
                    Default = table.Column<bool>(type: "INTEGER", nullable: true),
                    DisplayTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Dub = table.Column<bool>(type: "INTEGER", nullable: true),
                    ExtendedDisplayTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Forced = table.Column<bool>(type: "INTEGER", nullable: true),
                    FrameRate = table.Column<float>(type: "REAL", nullable: true),
                    HasScalingMatrix = table.Column<bool>(type: "INTEGER", nullable: true),
                    HearingImpaired = table.Column<bool>(type: "INTEGER", nullable: true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageTag = table.Column<string>(type: "TEXT", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: true),
                    Original = table.Column<bool>(type: "INTEGER", nullable: true),
                    PlexId = table.Column<long>(type: "INTEGER", nullable: false),
                    Profile = table.Column<string>(type: "TEXT", nullable: true),
                    RefFrames = table.Column<int>(type: "INTEGER", nullable: true),
                    SamplingRate = table.Column<int>(type: "INTEGER", nullable: true),
                    ScanType = table.Column<string>(type: "TEXT", nullable: true),
                    Selected = table.Column<bool>(type: "INTEGER", nullable: true),
                    StreamType = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Width = table.Column<int>(type: "INTEGER", nullable: true),
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
    }
}
