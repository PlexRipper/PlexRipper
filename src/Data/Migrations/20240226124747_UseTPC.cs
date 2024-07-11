using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class UseTPC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskMovieFile_DownloadTaskMovieFileId",
                table: "DownloadWorkerTasks"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskTvShowEpisodeFile_DownloadTaskTvShowEpisodeFileId",
                table: "DownloadWorkerTasks"
            );

            migrationBuilder.DropIndex(
                name: "IX_DownloadWorkerTasks_DownloadTaskMovieFileId",
                table: "DownloadWorkerTasks"
            );

            migrationBuilder.DropColumn(name: "DownloadTaskMovieFileId", table: "DownloadWorkerTasks");

            migrationBuilder.RenameColumn(
                name: "DownloadTaskTvShowEpisodeFileId",
                table: "DownloadWorkerTasks",
                newName: "DownloadTaskFileBaseId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_DownloadWorkerTasks_DownloadTaskTvShowEpisodeFileId",
                table: "DownloadWorkerTasks",
                newName: "IX_DownloadWorkerTasks_DownloadTaskFileBaseId"
            );

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "DownloadTaskTvShowSeason",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "DownloadTaskTvShowEpisodeFile",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "DownloadTaskTvShowEpisode",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "DownloadTaskTvShow",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "DownloadTaskMovieFile",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "DownloadTaskMovie",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Sqlite:Autoincrement", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DownloadTaskFileBaseId",
                table: "DownloadWorkerTasks",
                newName: "DownloadTaskTvShowEpisodeFileId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_DownloadWorkerTasks_DownloadTaskFileBaseId",
                table: "DownloadWorkerTasks",
                newName: "IX_DownloadWorkerTasks_DownloadTaskTvShowEpisodeFileId"
            );

            migrationBuilder.AddColumn<int>(
                name: "DownloadTaskMovieFileId",
                table: "DownloadWorkerTasks",
                type: "INTEGER",
                nullable: true
            );

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "DownloadTaskTvShowSeason",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "DownloadTaskTvShowEpisodeFile",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "DownloadTaskTvShowEpisode",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "DownloadTaskTvShow",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "DownloadTaskMovieFile",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "DownloadTaskMovie",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.CreateIndex(
                name: "IX_DownloadWorkerTasks_DownloadTaskMovieFileId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskMovieFileId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskMovieFile_DownloadTaskMovieFileId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskMovieFileId",
                principalTable: "DownloadTaskMovieFile",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadWorkerTasks_DownloadTaskTvShowEpisodeFile_DownloadTaskTvShowEpisodeFileId",
                table: "DownloadWorkerTasks",
                column: "DownloadTaskTvShowEpisodeFileId",
                principalTable: "DownloadTaskTvShowEpisodeFile",
                principalColumn: "Id"
            );
        }
    }
}
