using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class AddedAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "AspNetRoleClaims");

            migrationBuilder.DropTable(
                "AspNetUserClaims");

            migrationBuilder.DropTable(
                "AspNetUserLogins");

            migrationBuilder.DropTable(
                "AspNetUserRoles");

            migrationBuilder.DropTable(
                "AspNetUserTokens");

            migrationBuilder.DropTable(
                "DeviceCodes");

            migrationBuilder.DropTable(
                "PersistedGrants");

            migrationBuilder.DropTable(
                "AspNetRoles");

            migrationBuilder.DropTable(
                "AspNetUsers");

            migrationBuilder.CreateTable(
                "Accounts",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    IsConfirmed = table.Column<bool>(nullable: false),
                    ConfirmedAt = table.Column<DateTime>(nullable: false),
                },
                constraints: table => { table.PrimaryKey("PK_Accounts", x => x.Id); });

            migrationBuilder.CreateTable(
                "PlexAccounts",
                table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uuid = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    JoinedAt = table.Column<DateTime>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    HasPassword = table.Column<bool>(nullable: false),
                    AuthToken = table.Column<string>(nullable: true),
                    AuthenticationToken = table.Column<string>(nullable: true),
                    ConfirmedAt = table.Column<DateTime>(nullable: false),
                    ForumId = table.Column<int>(nullable: false),
                },
                constraints: table => { table.PrimaryKey("PK_PlexAccounts", x => x.Id); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Accounts");

            migrationBuilder.DropTable(
                "PlexAccounts");

            migrationBuilder.CreateTable(
                "AspNetRoles",
                table => new
                {
                    Id = table.Column<string>("TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>("TEXT", nullable: true),
                    Name = table.Column<string>("TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>("TEXT", maxLength: 256, nullable: true),
                },
                constraints: table => { table.PrimaryKey("PK_AspNetRoles", x => x.Id); });

            migrationBuilder.CreateTable(
                "AspNetUsers",
                table => new
                {
                    Id = table.Column<string>("TEXT", nullable: false),
                    AccessFailedCount = table.Column<int>("INTEGER", nullable: false),
                    ConcurrencyStamp = table.Column<string>("TEXT", nullable: true),
                    Email = table.Column<string>("TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>("INTEGER", nullable: false),
                    LockoutEnabled = table.Column<bool>("INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>("TEXT", nullable: true),
                    NormalizedEmail = table.Column<string>("TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>("TEXT", maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>("TEXT", nullable: true),
                    PhoneNumber = table.Column<string>("TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>("INTEGER", nullable: false),
                    SecurityStamp = table.Column<string>("TEXT", nullable: true),
                    TwoFactorEnabled = table.Column<bool>("INTEGER", nullable: false),
                    UserName = table.Column<string>("TEXT", maxLength: 256, nullable: true),
                },
                constraints: table => { table.PrimaryKey("PK_AspNetUsers", x => x.Id); });

            migrationBuilder.CreateTable(
                "DeviceCodes",
                table => new
                {
                    UserCode = table.Column<string>("TEXT", maxLength: 200, nullable: false),
                    ClientId = table.Column<string>("TEXT", maxLength: 200, nullable: false),
                    CreationTime = table.Column<DateTime>("TEXT", nullable: false),
                    Data = table.Column<string>("TEXT", maxLength: 50000, nullable: false),
                    DeviceCode = table.Column<string>("TEXT", maxLength: 200, nullable: false),
                    Expiration = table.Column<DateTime>("TEXT", nullable: false),
                    SubjectId = table.Column<string>("TEXT", maxLength: 200, nullable: true),
                },
                constraints: table => { table.PrimaryKey("PK_DeviceCodes", x => x.UserCode); });

            migrationBuilder.CreateTable(
                "PersistedGrants",
                table => new
                {
                    Key = table.Column<string>("TEXT", maxLength: 200, nullable: false),
                    ClientId = table.Column<string>("TEXT", maxLength: 200, nullable: false),
                    CreationTime = table.Column<DateTime>("TEXT", nullable: false),
                    Data = table.Column<string>("TEXT", maxLength: 50000, nullable: false),
                    Expiration = table.Column<DateTime>("TEXT", nullable: true),
                    SubjectId = table.Column<string>("TEXT", maxLength: 200, nullable: true),
                    Type = table.Column<string>("TEXT", maxLength: 50, nullable: false),
                },
                constraints: table => { table.PrimaryKey("PK_PersistedGrants", x => x.Key); });

            migrationBuilder.CreateTable(
                "AspNetRoleClaims",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClaimType = table.Column<string>("TEXT", nullable: true),
                    ClaimValue = table.Column<string>("TEXT", nullable: true),
                    RoleId = table.Column<string>("TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        x => x.RoleId,
                        "AspNetRoles",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "AspNetUserClaims",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClaimType = table.Column<string>("TEXT", nullable: true),
                    ClaimValue = table.Column<string>("TEXT", nullable: true),
                    UserId = table.Column<string>("TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        "FK_AspNetUserClaims_AspNetUsers_UserId",
                        x => x.UserId,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "AspNetUserLogins",
                table => new
                {
                    LoginProvider = table.Column<string>("TEXT", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>("TEXT", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>("TEXT", nullable: true),
                    UserId = table.Column<string>("TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        "FK_AspNetUserLogins_AspNetUsers_UserId",
                        x => x.UserId,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "AspNetUserRoles",
                table => new
                {
                    UserId = table.Column<string>("TEXT", nullable: false),
                    RoleId = table.Column<string>("TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        x => x.RoleId,
                        "AspNetRoles",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_AspNetUserRoles_AspNetUsers_UserId",
                        x => x.UserId,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "AspNetUserTokens",
                table => new
                {
                    UserId = table.Column<string>("TEXT", nullable: false),
                    LoginProvider = table.Column<string>("TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>("TEXT", maxLength: 128, nullable: false),
                    Value = table.Column<string>("TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        "FK_AspNetUserTokens_AspNetUsers_UserId",
                        x => x.UserId,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_AspNetRoleClaims_RoleId",
                "AspNetRoleClaims",
                "RoleId");

            migrationBuilder.CreateIndex(
                "RoleNameIndex",
                "AspNetRoles",
                "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_AspNetUserClaims_UserId",
                "AspNetUserClaims",
                "UserId");

            migrationBuilder.CreateIndex(
                "IX_AspNetUserLogins_UserId",
                "AspNetUserLogins",
                "UserId");

            migrationBuilder.CreateIndex(
                "IX_AspNetUserRoles_RoleId",
                "AspNetUserRoles",
                "RoleId");

            migrationBuilder.CreateIndex(
                "EmailIndex",
                "AspNetUsers",
                "NormalizedEmail");

            migrationBuilder.CreateIndex(
                "UserNameIndex",
                "AspNetUsers",
                "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_DeviceCodes_DeviceCode",
                "DeviceCodes",
                "DeviceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_DeviceCodes_Expiration",
                "DeviceCodes",
                "Expiration");

            migrationBuilder.CreateIndex(
                "IX_PersistedGrants_Expiration",
                "PersistedGrants",
                "Expiration");

            migrationBuilder.CreateIndex(
                "IX_PersistedGrants_SubjectId_ClientId_Type",
                "PersistedGrants",
                new[] { "SubjectId", "ClientId", "Type" });
        }
    }
}