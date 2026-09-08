using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class RequireIntegrationDownloadFolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntegrationsRadarr_FolderPaths_DownloadFolderId",
                table: "IntegrationsRadarr");

            migrationBuilder.DropForeignKey(
                name: "FK_IntegrationsSonarr_FolderPaths_DownloadFolderId",
                table: "IntegrationsSonarr");

            migrationBuilder.AlterColumn<int>(
                name: "DownloadFolderId",
                table: "IntegrationsSonarr",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DownloadFolderId",
                table: "IntegrationsRadarr",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_IntegrationsRadarr_FolderPaths_DownloadFolderId",
                table: "IntegrationsRadarr",
                column: "DownloadFolderId",
                principalTable: "FolderPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IntegrationsSonarr_FolderPaths_DownloadFolderId",
                table: "IntegrationsSonarr",
                column: "DownloadFolderId",
                principalTable: "FolderPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntegrationsRadarr_FolderPaths_DownloadFolderId",
                table: "IntegrationsRadarr");

            migrationBuilder.DropForeignKey(
                name: "FK_IntegrationsSonarr_FolderPaths_DownloadFolderId",
                table: "IntegrationsSonarr");

            migrationBuilder.AlterColumn<int>(
                name: "DownloadFolderId",
                table: "IntegrationsSonarr",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "DownloadFolderId",
                table: "IntegrationsRadarr",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_IntegrationsRadarr_FolderPaths_DownloadFolderId",
                table: "IntegrationsRadarr",
                column: "DownloadFolderId",
                principalTable: "FolderPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_IntegrationsSonarr_FolderPaths_DownloadFolderId",
                table: "IntegrationsSonarr",
                column: "DownloadFolderId",
                principalTable: "FolderPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
