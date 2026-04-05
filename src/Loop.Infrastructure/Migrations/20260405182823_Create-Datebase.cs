using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loop.Infrastructure.Migrations;

/// <inheritdoc />
public partial class CreateDatebase : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "public");

        migrationBuilder.CreateTable(
            name: "category",
            schema: "public",
            columns: table => new
            {
                category_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                icon_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                description = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_category", x => x.category_id);
            });

        migrationBuilder.CreateTable(
            name: "mall",
            schema: "public",
            columns: table => new
            {
                mall_id = table.Column<Guid>(type: "uuid", nullable: false),
                system_config_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                logo_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                cover_image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_mall", x => x.mall_id);
            });

        migrationBuilder.CreateTable(
            name: "tier",
            schema: "public",
            columns: table => new
            {
                tier_id = table.Column<Guid>(type: "uuid", nullable: false),
                tier_order = table.Column<int>(type: "integer", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                points_required = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                benefits = table.Column<string>(type: "jsonb", nullable: true),
                icon_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                color_hex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tier", x => x.tier_id);
            });

        migrationBuilder.CreateTable(
            name: "mall_admin",
            schema: "public",
            columns: table => new
            {
                mall_admin_id = table.Column<Guid>(type: "uuid", nullable: false),
                mall_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_mall_admin", x => x.mall_admin_id);
                table.ForeignKey(
                    name: "fk_mall_admin_mall_mall_id",
                    column: x => x.mall_id,
                    principalSchema: "public",
                    principalTable: "mall",
                    principalColumn: "mall_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "shop",
            schema: "public",
            columns: table => new
            {
                shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                mall_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                category_id = table.Column<Guid>(type: "uuid", nullable: false),
                logo_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                cover_image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                bio = table.Column<string>(type: "text", nullable: true),
                website_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                social_links = table.Column<string>(type: "jsonb", nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_shop", x => x.shop_id);
                table.ForeignKey(
                    name: "fk_shop_category_category_id",
                    column: x => x.category_id,
                    principalSchema: "public",
                    principalTable: "category",
                    principalColumn: "category_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_shop_mall_mall_id",
                    column: x => x.mall_id,
                    principalSchema: "public",
                    principalTable: "mall",
                    principalColumn: "mall_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "user",
            schema: "public",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: false),
                gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                profile_image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                tier_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user", x => x.user_id);
                table.ForeignKey(
                    name: "fk_user_tier_tier_id",
                    column: x => x.tier_id,
                    principalSchema: "public",
                    principalTable: "tier",
                    principalColumn: "tier_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "system_config",
            schema: "public",
            columns: table => new
            {
                config_id = table.Column<Guid>(type: "uuid", nullable: false),
                mall_id = table.Column<Guid>(type: "uuid", nullable: false),
                points_to_currency_ratio = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                earn_points_per_currency = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                min_redemption_threshold = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                updated_by_admin_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_system_config", x => x.config_id);
                table.ForeignKey(
                    name: "fk_system_config_mall_admin_updated_by_admin_id",
                    column: x => x.updated_by_admin_id,
                    principalSchema: "public",
                    principalTable: "mall_admin",
                    principalColumn: "mall_admin_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_system_config_mall_mall_id",
                    column: x => x.mall_id,
                    principalSchema: "public",
                    principalTable: "mall",
                    principalColumn: "mall_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "offer",
            schema: "public",
            columns: table => new
            {
                offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "text", nullable: true),
                image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                reward_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                reward_value = table.Column<string>(type: "jsonb", nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_offer", x => x.offer_id);
                table.ForeignKey(
                    name: "fk_offer_shop_shop_id",
                    column: x => x.shop_id,
                    principalSchema: "public",
                    principalTable: "shop",
                    principalColumn: "shop_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "shop_admin",
            schema: "public",
            columns: table => new
            {
                shop_admin_id = table.Column<Guid>(type: "uuid", nullable: false),
                shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_shop_admin", x => x.shop_admin_id);
                table.ForeignKey(
                    name: "fk_shop_admin_shop_shop_id",
                    column: x => x.shop_id,
                    principalSchema: "public",
                    principalTable: "shop",
                    principalColumn: "shop_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "stamp",
            schema: "public",
            columns: table => new
            {
                stamp_id = table.Column<Guid>(type: "uuid", nullable: false),
                shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "text", nullable: true),
                image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                stamp_icon_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                stamps_required = table.Column<int>(type: "integer", nullable: false),
                reward_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_stamp", x => x.stamp_id);
                table.ForeignKey(
                    name: "fk_stamp_shop_shop_id",
                    column: x => x.shop_id,
                    principalSchema: "public",
                    principalTable: "shop",
                    principalColumn: "shop_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "earn_transaction",
            schema: "public",
            columns: table => new
            {
                earn_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                PurchaseAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                PurchaseCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                points_earned = table.Column<int>(type: "integer", nullable: false),
                transaction_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_earn_transaction", x => x.earn_id);
                table.ForeignKey(
                    name: "fk_earn_transaction_shop_shop_id",
                    column: x => x.shop_id,
                    principalSchema: "public",
                    principalTable: "shop",
                    principalColumn: "shop_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_earn_transaction_user_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "user",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "qr_code",
            schema: "public",
            columns: table => new
            {
                qr_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: true),
                shop_id = table.Column<Guid>(type: "uuid", nullable: true),
                qr_code_data = table.Column<string>(type: "jsonb", nullable: false),
                expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_qr_code", x => x.qr_id);
                table.ForeignKey(
                    name: "fk_qr_code_shop_shop_id",
                    column: x => x.shop_id,
                    principalSchema: "public",
                    principalTable: "shop",
                    principalColumn: "shop_id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_qr_code_user_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "user",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "receipt",
            schema: "public",
            columns: table => new
            {
                receipt_id = table.Column<Guid>(type: "uuid", nullable: false),
                receipt_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                receipt_details = table.Column<string>(type: "jsonb", nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_receipt", x => x.receipt_id);
                table.ForeignKey(
                    name: "fk_receipt_shop_shop_id",
                    column: x => x.shop_id,
                    principalSchema: "public",
                    principalTable: "shop",
                    principalColumn: "shop_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_receipt_user_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "user",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "redeem_transaction",
            schema: "public",
            columns: table => new
            {
                redeem_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                points_used = table.Column<int>(type: "integer", nullable: false),
                DiscountAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                DiscountCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                verification_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_redeem_transaction", x => x.redeem_id);
                table.ForeignKey(
                    name: "fk_redeem_transaction_shop_shop_id",
                    column: x => x.shop_id,
                    principalSchema: "public",
                    principalTable: "shop",
                    principalColumn: "shop_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_redeem_transaction_user_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "user",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "user_points_balance",
            schema: "public",
            columns: table => new
            {
                user_points_balance_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                total_points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                lifetime_points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_points_balance", x => x.user_points_balance_id);
                table.ForeignKey(
                    name: "fk_user_points_balance_user_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "user",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "offer_redemption",
            schema: "public",
            columns: table => new
            {
                redemption_id = table.Column<Guid>(type: "uuid", nullable: false),
                offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                redemption_ref = table.Column<Guid>(type: "uuid", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_offer_redemption", x => x.redemption_id);
                table.ForeignKey(
                    name: "fk_offer_redemption_offer_offer_id",
                    column: x => x.offer_id,
                    principalSchema: "public",
                    principalTable: "offer",
                    principalColumn: "offer_id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_offer_redemption_shop_shop_id",
                    column: x => x.shop_id,
                    principalSchema: "public",
                    principalTable: "shop",
                    principalColumn: "shop_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_offer_redemption_user_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "user",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "audit_log",
            schema: "public",
            columns: table => new
            {
                log_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: true),
                shop_id = table.Column<Guid>(type: "uuid", nullable: true),
                shop_admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                mall_admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                admin_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                action_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                points = table.Column<int>(type: "integer", nullable: true),
                metadata = table.Column<string>(type: "jsonb", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_audit_log", x => x.log_id);
                table.ForeignKey(
                    name: "fk_audit_log_mall_admin_mall_admin_id",
                    column: x => x.mall_admin_id,
                    principalSchema: "public",
                    principalTable: "mall_admin",
                    principalColumn: "mall_admin_id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_audit_log_shop_admin_shop_admin_id",
                    column: x => x.shop_admin_id,
                    principalSchema: "public",
                    principalTable: "shop_admin",
                    principalColumn: "shop_admin_id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_audit_log_shop_shop_id",
                    column: x => x.shop_id,
                    principalSchema: "public",
                    principalTable: "shop",
                    principalColumn: "shop_id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_audit_log_user_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "user",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "stamp_transaction",
            schema: "public",
            columns: table => new
            {
                stamp_tx_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                stamp_program_id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                stamps_count = table.Column<int>(type: "integer", nullable: false),
                redemption_ref = table.Column<Guid>(type: "uuid", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_stamp_transaction", x => x.stamp_tx_id);
                table.ForeignKey(
                    name: "fk_stamp_transaction_shop_shop_id",
                    column: x => x.shop_id,
                    principalSchema: "public",
                    principalTable: "shop",
                    principalColumn: "shop_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_stamp_transaction_stamp_stamp_program_id",
                    column: x => x.stamp_program_id,
                    principalSchema: "public",
                    principalTable: "stamp",
                    principalColumn: "stamp_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_stamp_transaction_user_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "user",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "user_stamp_card",
            schema: "public",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                stamp_id = table.Column<Guid>(type: "uuid", nullable: false),
                stamps_counter = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                last_transaction = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_stamp_card", x => new { x.user_id, x.stamp_id });
                table.ForeignKey(
                    name: "fk_user_stamp_card_stamp_stamp_id",
                    column: x => x.stamp_id,
                    principalSchema: "public",
                    principalTable: "stamp",
                    principalColumn: "stamp_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_user_stamp_card_user_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "user",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_audit_log_created_at",
            schema: "public",
            table: "audit_log",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_audit_log_mall_admin_id",
            schema: "public",
            table: "audit_log",
            column: "mall_admin_id");

        migrationBuilder.CreateIndex(
            name: "ix_audit_log_shop_admin_id",
            schema: "public",
            table: "audit_log",
            column: "shop_admin_id");

        migrationBuilder.CreateIndex(
            name: "ix_audit_log_shop_id",
            schema: "public",
            table: "audit_log",
            column: "shop_id");

        migrationBuilder.CreateIndex(
            name: "ix_audit_log_user_id",
            schema: "public",
            table: "audit_log",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_category_name",
            schema: "public",
            table: "category",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_earn_transaction_created_at",
            schema: "public",
            table: "earn_transaction",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_earn_transaction_shop_id",
            schema: "public",
            table: "earn_transaction",
            column: "shop_id");

        migrationBuilder.CreateIndex(
            name: "ix_earn_transaction_user_id",
            schema: "public",
            table: "earn_transaction",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_mall_name",
            schema: "public",
            table: "mall",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_mall_admin_email",
            schema: "public",
            table: "mall_admin",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_mall_admin_mall_id",
            schema: "public",
            table: "mall_admin",
            column: "mall_id");

        migrationBuilder.CreateIndex(
            name: "ix_mall_admin_phone",
            schema: "public",
            table: "mall_admin",
            column: "phone",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_offer_is_active",
            schema: "public",
            table: "offer",
            column: "is_active");

        migrationBuilder.CreateIndex(
            name: "ix_offer_shop_id",
            schema: "public",
            table: "offer",
            column: "shop_id");

        migrationBuilder.CreateIndex(
            name: "ix_offer_start_date",
            schema: "public",
            table: "offer",
            column: "start_date");

        migrationBuilder.CreateIndex(
            name: "ix_offer_redemption_offer_id",
            schema: "public",
            table: "offer_redemption",
            column: "offer_id");

        migrationBuilder.CreateIndex(
            name: "ix_offer_redemption_shop_id",
            schema: "public",
            table: "offer_redemption",
            column: "shop_id");

        migrationBuilder.CreateIndex(
            name: "ix_offer_redemption_user_id",
            schema: "public",
            table: "offer_redemption",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_offer_redemption_user_id_offer_id",
            schema: "public",
            table: "offer_redemption",
            columns: new[] { "user_id", "offer_id" });

        migrationBuilder.CreateIndex(
            name: "ix_qr_code_expires_at",
            schema: "public",
            table: "qr_code",
            column: "expires_at");

        migrationBuilder.CreateIndex(
            name: "ix_qr_code_shop_id",
            schema: "public",
            table: "qr_code",
            column: "shop_id");

        migrationBuilder.CreateIndex(
            name: "ix_qr_code_user_id",
            schema: "public",
            table: "qr_code",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_receipt_shop_id",
            schema: "public",
            table: "receipt",
            column: "shop_id");

        migrationBuilder.CreateIndex(
            name: "ix_receipt_status",
            schema: "public",
            table: "receipt",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_receipt_user_id",
            schema: "public",
            table: "receipt",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_redeem_transaction_shop_id",
            schema: "public",
            table: "redeem_transaction",
            column: "shop_id");

        migrationBuilder.CreateIndex(
            name: "ix_redeem_transaction_status",
            schema: "public",
            table: "redeem_transaction",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_redeem_transaction_user_id",
            schema: "public",
            table: "redeem_transaction",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_redeem_transaction_verification_code",
            schema: "public",
            table: "redeem_transaction",
            column: "verification_code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_shop_category_id",
            schema: "public",
            table: "shop",
            column: "category_id");

        migrationBuilder.CreateIndex(
            name: "ix_shop_mall_id",
            schema: "public",
            table: "shop",
            column: "mall_id");

        migrationBuilder.CreateIndex(
            name: "ix_shop_name",
            schema: "public",
            table: "shop",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_shop_admin_email",
            schema: "public",
            table: "shop_admin",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_shop_admin_phone",
            schema: "public",
            table: "shop_admin",
            column: "phone",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_shop_admin_shop_id",
            schema: "public",
            table: "shop_admin",
            column: "shop_id");

        migrationBuilder.CreateIndex(
            name: "ix_stamp_is_active",
            schema: "public",
            table: "stamp",
            column: "is_active");

        migrationBuilder.CreateIndex(
            name: "ix_stamp_shop_id",
            schema: "public",
            table: "stamp",
            column: "shop_id");

        migrationBuilder.CreateIndex(
            name: "ix_stamp_transaction_shop_id",
            schema: "public",
            table: "stamp_transaction",
            column: "shop_id");

        migrationBuilder.CreateIndex(
            name: "ix_stamp_transaction_stamp_program_id",
            schema: "public",
            table: "stamp_transaction",
            column: "stamp_program_id");

        migrationBuilder.CreateIndex(
            name: "ix_stamp_transaction_user_id",
            schema: "public",
            table: "stamp_transaction",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_system_config_mall_id",
            schema: "public",
            table: "system_config",
            column: "mall_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_system_config_updated_by_admin_id",
            schema: "public",
            table: "system_config",
            column: "updated_by_admin_id");

        migrationBuilder.CreateIndex(
            name: "ix_tier_name",
            schema: "public",
            table: "tier",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_tier_tier_order",
            schema: "public",
            table: "tier",
            column: "tier_order",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_email",
            schema: "public",
            table: "user",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_phone",
            schema: "public",
            table: "user",
            column: "phone",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_tier_id",
            schema: "public",
            table: "user",
            column: "tier_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_points_balance_user_id",
            schema: "public",
            table: "user_points_balance",
            column: "user_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_stamp_card_stamp_id",
            schema: "public",
            table: "user_stamp_card",
            column: "stamp_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_stamp_card_user_id",
            schema: "public",
            table: "user_stamp_card",
            column: "user_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "audit_log",
            schema: "public");

        migrationBuilder.DropTable(
            name: "earn_transaction",
            schema: "public");

        migrationBuilder.DropTable(
            name: "offer_redemption",
            schema: "public");

        migrationBuilder.DropTable(
            name: "qr_code",
            schema: "public");

        migrationBuilder.DropTable(
            name: "receipt",
            schema: "public");

        migrationBuilder.DropTable(
            name: "redeem_transaction",
            schema: "public");

        migrationBuilder.DropTable(
            name: "stamp_transaction",
            schema: "public");

        migrationBuilder.DropTable(
            name: "system_config",
            schema: "public");

        migrationBuilder.DropTable(
            name: "user_points_balance",
            schema: "public");

        migrationBuilder.DropTable(
            name: "user_stamp_card",
            schema: "public");

        migrationBuilder.DropTable(
            name: "shop_admin",
            schema: "public");

        migrationBuilder.DropTable(
            name: "offer",
            schema: "public");

        migrationBuilder.DropTable(
            name: "mall_admin",
            schema: "public");

        migrationBuilder.DropTable(
            name: "stamp",
            schema: "public");

        migrationBuilder.DropTable(
            name: "user",
            schema: "public");

        migrationBuilder.DropTable(
            name: "shop",
            schema: "public");

        migrationBuilder.DropTable(
            name: "tier",
            schema: "public");

        migrationBuilder.DropTable(
            name: "category",
            schema: "public");

        migrationBuilder.DropTable(
            name: "mall",
            schema: "public");
    }
}
