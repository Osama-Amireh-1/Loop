using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loop.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddStampTransactionQrForeignKey : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_stamp_transaction_redemption_ref",
            schema: "public",
            table: "stamp_transaction",
            column: "redemption_ref");

        migrationBuilder.AddForeignKey(
            name: "fk_stamp_transaction_qr_code_redemption_ref",
            schema: "public",
            table: "stamp_transaction",
            column: "redemption_ref",
            principalSchema: "public",
            principalTable: "qr_code",
            principalColumn: "qr_id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_stamp_transaction_qr_code_redemption_ref",
            schema: "public",
            table: "stamp_transaction");

        migrationBuilder.DropIndex(
            name: "ix_stamp_transaction_redemption_ref",
            schema: "public",
            table: "stamp_transaction");
    }
}
