using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace MultiGuide.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "admin_users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    failed_login_count = table.Column<int>(type: "int", nullable: false),
                    locked_until = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_users", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "languages",
                columns: table => new
                {
                    code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_languages", x => x.code);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tour_areas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    center_lat = table.Column<double>(type: "double", nullable: false),
                    center_lng = table.Column<double>(type: "double", nullable: false),
                    radius_m = table.Column<int>(type: "int", nullable: false),
                    price_usd = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    pass_ttl_hours = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_areas", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pois",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    area_id = table.Column<long>(type: "bigint", nullable: false),
                    lat = table.Column<double>(type: "double", nullable: false),
                    lng = table.Column<double>(type: "double", nullable: false),
                    trigger_radius_m = table.Column<int>(type: "int", nullable: false),
                    priority = table.Column<int>(type: "int", nullable: false),
                    image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pois", x => x.id);
                    table.ForeignKey(
                        name: "FK_pois_tour_areas_area_id",
                        column: x => x.area_id,
                        principalTable: "tour_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "visitor_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    area_id = table.Column<long>(type: "bigint", nullable: false),
                    language_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ended_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visitor_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_visitor_sessions_languages_language_code",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_visitor_sessions_tour_areas_area_id",
                        column: x => x.area_id,
                        principalTable: "tour_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "poi_translations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    poi_id = table.Column<long>(type: "bigint", nullable: false),
                    language_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    audio_path = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    audio_duration_sec = table.Column<int>(type: "int", nullable: true),
                    audio_checksum = table.Column<string>(type: "char(64)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poi_translations", x => x.id);
                    table.ForeignKey(
                        name: "FK_poi_translations_languages_language_code",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_poi_translations_pois_poi_id",
                        column: x => x.poi_id,
                        principalTable: "pois",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    session_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "char(3)", nullable: false),
                    provider = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    provider_ref = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    idempotency_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    request_hash = table.Column<string>(type: "char(64)", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_payments_visitor_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "visitor_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "play_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    poi_id = table.Column<long>(type: "bigint", nullable: false),
                    session_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    language_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    listened_sec = table.Column<int>(type: "int", nullable: false),
                    completed = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_play_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_play_logs_languages_language_code",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_play_logs_pois_poi_id",
                        column: x => x.poi_id,
                        principalTable: "pois",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_play_logs_visitor_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "visitor_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "access_passes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    session_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    payment_id = table.Column<long>(type: "bigint", nullable: false),
                    token_hash = table.Column<string>(type: "char(64)", nullable: false),
                    issued_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_passes", x => x.id);
                    table.ForeignKey(
                        name: "FK_access_passes_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_access_passes_visitor_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "visitor_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_access_passes_payment_id",
                table: "access_passes",
                column: "payment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_access_passes_session_id",
                table: "access_passes",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_access_passes_token_hash",
                table: "access_passes",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_email",
                table: "admin_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_session_id_idempotency_key",
                table: "payments",
                columns: new[] { "session_id", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_play_logs_language_code",
                table: "play_logs",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "IX_play_logs_poi_id_started_at",
                table: "play_logs",
                columns: new[] { "poi_id", "started_at" });

            migrationBuilder.CreateIndex(
                name: "IX_play_logs_session_id",
                table: "play_logs",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_poi_translations_language_code",
                table: "poi_translations",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "IX_poi_translations_poi_id_language_code",
                table: "poi_translations",
                columns: new[] { "poi_id", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pois_area_id_is_active",
                table: "pois",
                columns: new[] { "area_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_pois_lat_lng",
                table: "pois",
                columns: new[] { "lat", "lng" });

            migrationBuilder.CreateIndex(
                name: "IX_tour_areas_code",
                table: "tour_areas",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_visitor_sessions_area_id",
                table: "visitor_sessions",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_sessions_language_code",
                table: "visitor_sessions",
                column: "language_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "access_passes");

            migrationBuilder.DropTable(
                name: "admin_users");

            migrationBuilder.DropTable(
                name: "play_logs");

            migrationBuilder.DropTable(
                name: "poi_translations");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "pois");

            migrationBuilder.DropTable(
                name: "visitor_sessions");

            migrationBuilder.DropTable(
                name: "languages");

            migrationBuilder.DropTable(
                name: "tour_areas");
        }
    }
}
