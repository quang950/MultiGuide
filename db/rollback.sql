START TRANSACTION;
DROP TABLE `access_passes`;

DROP TABLE `admin_users`;

DROP TABLE `play_logs`;

DROP TABLE `poi_translations`;

DROP TABLE `payments`;

DROP TABLE `pois`;

DROP TABLE `visitor_sessions`;

DROP TABLE `languages`;

DROP TABLE `tour_areas`;

DELETE FROM `__EFMigrationsHistory`
WHERE `MigrationId` = '20261012125759_InitialCreate';

COMMIT;

