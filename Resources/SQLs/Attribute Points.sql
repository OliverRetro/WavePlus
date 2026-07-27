SET @col_exists := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'user_rp_statistics'
      AND COLUMN_NAME = 'attribute_points'
);

SET @ddl := IF(@col_exists = 0,
    'ALTER TABLE `user_rp_statistics` ADD COLUMN `attribute_points` int(11) NOT NULL DEFAULT 0 AFTER `strength`',
    'SELECT "attribute_points already exists, skipping"'
);

PREPARE stmt FROM @ddl;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
