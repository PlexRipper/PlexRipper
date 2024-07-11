using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedFileTransferSpeedToDownloadTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<string>(
                    name: "ServerMachineIdentifier",
                    table: "DownloadTasks",
                    type: "TEXT",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "TEXT",
                    oldNullable: true
                )
                .Annotation("Relational:ColumnOrder", 20)
                .OldAnnotation("Relational:ColumnOrder", 19);

            migrationBuilder
                .AddColumn<int>(
                    name: "FileTransferSpeed",
                    table: "DownloadTasks",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Relational:ColumnOrder", 19);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FileTransferSpeed", table: "DownloadTasks");

            migrationBuilder
                .AlterColumn<string>(
                    name: "ServerMachineIdentifier",
                    table: "DownloadTasks",
                    type: "TEXT",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "TEXT",
                    oldNullable: true
                )
                .Annotation("Relational:ColumnOrder", 19)
                .OldAnnotation("Relational:ColumnOrder", 20);
        }
    }
}
