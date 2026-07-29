-- ============================================================================
--  Clickable chat links — whitelist + per-user warning toggle
-- ============================================================================
--  Only URLs whose host/prefix matches a row here become clickable in chat
--  bubbles. The list is loaded on emulator boot and is editable live with the
--  :linkwhitelist command (which reloads + re-pushes to every online client).
--
--  match_type:
--    domain    host == pattern OR host ends with ".pattern"
--              e.g. pattern "youtube.com" matches youtube.com AND www.youtube.com
--    wildcard  pattern is "*.base"; matches base and any subdomain of it
--              e.g. pattern "*.spotify.com" matches open.spotify.com
--    prefix    the full URL must start with pattern (case-insensitive)
--              e.g. pattern "https://discord.com/invite/" allows only invites
--                   while the rest of discord.com stays non-clickable
-- ============================================================================

CREATE TABLE IF NOT EXISTS `rp_link_whitelist` (
  `id`         INT(11)      NOT NULL AUTO_INCREMENT,
  `pattern`    VARCHAR(255) NOT NULL,
  `match_type` ENUM('domain','wildcard','prefix') NOT NULL DEFAULT 'domain',
  `enabled`    TINYINT(4)   NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`),
  UNIQUE KEY `pattern_type` (`pattern`, `match_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT IGNORE INTO `rp_link_whitelist` (`pattern`, `match_type`, `enabled`) VALUES
  ('youtube.com',      'domain',   1),
  ('youtu.be',         'domain',   1),
  ('*.spotify.com',    'wildcard', 1),
  ('spotify.com',      'domain',   1),
  ('tenor.com',        'domain',   1),
  ('giphy.com',        'domain',   1);
  -- Example of a specific-link allow (Discord invites only, rest of discord.com blocked):
  -- ('https://discord.com/invite/', 'prefix', 1);

-- Per-user toggle: show the "you are leaving the site" warning before opening a
-- link. Default 1 (on) as requested.
-- NOTE: plain ADD COLUMN for MySQL portability (MySQL has no ADD COLUMN IF NOT
-- EXISTS). Run once; if the column already exists, skip this statement.
ALTER TABLE `user_rp_settings`
  ADD COLUMN `link_warning_enabled` TINYINT(4) NOT NULL DEFAULT 1;

-- Admin permission for the :linkwhitelist command (group 6, same as :chatbubble add).
INSERT IGNORE INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_link_whitelist', 6);
