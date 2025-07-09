using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mashkoor.Modules.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.EnsureSchema(
                name: "id");

            migrationBuilder.EnsureSchema(
                name: "i18n");

            migrationBuilder.EnsureSchema(
                name: "settings");

            migrationBuilder.CreateSequence<int>(
                name: "app_user_seq",
                incrementBy: 100);

            migrationBuilder.CreateTable(
                name: "app_user",
                schema: "id",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    registered_on = table.Column<DateTime>(type: "datetime2", nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    firstname = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    lastname = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    image_url = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    preferred_language = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    updated_email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_on = table.Column<DateTime>(type: "datetime2", nullable: true),
                    original_deleted_username = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    user_name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    normalized_user_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "bit", nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    security_stamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    phone_number = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "bit", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "bit", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "bit", nullable: false),
                    access_failed_count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "client_app_info",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    package_name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    hash_string = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    android_store_link = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ios_store_link = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    build = table.Column<int>(type: "int", nullable: false),
                    major = table.Column<int>(type: "int", nullable: false),
                    minor = table.Column<int>(type: "int", nullable: false),
                    revision = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_app_info", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "device_error",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    reported_on = table.Column<DateTime>(type: "datetime2", nullable: false),
                    device_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    username = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    locale = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    silent = table.Column<bool>(type: "bit", nullable: false),
                    source = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    app_version = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    app_state = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    stack_trace = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    info = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    device_info = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_error", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "language",
                schema: "i18n",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_language", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_on = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    contents = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "privacy_policy",
                schema: "settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_privacy_policy", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "push_notification_problem",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    token = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    reported_on = table.Column<DateTime>(type: "datetime2", nullable: false),
                    error_code = table.Column<int>(type: "int", nullable: false),
                    messaging_error_code = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_push_notification_problem", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                schema: "id",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "terms",
                schema: "settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_terms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "app_usage_history",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    opened_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_usage_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_app_usage_history_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "id",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    username = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.id);
                    table.ForeignKey(
                        name: "FK_customer_app_user_id",
                        column: x => x.id,
                        principalSchema: "id",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "device",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    device_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    manufacturer = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    model = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    platform = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    os_version = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    idiom = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    device_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    pns_handle = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    pns_handle_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    pns_handle_last_refreshed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    registered_on = table.Column<DateTime>(type: "datetime2", nullable: false),
                    app = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    app_version = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    last_ping = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device", x => x.id);
                    table.ForeignKey(
                        name: "FK_device_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "id",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_token",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    token = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    revoked = table.Column<DateTime>(type: "datetime2", nullable: true),
                    concurrency_token = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_token_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "id",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_claim",
                schema: "id",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    claim_type = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    claim_value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_claim", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_claim_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "id",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_login",
                schema: "id",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    provider_key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    provider_display_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_login", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "FK_user_login_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "id",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_status_change_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    changed_on = table.Column<DateTime>(type: "datetime2", nullable: false),
                    changed_by_id = table.Column<int>(type: "int", nullable: false),
                    old_status = table.Column<int>(type: "int", nullable: false),
                    new_status = table.Column<int>(type: "int", nullable: false),
                    change_reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_status_change_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_status_change_history_app_user_changed_by_id",
                        column: x => x.changed_by_id,
                        principalSchema: "id",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_status_change_history_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "id",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_token",
                schema: "id",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    login_provider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_token", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "FK_user_token_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "id",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_notification",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    notification_id = table.Column<int>(type: "int", nullable: false),
                    is_read = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_notification", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_notification_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "id",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_notification_notification_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "privacy_policy_translation",
                schema: "i18n",
                columns: table => new
                {
                    entity_id = table.Column<int>(type: "int", nullable: false),
                    language_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_privacy_policy_translation", x => new { x.entity_id, x.language_id });
                    table.ForeignKey(
                        name: "FK_privacy_policy_translation_language_language_id",
                        column: x => x.language_id,
                        principalSchema: "i18n",
                        principalTable: "language",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_privacy_policy_translation_privacy_policy_entity_id",
                        column: x => x.entity_id,
                        principalSchema: "settings",
                        principalTable: "privacy_policy",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_claim",
                schema: "id",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    claim_type = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    claim_value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_claim", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_claim_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "id",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                schema: "id",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_role_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "id",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_role_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "id",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "terms_translation",
                schema: "i18n",
                columns: table => new
                {
                    entity_id = table.Column<int>(type: "int", nullable: false),
                    language_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_terms_translation", x => new { x.entity_id, x.language_id });
                    table.ForeignKey(
                        name: "FK_terms_translation_language_language_id",
                        column: x => x.language_id,
                        principalSchema: "i18n",
                        principalTable: "language",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_terms_translation_terms_entity_id",
                        column: x => x.entity_id,
                        principalSchema: "settings",
                        principalTable: "terms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_usage_history_user_id",
                schema: "dbo",
                table: "app_usage_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "id",
                table: "app_user",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "IX_app_user_is_deleted",
                schema: "id",
                table: "app_user",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_app_user_status",
                schema: "id",
                table: "app_user",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_app_user_user_name",
                schema: "id",
                table: "app_user",
                column: "user_name",
                unique: true,
                filter: "[user_name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "id",
                table: "app_user",
                column: "normalized_user_name",
                unique: true,
                filter: "[normalized_user_name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_client_app_info_android_store_link",
                table: "client_app_info",
                column: "android_store_link",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_client_app_info_hash_string",
                table: "client_app_info",
                column: "hash_string",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_client_app_info_ios_store_link",
                table: "client_app_info",
                column: "ios_store_link",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_client_app_info_package_name",
                table: "client_app_info",
                column: "package_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_username",
                table: "customer",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_device_device_id",
                schema: "dbo",
                table: "device",
                column: "device_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_device_pns_handle",
                schema: "dbo",
                table: "device",
                column: "pns_handle",
                unique: true,
                filter: "[pns_handle] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_device_user_id",
                schema: "dbo",
                table: "device",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_privacy_policy_translation_language_id",
                schema: "i18n",
                table: "privacy_policy_translation",
                column: "language_id");

            migrationBuilder.CreateIndex(
                name: "IX_push_notification_problem_error_code",
                schema: "dbo",
                table: "push_notification_problem",
                column: "error_code");

            migrationBuilder.CreateIndex(
                name: "IX_push_notification_problem_token",
                schema: "dbo",
                table: "push_notification_problem",
                column: "token");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_revoked",
                schema: "dbo",
                table: "refresh_token",
                column: "revoked");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_user_id",
                schema: "dbo",
                table: "refresh_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "id",
                table: "role",
                column: "normalized_name",
                unique: true,
                filter: "[normalized_name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_role_claim_role_id",
                schema: "id",
                table: "role_claim",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_terms_translation_language_id",
                schema: "i18n",
                table: "terms_translation",
                column: "language_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_claim_user_id",
                schema: "id",
                table: "user_claim",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_login_user_id",
                schema: "id",
                table: "user_login",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_notification_notification_id",
                table: "user_notification",
                column: "notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_notification_user_id",
                table: "user_notification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_role_role_id",
                schema: "id",
                table: "user_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_status_change_history_changed_by_id",
                table: "user_status_change_history",
                column: "changed_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_status_change_history_user_id",
                table: "user_status_change_history",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_usage_history",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "client_app_info");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "device",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "device_error",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "privacy_policy_translation",
                schema: "i18n");

            migrationBuilder.DropTable(
                name: "push_notification_problem",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "refresh_token",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "role_claim",
                schema: "id");

            migrationBuilder.DropTable(
                name: "terms_translation",
                schema: "i18n");

            migrationBuilder.DropTable(
                name: "user_claim",
                schema: "id");

            migrationBuilder.DropTable(
                name: "user_login",
                schema: "id");

            migrationBuilder.DropTable(
                name: "user_notification");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "id");

            migrationBuilder.DropTable(
                name: "user_status_change_history");

            migrationBuilder.DropTable(
                name: "user_token",
                schema: "id");

            migrationBuilder.DropTable(
                name: "privacy_policy",
                schema: "settings");

            migrationBuilder.DropTable(
                name: "language",
                schema: "i18n");

            migrationBuilder.DropTable(
                name: "terms",
                schema: "settings");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "role",
                schema: "id");

            migrationBuilder.DropTable(
                name: "app_user",
                schema: "id");

            migrationBuilder.DropSequence(
                name: "app_user_seq");
        }
    }
}
