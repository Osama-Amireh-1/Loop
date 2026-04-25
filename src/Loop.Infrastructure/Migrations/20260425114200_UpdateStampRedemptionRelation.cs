using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loop.Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdateStampRedemptionRelation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_stamp_redemption_stamp_stamp_id1",
            schema: "public",
            table: "stamp_redemption");

        migrationBuilder.DropIndex(
            name: "ix_stamp_redemption_stamp_id1",
            schema: "public",
            table: "stamp_redemption");

        migrationBuilder.DropColumn(
            name: "stamp_id1",
            schema: "public",
            table: "stamp_redemption");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "stamp_id1",
            schema: "public",
            table: "stamp_redemption",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_stamp_redemption_stamp_id1",
            schema: "public",
            table: "stamp_redemption",
            column: "stamp_id1");

        migrationBuilder.AddForeignKey(
            name: "fk_stamp_redemption_stamp_stamp_id1",
            schema: "public",
            table: "stamp_redemption",
            column: "stamp_id1",
            principalSchema: "public",
            principalTable: "stamp",
            principalColumn: "stamp_id");
    }
}
