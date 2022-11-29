using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class ChangedTheDownloadTaskEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DataTotal",
                table: "DownloadTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "DownloadTaskType",
                table: "DownloadTasks",
                type: "TEXT",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MediaId",
                table: "DownloadTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "DownloadTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "DownloadTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTasks_ParentId",
                table: "DownloadTasks",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTasks_DownloadTasks_ParentId",
                table: "DownloadTasks",
                column: "ParentId",
                principalTable: "DownloadTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTasks_DownloadTasks_ParentId",
                table: "DownloadTasks");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTasks_ParentId",
                table: "DownloadTasks");

            migrationBuilder.DropColumn(
                name: "DataTotal",
                table: "DownloadTasks");

            migrationBuilder.DropColumn(
                name: "DownloadTaskType",
                table: "DownloadTasks");

            migrationBuilder.DropColumn(
                name: "MediaId",
                table: "DownloadTasks");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "DownloadTasks");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "DownloadTasks");
        }
    }
}
