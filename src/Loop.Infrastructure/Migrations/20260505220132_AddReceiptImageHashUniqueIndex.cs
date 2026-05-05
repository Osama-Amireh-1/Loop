using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loop.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddReceiptImageHashUniqueIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "image_hash",
            schema: "public",
            table: "receipt",
            type: "character varying(64)",
            maxLength: 64,
            nullable: true);

        migrationBuilder.Sql("UPDATE public.receipt SET image_hash = md5(receipt_id::text) || md5(receipt_id::text) WHERE image_hash IS NULL;");

        migrationBuilder.UpdateData(
            schema: "public",
            table: "receipt",
            keyColumn: "receipt_id",
            keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
            column: "image_hash",
            value: "0000000000000000000000000000000000000000000000000000000000000000");

        migrationBuilder.AlterColumn<string>(
            name: "image_hash",
            schema: "public",
            table: "receipt",
            type: "character varying(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(64)",
            oldMaxLength: 64,
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_receipt_image_hash",
            schema: "public",
            table: "receipt",
            column: "image_hash",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_receipt_image_hash",
            schema: "public",
            table: "receipt");

        migrationBuilder.DropColumn(
            name: "image_hash",
            schema: "public",
            table: "receipt");
    }
}
