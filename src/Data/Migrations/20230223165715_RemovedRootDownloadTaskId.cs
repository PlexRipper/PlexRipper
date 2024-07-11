using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedRootDownloadTaskId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTasks_DownloadTasks_RootDownloadTaskId",
                table: "DownloadTasks"
            );

            migrationBuilder.DropIndex(name: "IX_DownloadTasks_RootDownloadTaskId", table: "DownloadTasks");

            migrationBuilder.AlterColumn<int>(
                name: "RootDownloadTaskId",
                table: "DownloadTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RootDownloadTaskId",
                table: "DownloadTasks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTasks_RootDownloadTaskId",
                table: "DownloadTasks",
                column: "RootDownloadTaskId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTasks_DownloadTasks_RootDownloadTaskId",
                table: "DownloadTasks",
                column: "RootDownloadTaskId",
                principalTable: "DownloadTasks",
                principalColumn: "Id"
            );
        }
    }
}
