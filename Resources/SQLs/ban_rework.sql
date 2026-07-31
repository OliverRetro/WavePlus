-- =============================================================================
--  Ban system rework  (:ban / :ipban / :mip / :unban)
--  One ban == one row. bantype is either 'account' or 'ip'; every row carries the
--  target's user_id so a single :unban lifts it. Run once against the live database.
-- =============================================================================

-- 1) Add the user_id column (account bans key on it; ip bans still key on value).
ALTER TABLE `bans`
    ADD COLUMN `user_id` INT(11) NOT NULL DEFAULT 0 AFTER `id`,
    ADD KEY `user_id` (`user_id`);

-- 2) Collapse the old three-type scheme into just account/ip.
--    old 'user'/'machine' -> 'account', old 'ip' stays 'ip'.
ALTER TABLE `bans` MODIFY `bantype` VARCHAR(20) NOT NULL DEFAULT 'account';
UPDATE `bans` SET `bantype` = 'account' WHERE `bantype` IN ('user', 'machine');
ALTER TABLE `bans` MODIFY `bantype` ENUM('account','ip') NOT NULL DEFAULT 'account';

-- 3) Backfill user_id for existing account bans (their value is the username).
UPDATE `bans` b
    JOIN `users` u ON u.username = b.value
    SET b.user_id = u.id
    WHERE b.bantype = 'account' AND b.user_id = 0;

-- 4) The emulator upserts on (user_id, bantype) so re-banning updates the single row
--    instead of duplicating. That needs a UNIQUE index on the pair. If this fails with
--    a duplicate-key error, run the de-dup DELETE below, then re-run this statement.
ALTER TABLE `bans`
    ADD UNIQUE KEY `userid_bantype` (`user_id`, `bantype`);

-- De-dup helper (only if the UNIQUE add above failed):
-- DELETE b1 FROM `bans` b1
--   JOIN `bans` b2
--     ON b1.user_id = b2.user_id AND b1.bantype = b2.bantype AND b1.id > b2.id;

-- 5) Register the :unban command + alias (group_id mirrors :ban's command_ban).
--    `keys` is the column the alias loader (ApplyDatabaseAliases) reads.
INSERT INTO `permissions_commands` (`command`, `group_id`, `subscription_id`, `keys`)
    VALUES ('command_unban', 2, 0, 'unban')
    ON DUPLICATE KEY UPDATE `keys` = 'unban';
