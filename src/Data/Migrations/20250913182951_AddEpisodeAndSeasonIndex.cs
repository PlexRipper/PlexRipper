using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEpisodeAndSeasonIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Guid_TVDB",
                table: "PlexTvShowSeason",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<int>(
                name: "Guid_TMDB",
                table: "PlexTvShowSeason",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "SeasonNumber",
                table: "PlexTvShowSeason",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AlterColumn<int>(
                name: "Guid_TVDB",
                table: "PlexTvShows",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<int>(
                name: "Guid_TMDB",
                table: "PlexTvShows",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<int>(
                name: "Guid_TVDB",
                table: "PlexTvShowEpisodes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<int>(
                name: "Guid_TMDB",
                table: "PlexTvShowEpisodes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "EpisodeNumber",
                table: "PlexTvShowEpisodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AlterColumn<int>(
                name: "Guid_TVDB",
                table: "PlexMovie",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<int>(
                name: "Guid_TMDB",
                table: "PlexMovie",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "SeasonNumber", table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(name: "EpisodeNumber", table: "PlexTvShowEpisodes");

            migrationBuilder.AlterColumn<string>(
                name: "Guid_TVDB",
                table: "PlexTvShowSeason",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Guid_TMDB",
                table: "PlexTvShowSeason",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Guid_TVDB",
                table: "PlexTvShows",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Guid_TMDB",
                table: "PlexTvShows",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Guid_TVDB",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Guid_TMDB",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Guid_TVDB",
                table: "PlexMovie",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Guid_TMDB",
                table: "PlexMovie",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true
            );
        }
    }
}
