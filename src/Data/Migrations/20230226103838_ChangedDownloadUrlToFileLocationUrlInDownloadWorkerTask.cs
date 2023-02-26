using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangedDownloadUrlToFileLocationUrlInDownloadWorkerTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DownloadUrl",
                table: "DownloadWorkerTasks",
                newName: "FileLocationUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileLocationUrl",
                table: "DownloadWorkerTasks",
                newName: "DownloadUrl");
        }
    }
}
