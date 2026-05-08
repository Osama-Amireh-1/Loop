using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loop.Infrastructure.Migrations;

/// <inheritdoc />
public partial class RenameRedemptionRefToQrId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "redemption_ref",
            schema: "public",
            table: "stamp_redemption",
            newName: "qr_id");

        migrationBuilder.CreateIndex(
            name: "ix_stamp_redemption_qr_id",
            schema: "public",
            table: "stamp_redemption",
            column: "qr_id",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "fk_stamp_redemption_qr_code_qr_id",
            schema: "public",
            table: "stamp_redemption",
            column: "qr_id",
            principalSchema: "public",
            principalTable: "qr_code",
            principalColumn: "qr_id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_stamp_redemption_qr_code_qr_id",
            schema: "public",
            table: "stamp_redemption");

        migrationBuilder.DropIndex(
            name: "ix_stamp_redemption_qr_id",
            schema: "public",
            table: "stamp_redemption");

        migrationBuilder.RenameColumn(
            name: "qr_id",
            schema: "public",
            table: "stamp_redemption",
            newName: "redemption_ref");
    }
}
