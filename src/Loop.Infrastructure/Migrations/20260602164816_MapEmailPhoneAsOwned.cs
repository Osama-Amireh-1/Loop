using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loop.Infrastructure.Migrations;

/// <inheritdoc />
public partial class MapEmailPhoneAsOwned : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            schema: "public",
            table: "user",
            keyColumn: "user_id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            schema: "public",
            table: "user",
            columns: new[] { "user_id", "created_at", "email", "first_name", "gender", "last_name", "password_hash", "phone", "profile_image_url", "tier_id" },
            values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DEMO.USER@LOOP.LOCAL", "Demo", "Male", "User", "seeded-password-hash", "+962790000001", "https://cdn.loop.local/users/demo-user.png", new Guid("11111111-1111-1111-1111-111111111111") });
    }
}
