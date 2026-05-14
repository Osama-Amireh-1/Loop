using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loop.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ConfigureShopAdminEmailPhoneConversions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "email",
            schema: "public",
            table: "shop_admin",
            type: "character varying(256)",
            maxLength: 256,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(256)",
            oldMaxLength: 256,
            oldCollation: "case_insensitive");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "email",
            schema: "public",
            table: "shop_admin",
            type: "character varying(256)",
            maxLength: 256,
            nullable: false,
            collation: "case_insensitive",
            oldClrType: typeof(string),
            oldType: "character varying(256)",
            oldMaxLength: 256);
    }
}
