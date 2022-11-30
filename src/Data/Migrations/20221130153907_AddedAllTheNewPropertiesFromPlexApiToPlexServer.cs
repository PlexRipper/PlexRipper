using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    public partial class AddedAllTheNewPropertiesFromPlexApiToPlexServer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Version",
                table: "PlexServers",
                newName: "Provides");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "PlexServers",
                newName: "LastSeenAt");

            migrationBuilder.AlterColumn<bool>(
                name: "ServerFixApplyDNSFix",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 24);

            migrationBuilder.AlterColumn<string>(
                name: "PublicAddress",
                table: "PlexServers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 13)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<long>(
                name: "OwnerId",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "MachineIdentifier",
                table: "PlexServers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 12)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PlexServers",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<string>(
                name: "Provides",
                table: "PlexServers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSeenAt",
                table: "PlexServers",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 11);

            migrationBuilder.AddColumn<string>(
                name: "Device",
                table: "PlexServers",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AddColumn<bool>(
                name: "DnsRebindingProtection",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 22);

            migrationBuilder.AddColumn<bool>(
                name: "Home",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 16);

            migrationBuilder.AddColumn<bool>(
                name: "HttpsRequired",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 20);

            migrationBuilder.AddColumn<bool>(
                name: "NatLoopbackSupported",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 23);

            migrationBuilder.AddColumn<bool>(
                name: "Owned",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 15);

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "PlexServers",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AddColumn<string>(
                name: "PlatformVersion",
                table: "PlexServers",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AddColumn<string>(
                name: "PlexServerOwnerUsername",
                table: "PlexServers",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AddColumn<int>(
                name: "PreferredConnection",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 14);

            migrationBuilder.AddColumn<bool>(
                name: "Presence",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 19);

            migrationBuilder.AddColumn<string>(
                name: "Product",
                table: "PlexServers",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 7);

            migrationBuilder.AddColumn<string>(
                name: "ProductVersion",
                table: "PlexServers",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AddColumn<bool>(
                name: "PublicAddressMatches",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 21);

            migrationBuilder.AddColumn<bool>(
                name: "Relay",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 18);

            migrationBuilder.AddColumn<bool>(
                name: "Synced",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 17);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Device",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "DnsRebindingProtection",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "Home",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "HttpsRequired",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "NatLoopbackSupported",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "Owned",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "PlatformVersion",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "PlexServerOwnerUsername",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "PreferredConnection",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "Presence",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "Product",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "ProductVersion",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "PublicAddressMatches",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "Relay",
                table: "PlexServers");

            migrationBuilder.DropColumn(
                name: "Synced",
                table: "PlexServers");

            migrationBuilder.RenameColumn(
                name: "Provides",
                table: "PlexServers",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "LastSeenAt",
                table: "PlexServers",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<bool>(
                name: "ServerFixApplyDNSFix",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 24);

            migrationBuilder.AlterColumn<string>(
                name: "PublicAddress",
                table: "PlexServers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<long>(
                name: "OwnerId",
                table: "PlexServers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "MachineIdentifier",
                table: "PlexServers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PlexServers",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "PlexServers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PlexServers",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .OldAnnotation("Relational:ColumnOrder", 11);
        }
    }
}
