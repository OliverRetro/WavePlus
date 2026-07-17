-- ----------------------------------------------------------------------------
-- Converts the bank date/time columns to int unix timestamps (seconds).
--
-- Why: both columns were `datetime(6) NOT NULL DEFAULT '0000-00-00 00:00:00.000000'
-- ON UPDATE CURRENT_TIMESTAMP(6)'. The ON UPDATE clause makes EF Core treat the column
-- as store-generated, so on INSERT it omits the value and reads it back — and the
-- zero-date default cannot be converted to System.DateTime, which threw
-- MySqlConversionException on every CreateAccount. Unix ints have no zero-date, no
-- store-generation, and match how the rest of the schema stores time.
--
-- Safe to run once on an existing database. Take a backup first.
-- ----------------------------------------------------------------------------

-- user_rp_bank.account_opened -------------------------------------------------
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
