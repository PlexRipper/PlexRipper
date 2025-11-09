using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddedDownloadClientSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadClientSessions",
                columns: table => new
                {
                    Sid = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadClientSessions", x => x.Sid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadClientSessions");
        }
    }
}
