ALTER TABLE `group_roles`
  ADD COLUMN `created_at_ts` int NOT NULL DEFAULT 0;

-- UNIX_TIMESTAMP() yields NULL for zero-dates under NO_ZERO_DATE; those become 0.
UPDATE `group_roles`
  SET `created_at_ts` = IFNULL(UNIX_TIMESTAMP(`created_at`), 0);

ALTER TABLE `group_roles`
  DROP COLUMN `created_at`;

ALTER TABLE `group_roles`
  CHANGE COLUMN `created_at_ts` `created_at` int NOT NULL DEFAULT 0;

ALTER TABLE `group_roles`
  ADD COLUMN `updated_at_ts` int NULL DEFAULT NULL;

-- Nullable: UNIX_TIMESTAMP(NULL) is NULL, so a never-touched row stays NULL.
UPDATE `group_roles`
  SET `updated_at_ts` = UNIX_TIMESTAMP(`updated_at`);

ALTER TABLE `group_roles`
  DROP COLUMN `updated_at`;

ALTER TABLE `group_roles`
  CHANGE COLUMN `updated_at_ts` `updated_at` int NULL DEFAULT NULL;

-- group_permissions -----------------------------------------------------------
ALTER TABLE `group_permissions`
  ADD COLUMN `created_at_ts` int NOT NULL DEFAULT 0;

UPDATE `group_permissions`
  SET `created_at_ts` = IFNULL(UNIX_TIMESTAMP(`created_at`), 0);

ALTER TABLE `group_permissions`
  DROP COLUMN `created_at`;

ALTER TABLE `group_permissions`
  CHANGE COLUMN `created_at_ts` `created_at` int NOT NULL DEFAULT 0;

ALTER TABLE `group_permissions`
  ADD COLUMN `updated_at_ts` int NULL DEFAULT NULL;

UPDATE `group_permissions`
  SET `updated_at_ts` = UNIX_TIMESTAMP(`updated_at`);

ALTER TABLE `group_permissions`
  DROP COLUMN `updated_at`;

ALTER TABLE `group_permissions`
  CHANGE COLUMN `updated_at_ts` `updated_at` int NULL DEFAULT NULL;

-- user_chat_bubbles -----------------------------------------------------------
-- Loses DEFAULT CURRENT_TIMESTAMP; the server sets this column explicitly.
ALTER TABLE `user_chat_bubbles`
  ADD COLUMN `created_at_ts` int NULL DEFAULT NULL;

UPDATE `user_chat_bubbles`
  SET `created_at_ts` = UNIX_TIMESTAMP(`created_at`);

ALTER TABLE `user_chat_bubbles`
  DROP COLUMN `created_at`;

ALTER TABLE `user_chat_bubbles`
  CHANGE COLUMN `created_at_ts` `created_at` int NULL DEFAULT NULL;

-- rp_shift_logs ---------------------------------------------------------------
-- The index has to come off first: the column is dropped and re-added, not modified.
ALTER TABLE `rp_shift_logs`
  DROP INDEX `idx_rp_shift_logs_finished`;

ALTER TABLE `rp_shift_logs`
  ADD COLUMN `shift_finished_ts` int NOT NULL DEFAULT 0;

UPDATE `rp_shift_logs`
  SET `shift_finished_ts` = IFNULL(UNIX_TIMESTAMP(`shift_finished`), 0);

ALTER TABLE `rp_shift_logs`
  DROP COLUMN `shift_finished`;

ALTER TABLE `rp_shift_logs`
  CHANGE COLUMN `shift_finished_ts` `shift_finished` int NOT NULL DEFAULT 0;

ALTER TABLE `rp_shift_logs`
  ADD INDEX `idx_rp_shift_logs_finished` (`shift_finished`);

-- rp_crime_log ----------------------------------------------------------------
ALTER TABLE `rp_crime_log`
  DROP INDEX `idx_rp_crime_log_user_expired_time`;

ALTER TABLE `rp_crime_log`
  ADD COLUMN `timestamp_ts` int NOT NULL DEFAULT 0;

UPDATE `rp_crime_log`
  SET `timestamp_ts` = IFNULL(UNIX_TIMESTAMP(`timestamp`), 0);

ALTER TABLE `rp_crime_log`
  DROP COLUMN `timestamp`;

ALTER TABLE `rp_crime_log`
  CHANGE COLUMN `timestamp_ts` `timestamp` int NOT NULL DEFAULT 0;

ALTER TABLE `rp_crime_log`
  ADD INDEX `idx_rp_crime_log_user_expired_time` (`user_id`, `expired`, `timestamp`);


ALTER TABLE `user_rp_bank`
  ADD COLUMN `account_opened_ts` int NOT NULL DEFAULT 0;

-- UNIX_TIMESTAMP() yields NULL for zero-dates under NO_ZERO_DATE; those become 0.
UPDATE `user_rp_bank`
  SET `account_opened_ts` = IFNULL(UNIX_TIMESTAMP(`account_opened`), 0);

ALTER TABLE `user_rp_bank`
  DROP COLUMN `account_opened`;

ALTER TABLE `user_rp_bank`
  CHANGE COLUMN `account_opened_ts` `account_opened` int NOT NULL DEFAULT 0;

-- user_rp_bank_logs.timestamp -------------------------------------------------
-- The index has to come off first: the column is dropped and re-added, not modified.
ALTER TABLE `user_rp_bank_logs`
  DROP INDEX `idx_user_rp_bank_logs_user_time`;

ALTER TABLE `user_rp_bank_logs`
  ADD COLUMN `timestamp_ts` int NOT NULL DEFAULT 0;

UPDATE `user_rp_bank_logs`
  SET `timestamp_ts` = IFNULL(UNIX_TIMESTAMP(`timestamp`), 0);

ALTER TABLE `user_rp_bank_logs`
  DROP COLUMN `timestamp`;

ALTER TABLE `user_rp_bank_logs`
  CHANGE COLUMN `timestamp_ts` `timestamp` int NOT NULL DEFAULT 0;

ALTER TABLE `user_rp_bank_logs`
  ADD INDEX `idx_user_rp_bank_logs_user_time` (`user_id`, `timestamp`);