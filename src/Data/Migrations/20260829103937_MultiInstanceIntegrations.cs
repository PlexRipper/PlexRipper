using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class MultiInstanceIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RadarrIntegrationId",
                table: "DownloadTaskTvShowSeason",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AddColumn<Guid>(
                name: "SonarrIntegrationId",
                table: "DownloadTaskTvShowSeason",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AddColumn<Guid>(
                name: "RadarrIntegrationId",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AddColumn<Guid>(
                name: "SonarrIntegrationId",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AddColumn<Guid>(
                name: "RadarrIntegrationId",
                table: "DownloadTaskTvShowEpisode",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AddColumn<Guid>(
                name: "SonarrIntegrationId",
                table: "DownloadTaskTvShowEpisode",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AddColumn<Guid>(
                name: "RadarrIntegrationId",
                table: "DownloadTaskTvShow",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AddColumn<Guid>(
                name: "SonarrIntegrationId",
                table: "DownloadTaskTvShow",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AddColumn<Guid>(
                name: "RadarrIntegrationId",
                table: "DownloadTaskMovieFile",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AddColumn<Guid>(
                name: "SonarrIntegrationId",
                table: "DownloadTaskMovieFile",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AddColumn<Guid>(
                name: "RadarrIntegrationId",
                table: "DownloadTaskMovie",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AddColumn<Guid>(
                name: "SonarrIntegrationId",
                table: "DownloadTaskMovie",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.CreateTable(
                name: "IntegrationsRadarr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    BaseUrl = table.Column<string>(type: "TEXT", nullable: false),
                    RadarrApiKey = table.Column<string>(type: "TEXT", nullable: false),
                    QBittorrentApiKey = table.Column<string>(type: "TEXT", unicode: false, maxLength: 32, nullable: false),
                    TorznabApiKey = table.Column<string>(type: "TEXT", unicode: false, maxLength: 32, nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadFolderId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExternalDownloadClientId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExternalIndexerId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProvisioningState = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationsRadarr", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationsRadarr_FolderPaths_DownloadFolderId",
                        column: x => x.DownloadFolderId,
                        principalTable: "FolderPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationsSonarr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    BaseUrl = table.Column<string>(type: "TEXT", nullable: false),
                    SonarrApiKey = table.Column<string>(type: "TEXT", nullable: false),
                    QBittorrentApiKey = table.Column<string>(type: "TEXT", unicode: false, maxLength: 32, nullable: false),
                    TorznabApiKey = table.Column<string>(type: "TEXT", unicode: false, maxLength: 32, nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadFolderId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExternalDownloadClientId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExternalIndexerId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProvisioningState = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationsSonarr", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationsSonarr_FolderPaths_DownloadFolderId",
                        column: x => x.DownloadFolderId,
                        principalTable: "FolderPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowSeason_RadarrIntegrationId",
                table: "DownloadTaskTvShowSeason",
                column: "RadarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowSeason_SonarrIntegrationId",
                table: "DownloadTaskTvShowSeason",
                column: "SonarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_RadarrIntegrationId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "RadarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_SonarrIntegrationId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "SonarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisode_RadarrIntegrationId",
                table: "DownloadTaskTvShowEpisode",
                column: "RadarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShowEpisode_SonarrIntegrationId",
                table: "DownloadTaskTvShowEpisode",
                column: "SonarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShow_RadarrIntegrationId",
                table: "DownloadTaskTvShow",
                column: "RadarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskTvShow_SonarrIntegrationId",
                table: "DownloadTaskTvShow",
                column: "SonarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFile_RadarrIntegrationId",
                table: "DownloadTaskMovieFile",
                column: "RadarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovieFile_SonarrIntegrationId",
                table: "DownloadTaskMovieFile",
                column: "SonarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovie_RadarrIntegrationId",
                table: "DownloadTaskMovie",
                column: "RadarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskMovie_SonarrIntegrationId",
                table: "DownloadTaskMovie",
                column: "SonarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationsRadarr_BaseUrl",
                table: "IntegrationsRadarr",
                column: "BaseUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationsRadarr_Category",
                table: "IntegrationsRadarr",
                column: "Category",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationsRadarr_DisplayName",
                table: "IntegrationsRadarr",
                column: "DisplayName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationsRadarr_DownloadFolderId",
                table: "IntegrationsRadarr",
                column: "DownloadFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationsSonarr_BaseUrl",
                table: "IntegrationsSonarr",
                column: "BaseUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationsSonarr_Category",
                table: "IntegrationsSonarr",
                column: "Category",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationsSonarr_DisplayName",
                table: "IntegrationsSonarr",
                column: "DisplayName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationsSonarr_DownloadFolderId",
                table: "IntegrationsSonarr",
                column: "DownloadFolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskMovie_IntegrationsRadarr_RadarrIntegrationId",
                table: "DownloadTaskMovie",
                column: "RadarrIntegrationId",
                principalTable: "IntegrationsRadarr",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskMovie_IntegrationsSonarr_SonarrIntegrationId",
                table: "DownloadTaskMovie",
                column: "SonarrIntegrationId",
                principalTable: "IntegrationsSonarr",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskMovieFile_IntegrationsRadarr_RadarrIntegrationId",
                table: "DownloadTaskMovieFile",
                column: "RadarrIntegrationId",
                principalTable: "IntegrationsRadarr",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskMovieFile_IntegrationsSonarr_SonarrIntegrationId",
                table: "DownloadTaskMovieFile",
                column: "SonarrIntegrationId",
                principalTable: "IntegrationsSonarr",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShow_IntegrationsRadarr_RadarrIntegrationId",
                table: "DownloadTaskTvShow",
                column: "RadarrIntegrationId",
                principalTable: "IntegrationsRadarr",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShow_IntegrationsSonarr_SonarrIntegrationId",
                table: "DownloadTaskTvShow",
                column: "SonarrIntegrationId",
                principalTable: "IntegrationsSonarr",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowEpisode_IntegrationsRadarr_RadarrIntegrationId",
                table: "DownloadTaskTvShowEpisode",
                column: "RadarrIntegrationId",
                principalTable: "IntegrationsRadarr",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowEpisode_IntegrationsSonarr_SonarrIntegrationId",
                table: "DownloadTaskTvShowEpisode",
                column: "SonarrIntegrationId",
                principalTable: "IntegrationsSonarr",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_IntegrationsRadarr_RadarrIntegrationId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "RadarrIntegrationId",
                principalTable: "IntegrationsRadarr",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_IntegrationsSonarr_SonarrIntegrationId",
                table: "DownloadTaskTvShowEpisodeFile",
                column: "SonarrIntegrationId",
                principalTable: "IntegrationsSonarr",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowSeason_IntegrationsRadarr_RadarrIntegrationId",
                table: "DownloadTaskTvShowSeason",
                column: "RadarrIntegrationId",
                principalTable: "IntegrationsRadarr",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskTvShowSeason_IntegrationsSonarr_SonarrIntegrationId",
                table: "DownloadTaskTvShowSeason",
                column: "SonarrIntegrationId",
                principalTable: "IntegrationsSonarr",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskMovie_IntegrationsRadarr_RadarrIntegrationId",
                table: "DownloadTaskMovie");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskMovie_IntegrationsSonarr_SonarrIntegrationId",
                table: "DownloadTaskMovie");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskMovieFile_IntegrationsRadarr_RadarrIntegrationId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskMovieFile_IntegrationsSonarr_SonarrIntegrationId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShow_IntegrationsRadarr_RadarrIntegrationId",
                table: "DownloadTaskTvShow");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShow_IntegrationsSonarr_SonarrIntegrationId",
                table: "DownloadTaskTvShow");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowEpisode_IntegrationsRadarr_RadarrIntegrationId",
                table: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowEpisode_IntegrationsSonarr_SonarrIntegrationId",
                table: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_IntegrationsRadarr_RadarrIntegrationId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowEpisodeFile_IntegrationsSonarr_SonarrIntegrationId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowSeason_IntegrationsRadarr_RadarrIntegrationId",
                table: "DownloadTaskTvShowSeason");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskTvShowSeason_IntegrationsSonarr_SonarrIntegrationId",
                table: "DownloadTaskTvShowSeason");

            migrationBuilder.DropTable(
                name: "IntegrationsRadarr");

            migrationBuilder.DropTable(
                name: "IntegrationsSonarr");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowSeason_RadarrIntegrationId",
                table: "DownloadTaskTvShowSeason");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowSeason_SonarrIntegrationId",
                table: "DownloadTaskTvShowSeason");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_RadarrIntegrationId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_SonarrIntegrationId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowEpisode_RadarrIntegrationId",
                table: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShowEpisode_SonarrIntegrationId",
                table: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShow_RadarrIntegrationId",
                table: "DownloadTaskTvShow");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskTvShow_SonarrIntegrationId",
                table: "DownloadTaskTvShow");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskMovieFile_RadarrIntegrationId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskMovieFile_SonarrIntegrationId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskMovie_RadarrIntegrationId",
                table: "DownloadTaskMovie");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskMovie_SonarrIntegrationId",
                table: "DownloadTaskMovie");

            migrationBuilder.DropColumn(
                name: "RadarrIntegrationId",
                table: "DownloadTaskTvShowSeason");

            migrationBuilder.DropColumn(
                name: "SonarrIntegrationId",
                table: "DownloadTaskTvShowSeason");

            migrationBuilder.DropColumn(
                name: "RadarrIntegrationId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(
                name: "SonarrIntegrationId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(
                name: "RadarrIntegrationId",
                table: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropColumn(
                name: "SonarrIntegrationId",
                table: "DownloadTaskTvShowEpisode");

            migrationBuilder.DropColumn(
                name: "RadarrIntegrationId",
                table: "DownloadTaskTvShow");

            migrationBuilder.DropColumn(
                name: "SonarrIntegrationId",
                table: "DownloadTaskTvShow");

            migrationBuilder.DropColumn(
                name: "RadarrIntegrationId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(
                name: "SonarrIntegrationId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(
                name: "RadarrIntegrationId",
                table: "DownloadTaskMovie");

            migrationBuilder.DropColumn(
                name: "SonarrIntegrationId",
                table: "DownloadTaskMovie");
        }
    }
}
