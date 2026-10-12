CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;
IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE TABLE `admin_users` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `email` varchar(255) NOT NULL,
        `password_hash` varchar(255) NOT NULL,
        `role` varchar(30) NOT NULL,
        `is_active` tinyint(1) NOT NULL,
        `failed_login_count` int NOT NULL,
        `locked_until` datetime(6) NULL,
        `created_at` datetime(6) NOT NULL,
        PRIMARY KEY (`id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE TABLE `languages` (
        `code` varchar(10) NOT NULL,
        `name` varchar(50) NOT NULL,
        `is_active` tinyint(1) NOT NULL,
        PRIMARY KEY (`code`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE TABLE `tour_areas` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `code` varchar(50) NOT NULL,
        `name` varchar(200) NOT NULL,
        `center_lat` double NOT NULL,
        `center_lng` double NOT NULL,
        `radius_m` int NOT NULL,
        `price_usd` decimal(10,2) NOT NULL,
        `pass_ttl_hours` int NOT NULL,
        `is_active` tinyint(1) NOT NULL,
        PRIMARY KEY (`id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE TABLE `pois` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `area_id` bigint NOT NULL,
        `lat` double NOT NULL,
        `lng` double NOT NULL,
        `trigger_radius_m` int NOT NULL,
        `priority` int NOT NULL,
        `image_url` varchar(500) NULL,
        `is_active` tinyint(1) NOT NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        PRIMARY KEY (`id`),
        CONSTRAINT `FK_pois_tour_areas_area_id` FOREIGN KEY (`area_id`) REFERENCES `tour_areas` (`id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE TABLE `visitor_sessions` (
        `id` char(36) NOT NULL,
        `area_id` bigint NOT NULL,
        `language_code` varchar(10) NOT NULL,
        `started_at` datetime(6) NOT NULL,
        `ended_at` datetime(6) NULL,
        PRIMARY KEY (`id`),
        CONSTRAINT `FK_visitor_sessions_languages_language_code` FOREIGN KEY (`language_code`) REFERENCES `languages` (`code`) ON DELETE RESTRICT,
        CONSTRAINT `FK_visitor_sessions_tour_areas_area_id` FOREIGN KEY (`area_id`) REFERENCES `tour_areas` (`id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE TABLE `poi_translations` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `poi_id` bigint NOT NULL,
        `language_code` varchar(10) NOT NULL,
        `title` varchar(200) NOT NULL,
        `description` text NULL,
        `audio_path` varchar(500) NULL,
        `audio_duration_sec` int NULL,
        `audio_checksum` char(64) NULL,
        `updated_at` datetime(6) NOT NULL,
        PRIMARY KEY (`id`),
        CONSTRAINT `FK_poi_translations_languages_language_code` FOREIGN KEY (`language_code`) REFERENCES `languages` (`code`) ON DELETE RESTRICT,
        CONSTRAINT `FK_poi_translations_pois_poi_id` FOREIGN KEY (`poi_id`) REFERENCES `pois` (`id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE TABLE `payments` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `session_id` char(36) NOT NULL,
        `amount` decimal(10,2) NOT NULL,
        `currency` char(3) NOT NULL,
        `provider` varchar(50) NOT NULL,
        `provider_ref` varchar(100) NULL,
        `idempotency_key` varchar(100) NOT NULL,
        `request_hash` char(64) NOT NULL,
        `status` varchar(20) NOT NULL,
        `created_at` datetime(6) NOT NULL,
        PRIMARY KEY (`id`),
        CONSTRAINT `FK_payments_visitor_sessions_session_id` FOREIGN KEY (`session_id`) REFERENCES `visitor_sessions` (`id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE TABLE `play_logs` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `poi_id` bigint NOT NULL,
        `session_id` char(36) NOT NULL,
        `language_code` varchar(10) NOT NULL,
        `started_at` datetime(6) NOT NULL,
        `listened_sec` int NOT NULL,
        `completed` tinyint(1) NOT NULL,
        PRIMARY KEY (`id`),
        CONSTRAINT `FK_play_logs_languages_language_code` FOREIGN KEY (`language_code`) REFERENCES `languages` (`code`) ON DELETE RESTRICT,
        CONSTRAINT `FK_play_logs_pois_poi_id` FOREIGN KEY (`poi_id`) REFERENCES `pois` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_play_logs_visitor_sessions_session_id` FOREIGN KEY (`session_id`) REFERENCES `visitor_sessions` (`id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE TABLE `access_passes` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `session_id` char(36) NOT NULL,
        `payment_id` bigint NOT NULL,
        `token_hash` char(64) NOT NULL,
        `issued_at` datetime(6) NOT NULL,
        `expires_at` datetime(6) NOT NULL,
        `status` varchar(20) NOT NULL,
        PRIMARY KEY (`id`),
        CONSTRAINT `FK_access_passes_payments_payment_id` FOREIGN KEY (`payment_id`) REFERENCES `payments` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_access_passes_visitor_sessions_session_id` FOREIGN KEY (`session_id`) REFERENCES `visitor_sessions` (`id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX `IX_access_passes_payment_id` ON `access_passes` (`payment_id`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE INDEX `IX_access_passes_session_id` ON `access_passes` (`session_id`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX `IX_access_passes_token_hash` ON `access_passes` (`token_hash`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX `IX_admin_users_email` ON `admin_users` (`email`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX `IX_payments_session_id_idempotency_key` ON `payments` (`session_id`, `idempotency_key`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE INDEX `IX_play_logs_language_code` ON `play_logs` (`language_code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE INDEX `IX_play_logs_poi_id_started_at` ON `play_logs` (`poi_id`, `started_at`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE INDEX `IX_play_logs_session_id` ON `play_logs` (`session_id`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE INDEX `IX_poi_translations_language_code` ON `poi_translations` (`language_code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX `IX_poi_translations_poi_id_language_code` ON `poi_translations` (`poi_id`, `language_code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE INDEX `IX_pois_area_id_is_active` ON `pois` (`area_id`, `is_active`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE INDEX `IX_pois_lat_lng` ON `pois` (`lat`, `lng`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX `IX_tour_areas_code` ON `tour_areas` (`code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE INDEX `IX_visitor_sessions_area_id` ON `visitor_sessions` (`area_id`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    CREATE INDEX `IX_visitor_sessions_language_code` ON `visitor_sessions` (`language_code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261012125759_InitialCreate')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261012125759_InitialCreate', '10.0.9');
END;

COMMIT;

