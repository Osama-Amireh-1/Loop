using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loop.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddShopPointsWalletAndRedeemRatio : Migration
{
    private static readonly Guid SeedShopLoopCoffeeId = Guid.Parse("7d5dc255-7f80-4f6f-b962-b83f0d0ac001");
    private static readonly Guid SeedShopUrbanWearId = Guid.Parse("7d5dc255-7f80-4f6f-b962-b83f0d0ac002");
    private static readonly Guid SeedShopTechZoneId = Guid.Parse("7d5dc255-7f80-4f6f-b962-b83f0d0ac003");

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "applied_points_to_currency_ratio",
            schema: "public",
            table: "redeem_transaction",
            type: "numeric(12,6)",
            precision: 12,
            scale: 6,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.CreateTable(
            name: "shop_points_wallet",
            schema: "public",
            columns: table => new
            {
                shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                points_received = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_shop_points_wallet", x => x.shop_id);
                table.ForeignKey(
                    name: "fk_shop_points_wallet_shop_shop_id",
                    column: x => x.shop_id,
                    principalSchema: "public",
                    principalTable: "shop",
                    principalColumn: "shop_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            schema: "public",
            table: "shop_points_wallet",
            columns: ["shop_id", "points_received", "last_updated"],
            values: new object[] { SeedShopLoopCoffeeId, 0, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });

        migrationBuilder.InsertData(
            schema: "public",
            table: "shop_points_wallet",
            columns: ["shop_id", "points_received", "last_updated"],
            values: new object[] { SeedShopUrbanWearId, 0, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });

        migrationBuilder.InsertData(
            schema: "public",
            table: "shop_points_wallet",
            columns: ["shop_id", "points_received", "last_updated"],
            values: new object[] { SeedShopTechZoneId, 0, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "shop_points_wallet",
            schema: "public");

        migrationBuilder.DropColumn(
            name: "applied_points_to_currency_ratio",
            schema: "public",
            table: "redeem_transaction");
    }
}
