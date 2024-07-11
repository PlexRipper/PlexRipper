using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class Test2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DataTotal", table: "DownloadTaskTvShowSeason");

            migrationBuilder.DropColumn(name: "DataTotal", table: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropColumn(name: "DataTotal", table: "DownloadTaskTvShow");

            migrationBuilder.DropColumn(name: "DataTotal", table: "DownloadTaskMovie");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AddColumn<long>(
                    name: "DataTotal",
                    table: "DownloadTaskTvShowSeason",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder
                .AddColumn<long>(
                    name: "DataTotal",
                    table: "DownloadTaskTvShowEpisode",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder
                .AddColumn<long>(
                    name: "DataTotal",
                    table: "DownloadTaskTvShow",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder
                .AddColumn<long>(
                    name: "DataTotal",
                    table: "DownloadTaskMovie",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation("Relational:ColumnOrder", 6);
        }
    }
}
