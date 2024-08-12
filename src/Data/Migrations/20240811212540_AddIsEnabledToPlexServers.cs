using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsEnabledToPlexServers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<bool>(
                    name: "Synced",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 18)
                .OldAnnotation("Relational:ColumnOrder", 17);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "ServerFixApplyDNSFix",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 25)
                .OldAnnotation("Relational:ColumnOrder", 24);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "Relay",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 19)
                .OldAnnotation("Relational:ColumnOrder", 18);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "PublicAddressMatches",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 22)
                .OldAnnotation("Relational:ColumnOrder", 21);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "Presence",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 20)
                .OldAnnotation("Relational:ColumnOrder", 19);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "Owned",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 16)
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "NatLoopbackSupported",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 24)
                .OldAnnotation("Relational:ColumnOrder", 23);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "HttpsRequired",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 21)
                .OldAnnotation("Relational:ColumnOrder", 20);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "Home",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 17)
                .OldAnnotation("Relational:ColumnOrder", 16);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "DnsRebindingProtection",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 23)
                .OldAnnotation("Relational:ColumnOrder", 22);

            migrationBuilder
                .AddColumn<bool>(
                    name: "IsEnabled",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: true
                )
                .Annotation("Relational:ColumnOrder", 15);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsEnabled", table: "PlexServers");

            migrationBuilder
                .AlterColumn<bool>(
                    name: "Synced",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 17)
                .OldAnnotation("Relational:ColumnOrder", 18);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "ServerFixApplyDNSFix",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 24)
                .OldAnnotation("Relational:ColumnOrder", 25);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "Relay",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 18)
                .OldAnnotation("Relational:ColumnOrder", 19);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "PublicAddressMatches",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 21)
                .OldAnnotation("Relational:ColumnOrder", 22);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "Presence",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 19)
                .OldAnnotation("Relational:ColumnOrder", 20);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "Owned",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 15)
                .OldAnnotation("Relational:ColumnOrder", 16);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "NatLoopbackSupported",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 23)
                .OldAnnotation("Relational:ColumnOrder", 24);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "HttpsRequired",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 20)
                .OldAnnotation("Relational:ColumnOrder", 21);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "Home",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 16)
                .OldAnnotation("Relational:ColumnOrder", 17);

            migrationBuilder
                .AlterColumn<bool>(
                    name: "DnsRebindingProtection",
                    table: "PlexServers",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(bool),
                    oldType: "INTEGER"
                )
                .Annotation("Relational:ColumnOrder", 22)
                .OldAnnotation("Relational:ColumnOrder", 23);
        }
    }
}
