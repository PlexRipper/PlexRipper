using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestructureMediaMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Exists",
                table: "PlexTvShowEpisodeDataParts",
                newName: "PrimaryAudioChannels"
            );

            migrationBuilder.RenameColumn(
                name: "Accessible",
                table: "PlexTvShowEpisodeDataParts",
                newName: "MaxAudioChannels"
            );

            migrationBuilder.RenameColumn(name: "Exists", table: "PlexMovieDataParts", newName: "PrimaryAudioChannels");

            migrationBuilder.RenameColumn(name: "Accessible", table: "PlexMovieDataParts", newName: "MaxAudioChannels");

            migrationBuilder.AddColumn<string>(
                name: "AudioLanguages",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: true
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

            migrationBuilder.AddColumn<decimal>(
                name: "FrameRate",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m
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
                name: "HasMetadata",
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

            migrationBuilder.AddColumn<string>(
                name: "PrimaryAudioCodec",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "RatingKey",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "ReleaseTitle",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "Resolution",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "SubtitleLanguages",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
            );

            migrationBuilder.AddColumn<int>(
                name: "VideoBitrate",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "VideoCodec",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "AudioLanguages",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: true
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

            migrationBuilder.AddColumn<decimal>(
                name: "FrameRate",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m
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
                name: "HasMetadata",
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

            migrationBuilder.AddColumn<string>(
                name: "PrimaryAudioCodec",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "RatingKey",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "ReleaseTitle",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "Resolution",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "SubtitleLanguages",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
            );

            migrationBuilder.AddColumn<int>(
                name: "VideoBitrate",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "VideoCodec",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AudioLanguages", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "BitDepth", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "Category", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "ColorSpace", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "FrameRate", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "HasAtmos", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "HasForcedSubs", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "HasMetadata", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "HasSdhSubs", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "Height", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "IsDolbyVision", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "IsHdr", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "IsHdr10", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "PrimaryAudioCodec", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "RatingKey", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "ReleaseTitle", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "Resolution", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "Source", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "SubtitleLanguages", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "UpdatedAt", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "VideoBitrate", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "VideoCodec", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "Width", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "AudioLanguages", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "BitDepth", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "Category", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "ColorSpace", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "FrameRate", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "HasAtmos", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "HasForcedSubs", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "HasMetadata", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "HasSdhSubs", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "Height", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "IsDolbyVision", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "IsHdr", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "IsHdr10", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "PrimaryAudioCodec", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "RatingKey", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "ReleaseTitle", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "Resolution", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "Source", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "SubtitleLanguages", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "UpdatedAt", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "VideoBitrate", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "VideoCodec", table: "PlexMovieDataParts");

            migrationBuilder.DropColumn(name: "Width", table: "PlexMovieDataParts");

            migrationBuilder.RenameColumn(
                name: "PrimaryAudioChannels",
                table: "PlexTvShowEpisodeDataParts",
                newName: "Exists"
            );

            migrationBuilder.RenameColumn(
                name: "MaxAudioChannels",
                table: "PlexTvShowEpisodeDataParts",
                newName: "Accessible"
            );

            migrationBuilder.RenameColumn(name: "PrimaryAudioChannels", table: "PlexMovieDataParts", newName: "Exists");

            migrationBuilder.RenameColumn(name: "MaxAudioChannels", table: "PlexMovieDataParts", newName: "Accessible");
        }
    }
}
