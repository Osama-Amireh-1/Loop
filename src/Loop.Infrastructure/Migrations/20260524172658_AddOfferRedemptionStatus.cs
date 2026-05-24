using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loop.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddOfferRedemptionStatus : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "redemption_ref",
            schema: "public",
            table: "offer_redemption",
            newName: "qr_id");

        migrationBuilder.AddColumn<DateTime>(
            name: "confirmed_at",
            schema: "public",
            table: "offer_redemption",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "status",
            schema: "public",
            table: "offer_redemption",
            type: "integer",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.CreateIndex(
            name: "ix_offer_redemption_status",
            schema: "public",
            table: "offer_redemption",
            column: "status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_offer_redemption_status",
            schema: "public",
            table: "offer_redemption");

        migrationBuilder.DropColumn(
            name: "confirmed_at",
            schema: "public",
            table: "offer_redemption");

        migrationBuilder.DropColumn(
            name: "status",
            schema: "public",
            table: "offer_redemption");

        migrationBuilder.RenameColumn(
            name: "qr_id",
            schema: "public",
            table: "offer_redemption",
            newName: "redemption_ref");
    }
}
