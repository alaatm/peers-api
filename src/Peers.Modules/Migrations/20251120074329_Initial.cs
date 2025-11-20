using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Peers.Modules.Migrations
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
                name: "catalog");

            migrationBuilder.EnsureSchema(
                name: "i18n");

            migrationBuilder.EnsureSchema(
                name: "lookup");

            migrationBuilder.EnsureSchema(
                name: "settings");

            migrationBuilder.CreateSequence<int>(
                name: "app_user_seq",
                incrementBy: 100);

            migrationBuilder.CreateSequence<int>(
                name: "listing_seq",
                incrementBy: 100);

            migrationBuilder.CreateTable(
                name: "app_user",
                schema: "id",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    registered_on = table.Column<DateTime>(type: "datetime2", nullable: false),
                    firstname = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    lastname = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
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
                    id = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_language", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lookup_type",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    key = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    constraint_mode = table.Column<int>(type: "int", nullable: false),
                    allow_variant = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lookup_type", x => x.id);
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
                name: "product_type",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    kind = table.Column<int>(type: "int", nullable: false),
                    state = table.Column<int>(type: "int", nullable: false),
                    slug = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    slug_path = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: false),
                    is_selectable = table.Column<bool>(type: "bit", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    parent_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_type", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_type_product_type_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "catalog",
                        principalTable: "product_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
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
                    username = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    secret = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    pin_code_hash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
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
                name: "lookup_option",
                schema: "lookup",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    type_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lookup_option", x => x.id);
                    table.UniqueConstraint("AK_lookup_option_type_id_id", x => new { x.type_id, x.id });
                    table.ForeignKey(
                        name: "FK_lookup_option_lookup_type_type_id",
                        column: x => x.type_id,
                        principalSchema: "lookup",
                        principalTable: "lookup_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "privacy_policy_tr",
                schema: "i18n",
                columns: table => new
                {
                    entity_id = table.Column<int>(type: "int", nullable: false),
                    lang_code = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false),
                    title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_privacy_policy_tr", x => new { x.entity_id, x.lang_code });
                    table.ForeignKey(
                        name: "FK_privacy_policy_tr_language_lang_code",
                        column: x => x.lang_code,
                        principalSchema: "i18n",
                        principalTable: "language",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_privacy_policy_tr_privacy_policy_entity_id",
                        column: x => x.entity_id,
                        principalSchema: "settings",
                        principalTable: "privacy_policy",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attribute_definition",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    kind = table.Column<int>(type: "int", nullable: false),
                    is_required = table.Column<bool>(type: "bit", nullable: false),
                    is_variant = table.Column<bool>(type: "bit", nullable: false),
                    position = table.Column<int>(type: "int", nullable: false),
                    product_type_id = table.Column<int>(type: "int", nullable: false),
                    depends_on_id = table.Column<int>(type: "int", nullable: true),
                    lookup_type_id = table.Column<int>(type: "int", nullable: true),
                    config = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    group_definition_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attribute_definition", x => x.id);
                    table.CheckConstraint("CK_AD_Group_VariantOnly", "[kind] <> 5 OR is_variant = 1");
                    table.CheckConstraint("CK_AD_IsVariant_NumericGroupEnumLookupOnly", "[is_variant] = 0 OR [kind] IN (0,1,5,6,7)");
                    table.CheckConstraint("CK_AD_LookupTypeId_LookupOnly", "(\r\n    ([kind] = 7 AND [lookup_type_id] IS NOT NULL)\r\n    OR\r\n    ([kind] <> 7 AND [lookup_type_id] IS NULL)\r\n)");
                    table.ForeignKey(
                        name: "FK_attribute_definition_attribute_definition_depends_on_id",
                        column: x => x.depends_on_id,
                        principalSchema: "catalog",
                        principalTable: "attribute_definition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_attribute_definition_attribute_definition_group_definition_id",
                        column: x => x.group_definition_id,
                        principalSchema: "catalog",
                        principalTable: "attribute_definition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_attribute_definition_lookup_type_lookup_type_id",
                        column: x => x.lookup_type_id,
                        principalSchema: "lookup",
                        principalTable: "lookup_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_attribute_definition_product_type_product_type_id",
                        column: x => x.product_type_id,
                        principalSchema: "catalog",
                        principalTable: "product_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_type_index",
                schema: "catalog",
                columns: table => new
                {
                    product_type_id = table.Column<int>(type: "int", nullable: false),
                    snapshot = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_type_index", x => x.product_type_id);
                    table.ForeignKey(
                        name: "FK_product_type_index_product_type_product_type_id",
                        column: x => x.product_type_id,
                        principalSchema: "catalog",
                        principalTable: "product_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_type_tr",
                schema: "i18n",
                columns: table => new
                {
                    entity_id = table.Column<int>(type: "int", nullable: false),
                    lang_code = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_type_tr", x => new { x.entity_id, x.lang_code });
                    table.ForeignKey(
                        name: "FK_product_type_tr_language_lang_code",
                        column: x => x.lang_code,
                        principalSchema: "i18n",
                        principalTable: "language",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_type_tr_product_type_entity_id",
                        column: x => x.entity_id,
                        principalSchema: "catalog",
                        principalTable: "product_type",
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
                name: "terms_tr",
                schema: "i18n",
                columns: table => new
                {
                    entity_id = table.Column<int>(type: "int", nullable: false),
                    lang_code = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false),
                    title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_terms_tr", x => new { x.entity_id, x.lang_code });
                    table.ForeignKey(
                        name: "FK_terms_tr_language_lang_code",
                        column: x => x.lang_code,
                        principalSchema: "i18n",
                        principalTable: "language",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_terms_tr_terms_entity_id",
                        column: x => x.entity_id,
                        principalSchema: "settings",
                        principalTable: "terms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_address",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    is_default = table.Column<bool>(type: "bit", nullable: false),
                    apartment_number = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    building_number = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    city = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    country = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    district = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    formatted_address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    governorate = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    location = table.Column<Point>(type: "geography", nullable: false),
                    street = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_address", x => x.id);
                    table.ForeignKey(
                        name: "FK_customer_address_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_file",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    batch_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    media_url = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    approved = table.Column<bool>(type: "bit", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    category = table.Column<int>(type: "int", nullable: false),
                    content_type = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    size_in_bytes = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    thumbnail_id = table.Column<int>(type: "int", nullable: true),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    entity_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_file", x => x.id);
                    table.ForeignKey(
                        name: "FK_media_file_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_media_file_media_file_thumbnail_id",
                        column: x => x.thumbnail_id,
                        principalSchema: "dbo",
                        principalTable: "media_file",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "seller",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seller", x => x.id);
                    table.ForeignKey(
                        name: "FK_seller_customer_id",
                        column: x => x.id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lookup_link",
                schema: "lookup",
                columns: table => new
                {
                    parent_type_id = table.Column<int>(type: "int", nullable: false),
                    parent_option_id = table.Column<int>(type: "int", nullable: false),
                    child_type_id = table.Column<int>(type: "int", nullable: false),
                    child_option_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lookup_link", x => new { x.parent_type_id, x.parent_option_id, x.child_type_id, x.child_option_id });
                    table.ForeignKey(
                        name: "FK_lookup_link_lookup_option_child_type_id_child_option_id",
                        columns: x => new { x.child_type_id, x.child_option_id },
                        principalSchema: "lookup",
                        principalTable: "lookup_option",
                        principalColumns: new[] { "type_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lookup_link_lookup_option_parent_type_id_parent_option_id",
                        columns: x => new { x.parent_type_id, x.parent_option_id },
                        principalSchema: "lookup",
                        principalTable: "lookup_option",
                        principalColumns: new[] { "type_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lookup_link_lookup_type_child_type_id",
                        column: x => x.child_type_id,
                        principalSchema: "lookup",
                        principalTable: "lookup_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lookup_link_lookup_type_parent_type_id",
                        column: x => x.parent_type_id,
                        principalSchema: "lookup",
                        principalTable: "lookup_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lookup_option_tr",
                schema: "i18n",
                columns: table => new
                {
                    entity_id = table.Column<int>(type: "int", nullable: false),
                    lang_code = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lookup_option_tr", x => new { x.entity_id, x.lang_code });
                    table.ForeignKey(
                        name: "FK_lookup_option_tr_language_lang_code",
                        column: x => x.lang_code,
                        principalSchema: "i18n",
                        principalTable: "language",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lookup_option_tr_lookup_option_entity_id",
                        column: x => x.entity_id,
                        principalSchema: "lookup",
                        principalTable: "lookup_option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attribute_definition_tr",
                schema: "i18n",
                columns: table => new
                {
                    entity_id = table.Column<int>(type: "int", nullable: false),
                    lang_code = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    unit = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attribute_definition_tr", x => new { x.entity_id, x.lang_code });
                    table.ForeignKey(
                        name: "FK_attribute_definition_tr_attribute_definition_entity_id",
                        column: x => x.entity_id,
                        principalSchema: "catalog",
                        principalTable: "attribute_definition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_attribute_definition_tr_language_lang_code",
                        column: x => x.lang_code,
                        principalSchema: "i18n",
                        principalTable: "language",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enum_attribute_option",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    position = table.Column<int>(type: "int", nullable: false),
                    enum_attribute_definition_id = table.Column<int>(type: "int", nullable: false),
                    parent_option_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enum_attribute_option", x => x.id);
                    table.ForeignKey(
                        name: "FK_enum_attribute_option_attribute_definition_enum_attribute_definition_id",
                        column: x => x.enum_attribute_definition_id,
                        principalSchema: "catalog",
                        principalTable: "attribute_definition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_enum_attribute_option_enum_attribute_option_parent_option_id",
                        column: x => x.parent_option_id,
                        principalTable: "enum_attribute_option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lookup_allowed",
                schema: "catalog",
                columns: table => new
                {
                    attribute_definition_id = table.Column<int>(type: "int", nullable: false),
                    option_id = table.Column<int>(type: "int", nullable: false),
                    type_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lookup_allowed", x => new { x.attribute_definition_id, x.option_id });
                    table.ForeignKey(
                        name: "FK_lookup_allowed_attribute_definition_attribute_definition_id",
                        column: x => x.attribute_definition_id,
                        principalSchema: "catalog",
                        principalTable: "attribute_definition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lookup_allowed_lookup_option_type_id_option_id",
                        columns: x => new { x.type_id, x.option_id },
                        principalSchema: "lookup",
                        principalTable: "lookup_option",
                        principalColumns: new[] { "type_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cart",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    buyer_id = table.Column<int>(type: "int", nullable: false),
                    seller_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_touched_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cart", x => x.id);
                    table.ForeignKey(
                        name: "FK_cart_customer_buyer_id",
                        column: x => x.buyer_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cart_seller_seller_id",
                        column: x => x.seller_id,
                        principalTable: "seller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    buyer_id = table.Column<int>(type: "int", nullable: false),
                    seller_id = table.Column<int>(type: "int", nullable: false),
                    number = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    placed_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    state = table.Column<int>(type: "int", nullable: false),
                    items_total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    shipping_fee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ready_to_ship_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancellation_reason = table.Column<int>(type: "int", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_customer_buyer_id",
                        column: x => x.buyer_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_seller_seller_id",
                        column: x => x.seller_id,
                        principalTable: "seller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shipping_profile",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    seller_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    origin_location = table.Column<Point>(type: "geography", nullable: false),
                    fsp_max_distance = table.Column<double>(type: "float", nullable: true),
                    fsp_min_order = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    r_base_fee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    r_flat_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    r_kind = table.Column<int>(type: "int", nullable: false),
                    r_min_fee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    r_rate_per_kg = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    r_rate_per_km = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipping_profile", x => x.id);
                    table.ForeignKey(
                        name: "FK_shipping_profile_seller_seller_id",
                        column: x => x.seller_id,
                        principalTable: "seller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enum_attribute_option_tr",
                schema: "i18n",
                columns: table => new
                {
                    entity_id = table.Column<int>(type: "int", nullable: false),
                    lang_code = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enum_attribute_option_tr", x => new { x.entity_id, x.lang_code });
                    table.ForeignKey(
                        name: "FK_enum_attribute_option_tr_enum_attribute_option_entity_id",
                        column: x => x.entity_id,
                        principalTable: "enum_attribute_option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_enum_attribute_option_tr_language_lang_code",
                        column: x => x.lang_code,
                        principalSchema: "i18n",
                        principalTable: "language",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "listing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    seller_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    hashtag = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    base_price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    state = table.Column<int>(type: "int", nullable: false),
                    product_type_id = table.Column<int>(type: "int", nullable: false),
                    shipping_profile_id = table.Column<int>(type: "int", nullable: true),
                    product_type_version = table.Column<int>(type: "int", nullable: false),
                    snapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    fp_method = table.Column<int>(type: "int", nullable: false),
                    fp_non_returnable = table.Column<bool>(type: "bit", nullable: true),
                    fp_outbound_paid_by = table.Column<int>(type: "int", nullable: true),
                    fp_return_paid_by = table.Column<int>(type: "int", nullable: true),
                    fp_oqp_max = table.Column<int>(type: "int", nullable: true),
                    fp_oqp_min = table.Column<int>(type: "int", nullable: true),
                    fp_sa_center = table.Column<Point>(type: "geography", nullable: true),
                    fp_sa_radius = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listing", x => x.id);
                    table.CheckConstraint("CK_Listing_BasePrice_NonNegative", "[base_price] >= 0");
                    table.CheckConstraint("CK_Listing_OrderQtyPolicy", "([fp_oqp_min] IS NULL OR [fp_oqp_min] >= 1)\r\nAND ([fp_oqp_max] IS NULL OR [fp_oqp_max] >= 1)\r\nAND ([fp_oqp_min] IS NULL OR [fp_oqp_max] IS NULL OR [fp_oqp_max] >= [fp_oqp_min])");
                    table.ForeignKey(
                        name: "FK_listing_product_type_product_type_id",
                        column: x => x.product_type_id,
                        principalSchema: "catalog",
                        principalTable: "product_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_listing_seller_seller_id",
                        column: x => x.seller_id,
                        principalTable: "seller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_listing_shipping_profile_shipping_profile_id",
                        column: x => x.shipping_profile_id,
                        principalTable: "shipping_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "listing_attribute",
                columns: table => new
                {
                    listing_id = table.Column<int>(type: "int", nullable: false),
                    attribute_definition_id = table.Column<int>(type: "int", nullable: false),
                    enum_attribute_option_id = table.Column<int>(type: "int", nullable: true),
                    lookup_option_id = table.Column<int>(type: "int", nullable: true),
                    attribute_kind = table.Column<int>(type: "int", nullable: false),
                    value = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listing_attribute", x => new { x.listing_id, x.attribute_definition_id });
                    table.CheckConstraint("CK_LA_OnePayload", "(\r\n    [attribute_kind] IN (0,1,2,3,4) AND [value] IS NOT NULL\r\n    AND [enum_attribute_option_id] IS NULL AND [lookup_option_id] IS NULL\r\n)\r\nOR\r\n(\r\n    [attribute_kind] = 6 AND [enum_attribute_option_id] IS NOT NULL\r\n    AND [value] IS NULL AND [lookup_option_id] IS NULL\r\n)\r\nOR\r\n(\r\n    [attribute_kind] = 7 AND [lookup_option_id] IS NOT NULL\r\n    AND [value] IS NULL AND [enum_attribute_option_id] IS NULL\r\n)");
                    table.CheckConstraint("CK_LA_Position_NonNegative", "[position] >= 0");
                    table.ForeignKey(
                        name: "FK_listing_attribute_attribute_definition_attribute_definition_id",
                        column: x => x.attribute_definition_id,
                        principalSchema: "catalog",
                        principalTable: "attribute_definition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_listing_attribute_enum_attribute_option_enum_attribute_option_id",
                        column: x => x.enum_attribute_option_id,
                        principalTable: "enum_attribute_option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_listing_attribute_listing_listing_id",
                        column: x => x.listing_id,
                        principalTable: "listing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_listing_attribute_lookup_option_lookup_option_id",
                        column: x => x.lookup_option_id,
                        principalSchema: "lookup",
                        principalTable: "lookup_option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "listing_tr",
                schema: "i18n",
                columns: table => new
                {
                    entity_id = table.Column<int>(type: "int", nullable: false),
                    lang_code = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false),
                    title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listing_tr", x => new { x.entity_id, x.lang_code });
                    table.ForeignKey(
                        name: "FK_listing_tr_language_lang_code",
                        column: x => x.lang_code,
                        principalSchema: "i18n",
                        principalTable: "language",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_listing_tr_listing_entity_id",
                        column: x => x.entity_id,
                        principalTable: "listing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "listing_variant",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    listing_id = table.Column<int>(type: "int", nullable: false),
                    variant_key = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    sku_code = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    stock_qty = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    selection_snapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    logi_fragile = table.Column<bool>(type: "bit", nullable: true),
                    logi_hazmat = table.Column<bool>(type: "bit", nullable: true),
                    logi_temperature_control = table.Column<int>(type: "int", nullable: true),
                    logi_weight = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    logi_dim_height = table.Column<double>(type: "float", nullable: true),
                    logi_dim_length = table.Column<double>(type: "float", nullable: true),
                    logi_dim_width = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listing_variant", x => x.id);
                    table.ForeignKey(
                        name: "FK_listing_variant_listing_listing_id",
                        column: x => x.listing_id,
                        principalTable: "listing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cart_line",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cart_id = table.Column<int>(type: "int", nullable: false),
                    listing_id = table.Column<int>(type: "int", nullable: false),
                    variant_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cart_line", x => new { x.cart_id, x.id });
                    table.ForeignKey(
                        name: "FK_cart_line_cart_cart_id",
                        column: x => x.cart_id,
                        principalTable: "cart",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cart_line_listing_listing_id",
                        column: x => x.listing_id,
                        principalTable: "listing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cart_line_listing_variant_variant_id",
                        column: x => x.variant_id,
                        principalTable: "listing_variant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "listing_variant_attribute",
                columns: table => new
                {
                    listing_variant_id = table.Column<int>(type: "int", nullable: false),
                    attribute_definition_id = table.Column<int>(type: "int", nullable: false),
                    attribute_kind = table.Column<int>(type: "int", nullable: false),
                    enum_attribute_option_id = table.Column<int>(type: "int", nullable: true),
                    lookup_option_id = table.Column<int>(type: "int", nullable: true),
                    numeric_value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listing_variant_attribute", x => new { x.listing_variant_id, x.attribute_definition_id });
                    table.CheckConstraint("CK_LVA_Position_NonNegative", "[position] >= 0");
                    table.CheckConstraint("CK_LVA_ValidValueCombination", "([attribute_kind] IN (0,1,6,7))\r\nAND (\r\n    ([attribute_kind] IN (0,1) AND [numeric_value] IS NOT NULL AND [enum_attribute_option_id] IS NULL AND [lookup_option_id] IS NULL)\r\n OR ([attribute_kind] = 6 AND [enum_attribute_option_id] IS NOT NULL AND [numeric_value] IS NULL AND [lookup_option_id] IS NULL)\r\n OR ([attribute_kind] = 7 AND [lookup_option_id] IS NOT NULL AND [numeric_value] IS NULL AND [enum_attribute_option_id] IS NULL)\r\n)                ");
                    table.ForeignKey(
                        name: "FK_listing_variant_attribute_attribute_definition_attribute_definition_id",
                        column: x => x.attribute_definition_id,
                        principalSchema: "catalog",
                        principalTable: "attribute_definition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_listing_variant_attribute_enum_attribute_option_enum_attribute_option_id",
                        column: x => x.enum_attribute_option_id,
                        principalTable: "enum_attribute_option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_listing_variant_attribute_listing_variant_listing_variant_id",
                        column: x => x.listing_variant_id,
                        principalTable: "listing_variant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_listing_variant_attribute_lookup_option_lookup_option_id",
                        column: x => x.lookup_option_id,
                        principalSchema: "lookup",
                        principalTable: "lookup_option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_line",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    listing_id = table.Column<int>(type: "int", nullable: false),
                    variant_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_line", x => new { x.order_id, x.id });
                    table.ForeignKey(
                        name: "FK_order_line_listing_listing_id",
                        column: x => x.listing_id,
                        principalTable: "listing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_line_listing_variant_variant_id",
                        column: x => x.variant_id,
                        principalTable: "listing_variant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_line_order_order_id",
                        column: x => x.order_id,
                        principalTable: "order",
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
                name: "IX_attribute_definition_depends_on_id",
                schema: "catalog",
                table: "attribute_definition",
                column: "depends_on_id");

            migrationBuilder.CreateIndex(
                name: "IX_attribute_definition_group_definition_id",
                schema: "catalog",
                table: "attribute_definition",
                column: "group_definition_id");

            migrationBuilder.CreateIndex(
                name: "IX_attribute_definition_lookup_type_id",
                schema: "catalog",
                table: "attribute_definition",
                column: "lookup_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_attribute_definition_product_type_id_key",
                schema: "catalog",
                table: "attribute_definition",
                columns: new[] { "product_type_id", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attribute_definition_product_type_id_position",
                schema: "catalog",
                table: "attribute_definition",
                columns: new[] { "product_type_id", "position" });

            migrationBuilder.CreateIndex(
                name: "IX_attribute_definition_tr_lang_code",
                schema: "i18n",
                table: "attribute_definition_tr",
                column: "lang_code");

            migrationBuilder.CreateIndex(
                name: "IX_cart_buyer_id_seller_id",
                table: "cart",
                columns: new[] { "buyer_id", "seller_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cart_seller_id",
                table: "cart",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "IX_cart_line_cart_id_variant_id",
                table: "cart_line",
                columns: new[] { "cart_id", "variant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cart_line_listing_id",
                table: "cart_line",
                column: "listing_id");

            migrationBuilder.CreateIndex(
                name: "IX_cart_line_variant_id",
                table: "cart_line",
                column: "variant_id");

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
                name: "IX_customer_address_customer_id",
                table: "customer_address",
                column: "customer_id",
                unique: true,
                filter: "[is_default] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_customer_address_customer_id_name",
                table: "customer_address",
                columns: new[] { "customer_id", "name" },
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
                name: "IX_enum_attribute_option_enum_attribute_definition_id_code",
                table: "enum_attribute_option",
                columns: new[] { "enum_attribute_definition_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enum_attribute_option_enum_attribute_definition_id_position",
                table: "enum_attribute_option",
                columns: new[] { "enum_attribute_definition_id", "position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enum_attribute_option_parent_option_id",
                table: "enum_attribute_option",
                column: "parent_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_enum_attribute_option_tr_lang_code",
                schema: "i18n",
                table: "enum_attribute_option_tr",
                column: "lang_code");

            migrationBuilder.CreateIndex(
                name: "IX_listing_product_type_id",
                table: "listing",
                column: "product_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_listing_seller_id_hashtag",
                table: "listing",
                columns: new[] { "seller_id", "hashtag" },
                unique: true,
                filter: "[hashtag] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_listing_shipping_profile_id",
                table: "listing",
                column: "shipping_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_listing_attribute_attribute_definition_id",
                table: "listing_attribute",
                column: "attribute_definition_id");

            migrationBuilder.CreateIndex(
                name: "IX_listing_attribute_enum_attribute_option_id",
                table: "listing_attribute",
                column: "enum_attribute_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_listing_attribute_listing_id_enum_attribute_option_id",
                table: "listing_attribute",
                columns: new[] { "listing_id", "enum_attribute_option_id" },
                unique: true,
                filter: "[enum_attribute_option_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_listing_attribute_listing_id_lookup_option_id",
                table: "listing_attribute",
                columns: new[] { "listing_id", "lookup_option_id" },
                unique: true,
                filter: "[lookup_option_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_listing_attribute_lookup_option_id",
                table: "listing_attribute",
                column: "lookup_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_listing_tr_lang_code",
                schema: "i18n",
                table: "listing_tr",
                column: "lang_code");

            migrationBuilder.CreateIndex(
                name: "IX_listing_variant_is_active",
                table: "listing_variant",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_listing_variant_listing_id_sku_code",
                table: "listing_variant",
                columns: new[] { "listing_id", "sku_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_listing_variant_listing_id_variant_key",
                table: "listing_variant",
                columns: new[] { "listing_id", "variant_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_listing_variant_attribute_enum_attribute_option_id",
                table: "listing_variant_attribute",
                column: "enum_attribute_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_listing_variant_attribute_lookup_option_id",
                table: "listing_variant_attribute",
                column: "lookup_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_LVA_Enum",
                table: "listing_variant_attribute",
                columns: new[] { "attribute_definition_id", "enum_attribute_option_id" },
                filter: "[enum_attribute_option_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LVA_Lookup",
                table: "listing_variant_attribute",
                columns: new[] { "attribute_definition_id", "lookup_option_id" },
                filter: "[lookup_option_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LVA_Num",
                table: "listing_variant_attribute",
                columns: new[] { "attribute_definition_id", "numeric_value" },
                filter: "[numeric_value] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_lookup_allowed_type_id_option_id",
                schema: "catalog",
                table: "lookup_allowed",
                columns: new[] { "type_id", "option_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lookup_link_child_type_id_child_option_id",
                schema: "lookup",
                table: "lookup_link",
                columns: new[] { "child_type_id", "child_option_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lookup_option_type_id_code",
                schema: "lookup",
                table: "lookup_option",
                columns: new[] { "type_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lookup_option_tr_lang_code",
                schema: "i18n",
                table: "lookup_option_tr",
                column: "lang_code");

            migrationBuilder.CreateIndex(
                name: "IX_lookup_type_key",
                schema: "lookup",
                table: "lookup_type",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_file_approved",
                schema: "dbo",
                table: "media_file",
                column: "approved");

            migrationBuilder.CreateIndex(
                name: "IX_media_file_batch_id",
                schema: "dbo",
                table: "media_file",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_media_file_category",
                schema: "dbo",
                table: "media_file",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_media_file_customer_id",
                schema: "dbo",
                table: "media_file",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_media_file_entity_id",
                schema: "dbo",
                table: "media_file",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_media_file_status",
                schema: "dbo",
                table: "media_file",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_media_file_thumbnail_id",
                schema: "dbo",
                table: "media_file",
                column: "thumbnail_id",
                unique: true,
                filter: "[thumbnail_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_media_file_type",
                schema: "dbo",
                table: "media_file",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_media_file_type_thumbnail_id_customer_id",
                schema: "dbo",
                table: "media_file",
                columns: new[] { "type", "thumbnail_id", "customer_id" },
                unique: true,
                filter: "[type] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_order_buyer_id_seller_id",
                table: "order",
                columns: new[] { "buyer_id", "seller_id" },
                unique: true,
                filter: "[state] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_order_seller_id",
                table: "order",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_line_listing_id",
                table: "order_line",
                column: "listing_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_line_order_id_variant_id",
                table: "order_line",
                columns: new[] { "order_id", "variant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_line_variant_id",
                table: "order_line",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_privacy_policy_tr_lang_code",
                schema: "i18n",
                table: "privacy_policy_tr",
                column: "lang_code");

            migrationBuilder.CreateIndex(
                name: "IX_product_type_is_selectable_parent_id",
                schema: "catalog",
                table: "product_type",
                columns: new[] { "is_selectable", "parent_id" });

            migrationBuilder.CreateIndex(
                name: "IX_product_type_parent_id_slug_version",
                schema: "catalog",
                table: "product_type",
                columns: new[] { "parent_id", "slug", "version" },
                unique: true,
                filter: "[parent_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_product_type_slug_path",
                schema: "catalog",
                table: "product_type",
                column: "slug_path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_type_tr_lang_code",
                schema: "i18n",
                table: "product_type_tr",
                column: "lang_code");

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
                name: "IX_refresh_token_user_id",
                schema: "dbo",
                table: "refresh_token",
                column: "user_id",
                unique: true,
                filter: "[revoked] IS NULL");

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
                name: "IX_shipping_profile_seller_id_name",
                table: "shipping_profile",
                columns: new[] { "seller_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_terms_tr_lang_code",
                schema: "i18n",
                table: "terms_tr",
                column: "lang_code");

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
                name: "attribute_definition_tr",
                schema: "i18n");

            migrationBuilder.DropTable(
                name: "cart_line");

            migrationBuilder.DropTable(
                name: "client_app_info");

            migrationBuilder.DropTable(
                name: "customer_address");

            migrationBuilder.DropTable(
                name: "device",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "device_error",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "enum_attribute_option_tr",
                schema: "i18n");

            migrationBuilder.DropTable(
                name: "listing_attribute");

            migrationBuilder.DropTable(
                name: "listing_tr",
                schema: "i18n");

            migrationBuilder.DropTable(
                name: "listing_variant_attribute");

            migrationBuilder.DropTable(
                name: "lookup_allowed",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "lookup_link",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "lookup_option_tr",
                schema: "i18n");

            migrationBuilder.DropTable(
                name: "media_file",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "order_line");

            migrationBuilder.DropTable(
                name: "privacy_policy_tr",
                schema: "i18n");

            migrationBuilder.DropTable(
                name: "product_type_index",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_type_tr",
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
                name: "terms_tr",
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
                name: "cart");

            migrationBuilder.DropTable(
                name: "enum_attribute_option");

            migrationBuilder.DropTable(
                name: "lookup_option",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "listing_variant");

            migrationBuilder.DropTable(
                name: "order");

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
                name: "attribute_definition",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "listing");

            migrationBuilder.DropTable(
                name: "lookup_type",
                schema: "lookup");

            migrationBuilder.DropTable(
                name: "product_type",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "shipping_profile");

            migrationBuilder.DropTable(
                name: "seller");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "app_user",
                schema: "id");

            migrationBuilder.DropSequence(
                name: "app_user_seq");

            migrationBuilder.DropSequence(
                name: "listing_seq");
        }
    }
}
