using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loop.Infrastructure.Migrations;

    /// <inheritdoc />
    public partial class RemoveShopPointsWalletRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove any shop_points_wallet rows that reference missing shops to avoid FK violations
            migrationBuilder.Sql(@"DELETE FROM public.shop_points_wallet
WHERE shop_id NOT IN (SELECT shop_id FROM public.shop);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: destructive cleanup cannot be reliably reversed.
        }
    }

