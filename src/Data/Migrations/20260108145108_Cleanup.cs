using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class Cleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PrimaryAudioChannels", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "PrimaryAudioChannels", table: "PlexMovieDataParts");

            migrationBuilder.AlterColumn<int>(
                name: "Source",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT"
            );

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryAudioCodec",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "AudioChannels",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AlterColumn<int>(
                name: "Source",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT"
            );

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryAudioCodec",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "AudioChannels",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AudioChannels", table: "PlexTvShowEpisodeDataParts");

            migrationBuilder.DropColumn(name: "AudioChannels", table: "PlexMovieDataParts");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryAudioCodec",
                table: "PlexTvShowEpisodeDataParts",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT"
            );

            migrationBuilder.AddColumn<int>(
                name: "PrimaryAudioChannels",
                table: "PlexTvShowEpisodeDataParts",
                type: "INTEGER",
                nullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryAudioCodec",
                table: "PlexMovieDataParts",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT"
            );

            migrationBuilder.AddColumn<int>(
                name: "PrimaryAudioChannels",
                table: "PlexMovieDataParts",
                type: "INTEGER",
                nullable: true
            );
        }
    }
}
