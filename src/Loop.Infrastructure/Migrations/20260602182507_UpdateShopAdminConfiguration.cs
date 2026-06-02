using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loop.Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdateShopAdminConfiguration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            schema: "public",
            table: "shop_admin",
            keyColumn: "shop_admin_id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            schema: "public",
            table: "shop_admin",
            columns: new[] { "shop_admin_id", "created_at", "is_active", "name", "password_hash", "phone", "shop_id", "Email" },
            values: new object[] { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Loop Coffee Admin", "seeded-password-hash", "+962790000002", new Guid("7d5dc255-7f80-4f6f-b962-b83f0d0ac001"), "SHOP.ADMIN@LOOP.LOCAL" });
    }
}
