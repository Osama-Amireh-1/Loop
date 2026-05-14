using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loop.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ConfigureShopAdminEmailAsOwned : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "email",
            schema: "public",
            table: "shop_admin",
            newName: "Email");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Email",
            schema: "public",
            table: "shop_admin",
            newName: "email");
    }
}
