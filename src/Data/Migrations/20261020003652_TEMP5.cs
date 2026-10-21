using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class TEMP5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskPhotoImageFileLogs_DownloadTaskPhotoAlbums_DownloadTaskPhotoAlbumId",
                table: "DownloadTaskPhotoImageFileLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskPhotoImageFileLogs_DownloadTaskPhotoImages_DownloadTaskPhotoId",
                table: "DownloadTaskPhotoImageFileLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskPhotoImageFiles_DownloadTaskPhotoImages_ParentId",
                table: "DownloadTaskPhotoImageFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexPhotoData_PlexLibraries_PlexLibraryId",
                table: "PlexPhotoData");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexPhotoData_PlexPhotoImages_PlexPhotoId",
                table: "PlexPhotoData");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexPhotoData_PlexServers_PlexServerId",
                table: "PlexPhotoData");

            migrationBuilder.DropTable(
                name: "DownloadTaskPhotoImages");

            migrationBuilder.DropTable(
                name: "PlexPhotoImages");

            migrationBuilder.DropTable(
                name: "DownloadTaskPhotoAlbums");

            migrationBuilder.DropIndex(
                name: "IX_PlexPhotoAlbums_PlexLibraryId_SortIndex",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskPhotoImageFileLogs_DownloadTaskPhotoAlbumId",
                table: "DownloadTaskPhotoImageFileLogs");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTaskPhotoImageFileLogs_DownloadTaskPhotoId",
                table: "DownloadTaskPhotoImageFileLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlexPhotoData",
                table: "PlexPhotoData");

            migrationBuilder.DropIndex(
                name: "IX_PlexPhotoData_PlexApiRatingKey",
                table: "PlexPhotoData");

            migrationBuilder.DropIndex(
                name: "IX_PlexPhotoData_PlexPhotoId",
                table: "PlexPhotoData");

            migrationBuilder.DropColumn(
                name: "ChildCount",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "ContentRating",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "FullTitle",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "Guid_IMDB",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "Guid_TMDB",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "Guid_TVDB",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "HasArt",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "HasTheme",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "MediaSize",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "OriginallyAvailableAt",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "SortIndex",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "Studio",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "VideoQuality",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropColumn(
                name: "DownloadTaskPhotoAlbumId",
                table: "DownloadTaskPhotoImageFileLogs");

            migrationBuilder.DropColumn(
                name: "DownloadTaskPhotoId",
                table: "DownloadTaskPhotoImageFileLogs");

            migrationBuilder.DropColumn(
                name: "NeedsGeneratedName",
                table: "PlexPhotoData");

            migrationBuilder.RenameTable(
                name: "PlexPhotoData",
                newName: "PlexPhotoMediaData");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "PlexPhotoMediaData",
                newName: "PlexApiMetaDataKey");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "PlexPhotoMediaData",
                newName: "HasThumb");

            migrationBuilder.RenameColumn(
                name: "PlexPhotoId",
                table: "PlexPhotoMediaData",
                newName: "AssetKind");

            migrationBuilder.RenameColumn(
                name: "GeneratedNameSyncedAt",
                table: "PlexPhotoMediaData",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "GeneratedFilename",
                table: "PlexPhotoMediaData",
                newName: "Summary");

            migrationBuilder.RenameIndex(
                name: "IX_PlexPhotoData_PlexServerId",
                table: "PlexPhotoMediaData",
                newName: "IX_PlexPhotoMediaData_PlexServerId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexPhotoData_PlexLibraryId_PlexApiPartId",
                table: "PlexPhotoMediaData",
                newName: "IX_PlexPhotoMediaData_PlexLibraryId_PlexApiPartId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: false,
                collation: "NATURALSORT",
                oldClrType: typeof(string),
                oldType: "TEXT")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<string>(
                name: "SearchTitle",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldCollation: "NOCASE")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiRatingKey",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiMetaDataKey",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<bool>(
                name: "HasThumb",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 18);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AddedAt",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder.AddColumn<int>(
                name: "AssetKind",
                table: "DownloadTaskPhotoImageFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "VideoCodec",
                table: "PlexPhotoMediaData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "PlexPhotoMediaData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiRatingKey",
                table: "PlexPhotoMediaData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiPartId",
                table: "PlexPhotoMediaData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiMediaId",
                table: "PlexPhotoMediaData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFilename",
                table: "PlexPhotoMediaData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PlexPhotoMediaData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "PlexPhotoMediaData",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<string>(
                name: "Container",
                table: "PlexPhotoMediaData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<string>(
                name: "AudioCodec",
                table: "PlexPhotoMediaData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<bool>(
                name: "HasThumb",
                table: "PlexPhotoMediaData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PlexPhotoMediaData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "PlexPhotoMediaData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedAt",
                table: "PlexPhotoMediaData",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "OriginallyAvailableAt",
                table: "PlexPhotoMediaData",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlexPhotoFolderId",
                table: "PlexPhotoMediaData",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchTitle",
                table: "PlexPhotoMediaData",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                collation: "NOCASE");

            migrationBuilder.AddColumn<string>(
                name: "SourcePath",
                table: "PlexPhotoMediaData",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "PlexPhotoMediaData",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                collation: "NATURALSORT");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexPhotoMediaData",
                table: "PlexPhotoMediaData",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DownloadTaskPhotoFolders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlexApiRatingKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    DownloadStatus = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    SonarrIntegrationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    RadarrIntegrationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RelativePath = table.Column<string>(type: "TEXT", nullable: false, collation: "BINARY"),
                    DownloadRootPath = table.Column<string>(type: "TEXT", nullable: false, collation: "BINARY"),
                    DestinationRootPath = table.Column<string>(type: "TEXT", nullable: false, collation: "BINARY")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTaskPhotoFolders", x => x.Id);
                    table.CheckConstraint("CK_DownloadTaskPhotoFolders_RootShape", "(\"ParentId\" IS NULL AND \"RelativePath\" = '')\nOR\n(\"ParentId\" IS NOT NULL AND \"RelativePath\" <> '')");
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoFolders_DownloadTaskPhotoFolders_ParentId",
                        column: x => x.ParentId,
                        principalTable: "DownloadTaskPhotoFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoFolders_IntegrationsRadarr_RadarrIntegrationId",
                        column: x => x.RadarrIntegrationId,
                        principalTable: "IntegrationsRadarr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoFolders_IntegrationsSonarr_SonarrIntegrationId",
                        column: x => x.SonarrIntegrationId,
                        principalTable: "IntegrationsSonarr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoFolders_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoFolders_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexPhotoAlbumMemberships",
                columns: table => new
                {
                    PlexPhotoAlbumId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexPhotoMediaDataId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexPhotoAlbumMemberships", x => new { x.PlexPhotoAlbumId, x.PlexPhotoMediaDataId });
                    table.ForeignKey(
                        name: "FK_PlexPhotoAlbumMemberships_PlexPhotoAlbums_PlexPhotoAlbumId",
                        column: x => x.PlexPhotoAlbumId,
                        principalTable: "PlexPhotoAlbums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexPhotoAlbumMemberships_PlexPhotoMediaData_PlexPhotoMediaDataId",
                        column: x => x.PlexPhotoMediaDataId,
                        principalTable: "PlexPhotoMediaData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexPhotoFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    RelativePath = table.Column<string>(type: "TEXT", nullable: false, collation: "BINARY"),
                    Title = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    RootIdentityKey = table.Column<string>(type: "TEXT", nullable: true, collation: "BINARY"),
                    SourceRootPath = table.Column<string>(type: "TEXT", nullable: true, collation: "BINARY"),
                    PlexApiLocationId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexPhotoFolders", x => x.Id);
                    table.CheckConstraint("CK_PlexPhotoFolders_RootShape", "(\n    \"ParentId\" IS NULL\n    AND \"RelativePath\" = ''\n    AND \"RootIdentityKey\" IS NOT NULL\n    AND \"RootIdentityKey\" <> ''\n    AND \"SourceRootPath\" IS NOT NULL\n    AND \"SourceRootPath\" <> ''\n    AND \"IsActive\" IS NOT NULL\n)\nOR\n(\n    \"ParentId\" IS NOT NULL\n    AND \"RelativePath\" <> ''\n    AND \"RootIdentityKey\" IS NULL\n    AND \"SourceRootPath\" IS NULL\n    AND \"PlexApiLocationId\" IS NULL\n    AND \"IsActive\" IS NULL\n)");
                    table.ForeignKey(
                        name: "FK_PlexPhotoFolders_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexPhotoFolders_PlexPhotoFolders_ParentId",
                        column: x => x.ParentId,
                        principalTable: "PlexPhotoFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexPhotoFolders_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoAlbums_PlexLibraryId_Title_Id",
                table: "PlexPhotoAlbums",
                columns: new[] { "PlexLibraryId", "Title", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoMediaData_PlexLibraryId_PlexApiRatingKey",
                table: "PlexPhotoMediaData",
                columns: new[] { "PlexLibraryId", "PlexApiRatingKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoMediaData_PlexLibraryId_PlexPhotoFolderId_Title_Id",
                table: "PlexPhotoMediaData",
                columns: new[] { "PlexLibraryId", "PlexPhotoFolderId", "Title", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoMediaData_PlexLibraryId_SearchTitle",
                table: "PlexPhotoMediaData",
                columns: new[] { "PlexLibraryId", "SearchTitle" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoMediaData_PlexLibraryId_Title_Id",
                table: "PlexPhotoMediaData",
                columns: new[] { "PlexLibraryId", "Title", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoMediaData_PlexPhotoFolderId",
                table: "PlexPhotoMediaData",
                column: "PlexPhotoFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoFolders_DownloadStatus",
                table: "DownloadTaskPhotoFolders",
                column: "DownloadStatus");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoFolders_ParentId",
                table: "DownloadTaskPhotoFolders",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoFolders_ParentId_RelativePath",
                table: "DownloadTaskPhotoFolders",
                columns: new[] { "ParentId", "RelativePath" },
                unique: true,
                filter: "\"ParentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoFolders_PlexLibraryId",
                table: "DownloadTaskPhotoFolders",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoFolders_PlexLibraryId_PlexServerId_PlexApiRatingKey",
                table: "DownloadTaskPhotoFolders",
                columns: new[] { "PlexLibraryId", "PlexServerId", "PlexApiRatingKey" });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoFolders_PlexServerId",
                table: "DownloadTaskPhotoFolders",
                column: "PlexServerId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoFolders_PlexServerId_PlexLibraryId_DownloadRootPath_DestinationRootPath",
                table: "DownloadTaskPhotoFolders",
                columns: new[] { "PlexServerId", "PlexLibraryId", "DownloadRootPath", "DestinationRootPath" },
                unique: true,
                filter: "\"ParentId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoFolders_RadarrIntegrationId",
                table: "DownloadTaskPhotoFolders",
                column: "RadarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoFolders_SonarrIntegrationId",
                table: "DownloadTaskPhotoFolders",
                column: "SonarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoAlbumMemberships_PlexPhotoMediaDataId_PlexPhotoAlbumId",
                table: "PlexPhotoAlbumMemberships",
                columns: new[] { "PlexPhotoMediaDataId", "PlexPhotoAlbumId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoFolders_ParentId_RelativePath",
                table: "PlexPhotoFolders",
                columns: new[] { "ParentId", "RelativePath" },
                unique: true,
                filter: "\"ParentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoFolders_PlexLibraryId_ParentId_Title_Id",
                table: "PlexPhotoFolders",
                columns: new[] { "PlexLibraryId", "ParentId", "Title", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoFolders_PlexLibraryId_RootIdentityKey",
                table: "PlexPhotoFolders",
                columns: new[] { "PlexLibraryId", "RootIdentityKey" },
                unique: true,
                filter: "\"ParentId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoFolders_PlexServerId",
                table: "PlexPhotoFolders",
                column: "PlexServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskPhotoImageFiles_DownloadTaskPhotoFolders_ParentId",
                table: "DownloadTaskPhotoImageFiles",
                column: "ParentId",
                principalTable: "DownloadTaskPhotoFolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexPhotoMediaData_PlexLibraries_PlexLibraryId",
                table: "PlexPhotoMediaData",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexPhotoMediaData_PlexPhotoFolders_PlexPhotoFolderId",
                table: "PlexPhotoMediaData",
                column: "PlexPhotoFolderId",
                principalTable: "PlexPhotoFolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexPhotoMediaData_PlexServers_PlexServerId",
                table: "PlexPhotoMediaData",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTaskPhotoImageFiles_DownloadTaskPhotoFolders_ParentId",
                table: "DownloadTaskPhotoImageFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexPhotoMediaData_PlexLibraries_PlexLibraryId",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexPhotoMediaData_PlexPhotoFolders_PlexPhotoFolderId",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexPhotoMediaData_PlexServers_PlexServerId",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropTable(
                name: "DownloadTaskPhotoFolders");

            migrationBuilder.DropTable(
                name: "PlexPhotoAlbumMemberships");

            migrationBuilder.DropTable(
                name: "PlexPhotoFolders");

            migrationBuilder.DropIndex(
                name: "IX_PlexPhotoAlbums_PlexLibraryId_Title_Id",
                table: "PlexPhotoAlbums");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlexPhotoMediaData",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropIndex(
                name: "IX_PlexPhotoMediaData_PlexLibraryId_PlexApiRatingKey",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropIndex(
                name: "IX_PlexPhotoMediaData_PlexLibraryId_PlexPhotoFolderId_Title_Id",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropIndex(
                name: "IX_PlexPhotoMediaData_PlexLibraryId_SearchTitle",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropIndex(
                name: "IX_PlexPhotoMediaData_PlexLibraryId_Title_Id",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropIndex(
                name: "IX_PlexPhotoMediaData_PlexPhotoFolderId",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropColumn(
                name: "AssetKind",
                table: "DownloadTaskPhotoImageFiles");

            migrationBuilder.DropColumn(
                name: "AddedAt",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropColumn(
                name: "OriginallyAvailableAt",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropColumn(
                name: "PlexPhotoFolderId",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropColumn(
                name: "SearchTitle",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropColumn(
                name: "SourcePath",
                table: "PlexPhotoMediaData");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "PlexPhotoMediaData");

            migrationBuilder.RenameTable(
                name: "PlexPhotoMediaData",
                newName: "PlexPhotoData");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "PlexPhotoData",
                newName: "GeneratedNameSyncedAt");

            migrationBuilder.RenameColumn(
                name: "Summary",
                table: "PlexPhotoData",
                newName: "GeneratedFilename");

            migrationBuilder.RenameColumn(
                name: "PlexApiMetaDataKey",
                table: "PlexPhotoData",
                newName: "VideoQuality");

            migrationBuilder.RenameColumn(
                name: "HasThumb",
                table: "PlexPhotoData",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "AssetKind",
                table: "PlexPhotoData",
                newName: "PlexPhotoId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexPhotoMediaData_PlexServerId",
                table: "PlexPhotoData",
                newName: "IX_PlexPhotoData_PlexServerId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexPhotoMediaData_PlexLibraryId_PlexApiPartId",
                table: "PlexPhotoData",
                newName: "IX_PlexPhotoData_PlexLibraryId_PlexApiPartId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 15);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldCollation: "NATURALSORT")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<string>(
                name: "SearchTitle",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldCollation: "NOCASE")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiRatingKey",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiMetaDataKey",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<bool>(
                name: "HasThumb",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 18);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AddedAt",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 14);

            migrationBuilder.AddColumn<int>(
                name: "ChildCount",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 13);

            migrationBuilder.AddColumn<string>(
                name: "ContentRating",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 11);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AddColumn<string>(
                name: "FullTitle",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 22);

            migrationBuilder.AddColumn<string>(
                name: "Guid",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 23);

            migrationBuilder.AddColumn<string>(
                name: "Guid_IMDB",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 24);

            migrationBuilder.AddColumn<int>(
                name: "Guid_TMDB",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 25);

            migrationBuilder.AddColumn<int>(
                name: "Guid_TVDB",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 26);

            migrationBuilder.AddColumn<bool>(
                name: "HasArt",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 19);

            migrationBuilder.AddColumn<bool>(
                name: "HasTheme",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 21);

            migrationBuilder.AddColumn<long>(
                name: "MediaSize",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Relational:ColumnOrder", 7);

            migrationBuilder.AddColumn<DateTime>(
                name: "OriginallyAvailableAt",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 16);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "PlexPhotoAlbums",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0)
                .Annotation("Relational:ColumnOrder", 12);

            migrationBuilder.AddColumn<int>(
                name: "SortIndex",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AddColumn<string>(
                name: "Studio",
                table: "PlexPhotoAlbums",
                type: "TEXT",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AddColumn<int>(
                name: "VideoQuality",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "PlexPhotoAlbums",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AddColumn<Guid>(
                name: "DownloadTaskPhotoAlbumId",
                table: "DownloadTaskPhotoImageFileLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DownloadTaskPhotoId",
                table: "DownloadTaskPhotoImageFileLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "VideoCodec",
                table: "PlexPhotoData",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "PlexPhotoData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiRatingKey",
                table: "PlexPhotoData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiPartId",
                table: "PlexPhotoData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiMediaId",
                table: "PlexPhotoData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFilename",
                table: "PlexPhotoData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PlexPhotoData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "PlexPhotoData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<string>(
                name: "Container",
                table: "PlexPhotoData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<string>(
                name: "AudioCodec",
                table: "PlexPhotoData",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<DateTime>(
                name: "GeneratedNameSyncedAt",
                table: "PlexPhotoData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 15);

            migrationBuilder.AlterColumn<string>(
                name: "GeneratedFilename",
                table: "PlexPhotoData",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<int>(
                name: "Source",
                table: "PlexPhotoData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 11);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsGeneratedName",
                table: "PlexPhotoData",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 14);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexPhotoData",
                table: "PlexPhotoData",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DownloadTaskPhotoAlbums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlexApiRatingKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    DownloadStatus = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    SonarrIntegrationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    RadarrIntegrationId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTaskPhotoAlbums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoAlbums_IntegrationsRadarr_RadarrIntegrationId",
                        column: x => x.RadarrIntegrationId,
                        principalTable: "IntegrationsRadarr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoAlbums_IntegrationsSonarr_SonarrIntegrationId",
                        column: x => x.SonarrIntegrationId,
                        principalTable: "IntegrationsSonarr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoAlbums_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoAlbums_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexPhotoImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlexApiRatingKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    SortIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    SearchTitle = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaSize = table.Column<long>(type: "INTEGER", nullable: false),
                    PlexApiMetaDataKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Studio = table.Column<string>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    ContentRating = table.Column<string>(type: "TEXT", nullable: false),
                    Rating = table.Column<double>(type: "REAL", nullable: false),
                    ChildCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OriginallyAvailableAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VideoQuality = table.Column<int>(type: "INTEGER", nullable: false),
                    HasThumb = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasArt = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasTheme = table.Column<bool>(type: "INTEGER", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Guid = table.Column<string>(type: "TEXT", nullable: false),
                    Guid_IMDB = table.Column<string>(type: "TEXT", nullable: true),
                    Guid_TMDB = table.Column<int>(type: "INTEGER", nullable: true),
                    Guid_TVDB = table.Column<int>(type: "INTEGER", nullable: true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexPhotoAlbumId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentKey = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexPhotoImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexPhotoImages_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexPhotoImages_PlexPhotoAlbums_PlexPhotoAlbumId",
                        column: x => x.PlexPhotoAlbumId,
                        principalTable: "PlexPhotoAlbums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexPhotoImages_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DownloadTaskPhotoImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlexApiRatingKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false, collation: "NATURALSORT"),
                    DownloadStatus = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FullTitle = table.Column<string>(type: "TEXT", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    SonarrIntegrationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    RadarrIntegrationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTaskPhotoImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoImages_DownloadTaskPhotoAlbums_ParentId",
                        column: x => x.ParentId,
                        principalTable: "DownloadTaskPhotoAlbums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoImages_IntegrationsRadarr_RadarrIntegrationId",
                        column: x => x.RadarrIntegrationId,
                        principalTable: "IntegrationsRadarr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoImages_IntegrationsSonarr_SonarrIntegrationId",
                        column: x => x.SonarrIntegrationId,
                        principalTable: "IntegrationsSonarr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoImages_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadTaskPhotoImages_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoAlbums_PlexLibraryId_SortIndex",
                table: "PlexPhotoAlbums",
                columns: new[] { "PlexLibraryId", "SortIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoImageFileLogs_DownloadTaskPhotoAlbumId",
                table: "DownloadTaskPhotoImageFileLogs",
                column: "DownloadTaskPhotoAlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoImageFileLogs_DownloadTaskPhotoId",
                table: "DownloadTaskPhotoImageFileLogs",
                column: "DownloadTaskPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoData_PlexApiRatingKey",
                table: "PlexPhotoData",
                column: "PlexApiRatingKey");

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoData_PlexPhotoId",
                table: "PlexPhotoData",
                column: "PlexPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoAlbums_DownloadStatus",
                table: "DownloadTaskPhotoAlbums",
                column: "DownloadStatus");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoAlbums_PlexLibraryId",
                table: "DownloadTaskPhotoAlbums",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoAlbums_PlexLibraryId_PlexServerId_PlexApiRatingKey",
                table: "DownloadTaskPhotoAlbums",
                columns: new[] { "PlexLibraryId", "PlexServerId", "PlexApiRatingKey" });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoAlbums_PlexServerId",
                table: "DownloadTaskPhotoAlbums",
                column: "PlexServerId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoAlbums_RadarrIntegrationId",
                table: "DownloadTaskPhotoAlbums",
                column: "RadarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoAlbums_SonarrIntegrationId",
                table: "DownloadTaskPhotoAlbums",
                column: "SonarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoImages_DownloadStatus",
                table: "DownloadTaskPhotoImages",
                column: "DownloadStatus");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoImages_ParentId",
                table: "DownloadTaskPhotoImages",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoImages_PlexLibraryId",
                table: "DownloadTaskPhotoImages",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoImages_PlexLibraryId_PlexServerId_PlexApiRatingKey",
                table: "DownloadTaskPhotoImages",
                columns: new[] { "PlexLibraryId", "PlexServerId", "PlexApiRatingKey" });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoImages_PlexServerId",
                table: "DownloadTaskPhotoImages",
                column: "PlexServerId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoImages_RadarrIntegrationId",
                table: "DownloadTaskPhotoImages",
                column: "RadarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTaskPhotoImages_SonarrIntegrationId",
                table: "DownloadTaskPhotoImages",
                column: "SonarrIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoImages_PlexLibraryId_PlexApiRatingKey",
                table: "PlexPhotoImages",
                columns: new[] { "PlexLibraryId", "PlexApiRatingKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoImages_PlexPhotoAlbumId_SortIndex",
                table: "PlexPhotoImages",
                columns: new[] { "PlexPhotoAlbumId", "SortIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexPhotoImages_PlexServerId",
                table: "PlexPhotoImages",
                column: "PlexServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskPhotoImageFileLogs_DownloadTaskPhotoAlbums_DownloadTaskPhotoAlbumId",
                table: "DownloadTaskPhotoImageFileLogs",
                column: "DownloadTaskPhotoAlbumId",
                principalTable: "DownloadTaskPhotoAlbums",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskPhotoImageFileLogs_DownloadTaskPhotoImages_DownloadTaskPhotoId",
                table: "DownloadTaskPhotoImageFileLogs",
                column: "DownloadTaskPhotoId",
                principalTable: "DownloadTaskPhotoImages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTaskPhotoImageFiles_DownloadTaskPhotoImages_ParentId",
                table: "DownloadTaskPhotoImageFiles",
                column: "ParentId",
                principalTable: "DownloadTaskPhotoImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexPhotoData_PlexLibraries_PlexLibraryId",
                table: "PlexPhotoData",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexPhotoData_PlexPhotoImages_PlexPhotoId",
                table: "PlexPhotoData",
                column: "PlexPhotoId",
                principalTable: "PlexPhotoImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexPhotoData_PlexServers_PlexServerId",
                table: "PlexPhotoData",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
