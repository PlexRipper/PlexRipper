using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplifiedStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowMediaQuality_PlexMediaQuality_PlexMediaQualityId",
                table: "PlexTvShowMediaQuality"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowSeasonMediaQuality_PlexMediaQuality_PlexMediaQualityId",
                table: "PlexTvShowSeasonMediaQuality"
            );

            migrationBuilder.DropTable(name: "PlexMovieMediaQuality");

            migrationBuilder.DropTable(name: "PlexTvShowEpisodeMediaQuality");

            migrationBuilder.DropTable(name: "PlexMediaQuality");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlexTvShowSeasonMediaQuality",
                table: "PlexTvShowSeasonMediaQuality"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_PlexTvShowMediaQuality", table: "PlexTvShowMediaQuality");

            migrationBuilder.RenameColumn(
                name: "PlexMediaQualityId",
                table: "PlexTvShowSeasonMediaQuality",
                newName: "Quality"
            );

            migrationBuilder.RenameColumn(
                name: "PlexMediaQualityId",
                table: "PlexTvShowMediaQuality",
                newName: "Quality"
            );

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexTvShowSeasonId",
                    table: "PlexTvShowSeasonMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexLibraryId",
                    table: "PlexTvShowSeasonMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Quality",
                    table: "PlexTvShowSeasonMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder
                .AddColumn<int>(
                    name: "Id",
                    table: "PlexTvShowSeasonMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Relational:ColumnOrder", 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexTvShowId",
                    table: "PlexTvShowMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexLibraryId",
                    table: "PlexTvShowMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Quality",
                    table: "PlexTvShowMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder
                .AddColumn<int>(
                    name: "Id",
                    table: "PlexTvShowMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Relational:ColumnOrder", 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "Quality",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "Quality",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexTvShowSeasonMediaQuality",
                table: "PlexTvShowSeasonMediaQuality",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexTvShowMediaQuality",
                table: "PlexTvShowMediaQuality",
                column: "Id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowSeasonMediaQuality_PlexLibraryId",
                table: "PlexTvShowSeasonMediaQuality",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowMediaQuality_PlexLibraryId",
                table: "PlexTvShowMediaQuality",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeData_Quality",
                table: "PlexTvShowEpisodeData",
                column: "Quality"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowMediaQuality_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowMediaQuality",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowSeasonMediaQuality_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowSeasonMediaQuality",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowMediaQuality_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowMediaQuality"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowSeasonMediaQuality_PlexLibraries_PlexLibraryId",
                table: "PlexTvShowSeasonMediaQuality"
            );

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlexTvShowSeasonMediaQuality",
                table: "PlexTvShowSeasonMediaQuality"
            );

            migrationBuilder.DropIndex(
                name: "IX_PlexTvShowSeasonMediaQuality_PlexLibraryId",
                table: "PlexTvShowSeasonMediaQuality"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_PlexTvShowMediaQuality", table: "PlexTvShowMediaQuality");

            migrationBuilder.DropIndex(
                name: "IX_PlexTvShowMediaQuality_PlexLibraryId",
                table: "PlexTvShowMediaQuality"
            );

            migrationBuilder.DropIndex(name: "IX_PlexTvShowEpisodeData_Quality", table: "PlexTvShowEpisodeData");

            migrationBuilder.DropColumn(name: "Id", table: "PlexTvShowSeasonMediaQuality");

            migrationBuilder.DropColumn(name: "Id", table: "PlexTvShowMediaQuality");

            migrationBuilder.DropColumn(name: "Quality", table: "PlexTvShowEpisodeData");

            migrationBuilder.DropColumn(name: "Quality", table: "PlexMovieData");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexTvShowSeasonMediaQuality",
                newName: "PlexMediaQualityId"
            );

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexTvShowMediaQuality",
                newName: "PlexMediaQualityId"
            );

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexTvShowSeasonId",
                    table: "PlexTvShowSeasonMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexLibraryId",
                    table: "PlexTvShowSeasonMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexMediaQualityId",
                    table: "PlexTvShowSeasonMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexTvShowId",
                    table: "PlexTvShowMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexLibraryId",
                    table: "PlexTvShowMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder
                .AlterColumn<int>(
                    name: "PlexMediaQualityId",
                    table: "PlexTvShowMediaQuality",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexTvShowSeasonMediaQuality",
                table: "PlexTvShowSeasonMediaQuality",
                columns: new[] { "PlexMediaQualityId", "PlexLibraryId", "PlexTvShowSeasonId" }
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexTvShowMediaQuality",
                table: "PlexTvShowMediaQuality",
                columns: new[] { "PlexMediaQualityId", "PlexLibraryId", "PlexTvShowId" }
            );

            migrationBuilder.CreateTable(
                name: "PlexMediaQuality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Quality = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMediaQuality", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexMovieMediaQuality",
                columns: table => new
                {
                    PlexMediaQualityId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PlexMovieMediaQuality",
                        x => new
                        {
                            x.PlexMediaQualityId,
                            x.PlexLibraryId,
                            x.PlexMovieId,
                        }
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieMediaQuality_PlexMediaQuality_PlexMediaQualityId",
                        column: x => x.PlexMediaQualityId,
                        principalTable: "PlexMediaQuality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieMediaQuality_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowEpisodeMediaQuality",
                columns: table => new
                {
                    PlexMediaQualityId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowEpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PlexTvShowEpisodeMediaQuality",
                        x => new
                        {
                            x.PlexMediaQualityId,
                            x.PlexLibraryId,
                            x.PlexTvShowEpisodeId,
                        }
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeMediaQuality_PlexMediaQuality_PlexMediaQualityId",
                        column: x => x.PlexMediaQualityId,
                        principalTable: "PlexMediaQuality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeMediaQuality_PlexTvShowEpisodes_PlexTvShowEpisodeId",
                        column: x => x.PlexTvShowEpisodeId,
                        principalTable: "PlexTvShowEpisodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMediaQuality_Quality",
                table: "PlexMediaQuality",
                column: "Quality"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieMediaQuality_PlexMovieId",
                table: "PlexMovieMediaQuality",
                column: "PlexMovieId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeMediaQuality_PlexTvShowEpisodeId",
                table: "PlexTvShowEpisodeMediaQuality",
                column: "PlexTvShowEpisodeId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowMediaQuality_PlexMediaQuality_PlexMediaQualityId",
                table: "PlexTvShowMediaQuality",
                column: "PlexMediaQualityId",
                principalTable: "PlexMediaQuality",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowSeasonMediaQuality_PlexMediaQuality_PlexMediaQualityId",
                table: "PlexTvShowSeasonMediaQuality",
                column: "PlexMediaQualityId",
                principalTable: "PlexMediaQuality",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
