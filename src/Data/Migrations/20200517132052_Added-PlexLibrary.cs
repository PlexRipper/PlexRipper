using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class AddedPlexLibrary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "TodoItems");

            migrationBuilder.DropTable(
                "TodoLists");

            migrationBuilder.CreateTable(
                "PlexLibraries",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SectionId = table.Column<int>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    Key = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    HasAccess = table.Column<bool>(nullable: false),
                    PlexServerId = table.Column<int>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexLibraries", x => x.Id);
                    table.ForeignKey(
                        "FK_PlexLibraries_PlexServers_PlexServerId",
                        x => x.PlexServerId,
                        "PlexServers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_PlexLibraries_PlexServerId",
                "PlexLibraries",
                "PlexServerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "PlexLibraries");

            migrationBuilder.CreateTable(
                "TodoLists",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Colour = table.Column<string>("TEXT", nullable: true),
                    Created = table.Column<DateTime>("TEXT", nullable: false),
                    CreatedBy = table.Column<string>("TEXT", nullable: true),
                    LastModified = table.Column<DateTime>("TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>("TEXT", nullable: true),
                    Title = table.Column<string>("TEXT", maxLength: 200, nullable: false),
                },
                constraints: table => { table.PrimaryKey("PK_TodoLists", x => x.Id); });

            migrationBuilder.CreateTable(
                "TodoItems",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>("TEXT", nullable: false),
                    CreatedBy = table.Column<string>("TEXT", nullable: true),
                    Done = table.Column<bool>("INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>("TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>("TEXT", nullable: true),
                    ListId = table.Column<int>("INTEGER", nullable: false),
                    Note = table.Column<string>("TEXT", nullable: true),
                    Priority = table.Column<int>("INTEGER", nullable: false),
                    Reminder = table.Column<DateTime>("TEXT", nullable: true),
                    Title = table.Column<string>("TEXT", maxLength: 200, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItems", x => x.Id);
                    table.ForeignKey(
                        "FK_TodoItems_TodoLists_ListId",
                        x => x.ListId,
                        "TodoLists",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_TodoItems_ListId",
                "TodoItems",
                "ListId");
        }
    }
}