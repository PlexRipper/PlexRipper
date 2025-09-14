using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHashIdToDownloadTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashId",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 13);

            migrationBuilder.AddColumn<string>(
                name: "HashId",
                table: "DownloadTaskMovieFile",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 13);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(
                name: "HashId",
                table: "DownloadTaskMovieFile");
        }
    }
}
