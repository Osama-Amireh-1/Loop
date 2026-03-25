using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddUserEntity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "todo_items",
            schema: "public");

        migrationBuilder.DropPrimaryKey(
            name: "pk_users",
            schema: "public",
            table: "users");

        migrationBuilder.RenameTable(
            name: "users",
            schema: "public",
            newName: "user",
            newSchema: "public");

        migrationBuilder.RenameIndex(
            name: "ix_users_email",
            schema: "public",
            table: "user",
            newName: "ix_user_email");

        migrationBuilder.AddPrimaryKey(
            name: "pk_user",
            schema: "public",
            table: "user",
            column: "id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_user",
            schema: "public",
            table: "user");

        migrationBuilder.RenameTable(
            name: "user",
            schema: "public",
            newName: "users",
            newSchema: "public");

        migrationBuilder.RenameIndex(
            name: "ix_user_email",
            schema: "public",
            table: "users",
            newName: "ix_users_email");

        migrationBuilder.AddPrimaryKey(
            name: "pk_users",
            schema: "public",
            table: "users",
            column: "id");

        migrationBuilder.CreateTable(
            name: "todo_items",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                description = table.Column<string>(type: "text", nullable: false),
                due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                is_completed = table.Column<bool>(type: "boolean", nullable: false),
                labels = table.Column<List<string>>(type: "text[]", nullable: false),
                priority = table.Column<int>(type: "integer", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_todo_items", x => x.id);
                table.ForeignKey(
                    name: "fk_todo_items_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_todo_items_user_id",
            schema: "public",
            table: "todo_items",
            column: "user_id");
    }
}
