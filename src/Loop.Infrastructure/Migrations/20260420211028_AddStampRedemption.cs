using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loop.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddStampRedemption : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "stamp_redemption",
            schema: "public",
            columns: table => new
            {
                redemption_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                stamp_id = table.Column<Guid>(type: "uuid", nullable: false),
                redemption_ref = table.Column<Guid>(type: "uuid", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                stamp_id1 = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_stamp_redemption", x => x.redemption_id);
                table.ForeignKey(
                    name: "fk_stamp_redemption_shop_shop_id",
                    column: x => x.shop_id,
                    principalSchema: "public",
                    principalTable: "shop",
                    principalColumn: "shop_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_stamp_redemption_stamp_stamp_id",
                    column: x => x.stamp_id,
                    principalSchema: "public",
                    principalTable: "stamp",
                    principalColumn: "stamp_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_stamp_redemption_stamp_stamp_id1",
                    column: x => x.stamp_id1,
                    principalSchema: "public",
                    principalTable: "stamp",
                    principalColumn: "stamp_id");
                table.ForeignKey(
                    name: "fk_stamp_redemption_user_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "user",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_stamp_redemption_shop_id",
            schema: "public",
            table: "stamp_redemption",
            column: "shop_id");

        migrationBuilder.CreateIndex(
            name: "ix_stamp_redemption_stamp_id",
            schema: "public",
            table: "stamp_redemption",
            column: "stamp_id");

        migrationBuilder.CreateIndex(
            name: "ix_stamp_redemption_stamp_id1",
            schema: "public",
            table: "stamp_redemption",
            column: "stamp_id1");

        migrationBuilder.CreateIndex(
            name: "ix_stamp_redemption_user_id",
            schema: "public",
            table: "stamp_redemption",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_stamp_redemption_user_id_stamp_id",
            schema: "public",
            table: "stamp_redemption",
            columns: new[] { "user_id", "stamp_id" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "stamp_redemption",
            schema: "public");
    }
}
