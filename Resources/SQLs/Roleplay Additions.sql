SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

CREATE TABLE IF NOT EXISTS `room_item_stock` (
  `id` INT(11) NOT NULL AUTO_INCREMENT,
  `room_id` INT(11) NOT NULL,
  `stock_type` VARCHAR(20) NOT NULL,
  `item_id` INT(11) NOT NULL,
  `amount` INT(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `room_item_stock_unique` (`room_id`, `stock_type`, `item_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT IGNORE INTO `server_settings` (`key`, `value`, `description`) VALUES
  ('rp.bank.rooms', '', 'Semicolon-separated room ids where users may open a bank account.'),
  ('rp.tazer.weapon.id', '5', 'rp_weapons id of the police tazer, issued on starting police work.'),
  ('rp.police.station.rooms', '', 'Semicolon-separated room ids that recharge an on-duty officer''s tazer. Empty = the police corporation room.');
 INSERT IGNORE INTO `server_settings` (`key`, `value`, `description`) VALUES
    ('rp.bank.withdrawal.fee.atm',   '5', 'Withdrawal fee % charged at a physical ATM. Falls back to rp.bank.withdrawal.fee.default.'),
    ('rp.bank.withdrawal.fee.phone', '5', 'Withdrawal fee % charged via the phone bank app. Falls back to rp.bank.withdrawal.fee.default.'),
    ('rp.bank.deposit.fee.atm',      '0', 'Deposit fee % charged at a physical ATM. Falls back to rp.bank.deposit.fee.default.'),
    ('rp.bank.deposit.fee.phone',    '0', 'Deposit fee % charged via the phone bank app. Falls back to rp.bank.deposit.fee.default.');
	INSERT INTO `rp_weapons`
  (`id`, `name`, `description`, `hit_message`, `critical_hit_message`, `minimum_damage`, `maximum_damage`, `critical_chance`, `critical_multiplier`, `effect_id`, `range`, `durability`, `image`, `stun_chance`, `rarity`)
SELECT 5, 'Tazer', 'Police-issue tazer', '*tazes %username%, stunning them*', '*tazes %username%, stunning them*', 0, 0, 0, 1, 0, 2, 10, 'none.png', 100, 0
WHERE NOT EXISTS (SELECT 1 FROM `rp_weapons` WHERE `id` = 5);

-- ── Vest items (secondary slot) ─────────────────────────────────────────────
INSERT INTO `rp_items` (`item_name`, `handitem_data`, `effect_data`, `base_price`, `attribute_effects`, `image_url`, `stack_limit`, `item_type`)
SELECT 'Body Armour', '0', '0', 300, '{"vest": 1, "shield": 20, "vest_reduction_min": 1, "vest_reduction_max": 3, "vest_fail_chance": 20}', '', 1, 'secondary'
WHERE NOT EXISTS (SELECT 1 FROM `rp_items` WHERE `item_name` = 'Body Armour');

INSERT INTO `rp_items` (`item_name`, `handitem_data`, `effect_data`, `base_price`, `attribute_effects`, `image_url`, `stack_limit`, `item_type`)
SELECT 'Suicide Vest', '0', '0', 500, '{"suicide_vest": 1, "explosion_min": 50, "explosion_max": 100}', '', 1, 'secondary'
WHERE NOT EXISTS (SELECT 1 FROM `rp_items` WHERE `item_name` = 'Suicide Vest');

UPDATE `server_settings` SET `value` = '2' WHERE `key` = 'rp.bank.deposit.fee.default';

-- disable clothing validation for staff
INSERT INTO `permissions` (`id`, `permission`, `description`) VALUES (53, 'clothing_no_validation', 'Doesn\'t validate sellable clothing.');
INSERT INTO `permissions_rights` (`group_id`, `permission_id`) VALUES (9, 53);
INSERT INTO `permissions_rights` (`group_id`, `permission_id`) VALUES (8, 53);

INSERT INTO `permissions_commands` (`command`) VALUES ('command_rp_user_info');
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_master_edit_rp', 6);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_room_heal', 5);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_global_heal', 6);
INSERT INTO `permissions_commands` (`command`) VALUES ('command_rp_toggle_skill');
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_hit', 1);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_buildheight', 1);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_unlockall', 6);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_wanted_list', 2);
INSERT INTO `permissions_commands` (`command`) VALUES ('command_rp_bank');
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_bank_account', 1);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_passive_mode', 1);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_group_permissions', 6);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_hire_anygroup', 6);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_gang_chat', 1);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_add_chat_bubble', 6);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_rob', 1);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_start_work', 1);
INSERT INTO `permissions_commands` (`command`) VALUES ('command_rp_give_item');
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_use_item', 1);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_stop_work', 1);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_livefeed_test', 6);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_police_override', 6);

-- corps n such
DROP TABLE IF EXISTS `group_roles`;
CREATE TABLE `group_roles`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `group_id` int NOT NULL COMMENT 'Group\'s ID.',
  `level` int NOT NULL DEFAULT 1 COMMENT 'Levels go in order of Slave -> CEO. 1 = Slave, 100 = CEO.',
  `name` varchar(55) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '' COMMENT 'Literal name displayed; ex. general manager',
  `shift_pay` int NOT NULL DEFAULT 0 COMMENT 'Credits paid via intervals.',
  `shift_costume` varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '' COMMENT 'Shift-Costume (full) to replace users own clothes. Blank for no swap.',
  `shift_duration` smallint NULL DEFAULT 5 COMMENT 'The duration at which worker will be auto-rewarded. In minutes.',
  `shift_motto` varchar(40) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '' COMMENT 'Replace users motto for the duration of shift? Blank for no swap.',
  `created_at` int NOT NULL DEFAULT 0,
  `updated_at` int NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `uniq_group_roles_group_level` (`group_id`,`level`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

DROP TABLE IF EXISTS `group_permission_keys`;
CREATE TABLE `group_permission_keys`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(55) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `key` varchar(55) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `description` varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT 'nodesc',
  `type_specific` text CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 25 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

INSERT INTO `group_permission_keys` VALUES (1, 'Decorate', 'edit_room', 'Edit the room furni.', '{0,1,2,3,4,5}');
INSERT INTO `group_permission_keys` VALUES (2, 'Hire', 'hire_user', 'Hire new players / accept membership requests.', '{1,5}');
INSERT INTO `group_permission_keys` VALUES (3, 'Promote', 'promote_user', 'Promote in the food chain, only works upto your level.', '{1,2,3,4,5}');
INSERT INTO `group_permission_keys` VALUES (4, 'Kick', 'kick_from_group', 'Kick from Group entirely.', '{0,1,2,3,4,5}');
INSERT INTO `group_permission_keys` VALUES (5, 'Edit Group', 'edit_group', 'Edit Group', '{1,2,3,4,5}');
INSERT INTO `group_permission_keys` VALUES (6, 'Edit Levels', 'edit_levels', 'Literally demote the players, pretty simple.', '{1,2,3,4,5}');
INSERT INTO `group_permission_keys` VALUES (7, 'Demote', 'demote_players', 'Demote them down the foodchain.', '{1,2,3,4,5}');
INSERT INTO `group_permission_keys` VALUES (8, 'Send Home', 'end_shift', 'Fuck \'em. Go sleep.', '{1}');
INSERT INTO `group_permission_keys` VALUES (9, 'Offer Item', 'offer_item', 'sell n shit', '{1,2,3,4,5}');
INSERT INTO `group_permission_keys` VALUES (10, 'Escort', 'escort_user', 'i mean yeah', '{1}');
INSERT INTO `group_permission_keys` VALUES (11, 'Stars', 'wanted_list_update', 'yeah for cops n shit ig', '{1}');
INSERT INTO `group_permission_keys` VALUES (12, 'Unescort', 'unescort_user', 'Unescort yeah', '{1}');
INSERT INTO `group_permission_keys` VALUES (13, 'Cuff', 'cuff_user', 'Cuff user yeah', '{1}');
INSERT INTO `group_permission_keys` VALUES (14, 'Invite', 'gang_invite', 'Invite to the gang', '{2,3,4}');
INSERT INTO `group_permission_keys` VALUES (15, 'Item Prices', 'alter_offer_prices', 'Alter prices for offer.', '{1,2,3,4,5}');
INSERT INTO `group_permission_keys` VALUES (16, 'Offerable Items', 'manage_offer_item', 'Manage offerable items from stock.', '{1,2,3,4,5}');
INSERT INTO `group_permission_keys` VALUES (17, 'Craft', 'craft_item', 'Craft stock for items.', '{1,2,3,4,5}');
INSERT INTO `group_permission_keys` VALUES (18, 'Arrest', 'arrest_user', 'Arrest users yea', '{1}');
INSERT INTO `group_permission_keys` VALUES (19, 'Fine', 'fine_user', 'Fine/ticket user.', '{1}');
INSERT INTO `group_permission_keys` VALUES (20, 'Release Prisoner', 'release_user', 'Uncharge user. Duh.', '{1}');
INSERT INTO `group_permission_keys` VALUES (21, 'Pardon', 'pardon_user', 'Remove from Wanted List etc.', '{1}');
INSERT INTO `group_permission_keys` VALUES (22, 'Membership Management', 'manage_group_acceptance', 'nodesc', '{0,1,2,3,4,5}');
INSERT INTO `group_permission_keys` VALUES (23, 'Heal', 'heal_user', 'For Hospital', '{1}');
INSERT INTO `group_permission_keys` VALUES (24, 'Room Swap', 'room_swap', 'Stay on shift when moving between rooms.', '{1,5}');

DROP TABLE IF EXISTS `group_permissions`;
CREATE TABLE `group_permissions`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `group_id` int NOT NULL COMMENT 'groups.id',
  `level_id` int NOT NULL COMMENT 'groups_roles.level',
  `permission_id` int NOT NULL COMMENT 'group_permissiosn_keys.id',
  `created_at` int NOT NULL DEFAULT 0,
  `updated_at` int NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

-- hospital
ALTER TABLE `user_rp_statistics`
ADD COLUMN `is_dead` int(11) NOT NULL DEFAULT 0,
ADD COLUMN `hospital_release_time` double NOT NULL DEFAULT 0,
ADD COLUMN `hospital_heal_start_time` double NOT NULL DEFAULT 0,
ADD COLUMN `hospital_health_start` int(11) NOT NULL DEFAULT 0;

-- police escort / jail persistence (see notes at the settings block below)
ALTER TABLE `user_rp_statistics`
ADD COLUMN `is_cuffed` tinyint(1) NOT NULL DEFAULT 0,
ADD COLUMN `jail_release_time` double NOT NULL DEFAULT 0,
ADD COLUMN `jail_pending` tinyint(1) NOT NULL DEFAULT 0,
ADD COLUMN `jail_stars` int NOT NULL DEFAULT 0,
ADD COLUMN `jail_revert_look` varchar(512) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '',
ADD COLUMN `jail_room_id` int NOT NULL DEFAULT 0;

-- jail time is paused while offline: remaining seconds are banked here on disconnect
ALTER TABLE `user_rp_statistics`
ADD COLUMN `jail_time_left` double NOT NULL DEFAULT 0;

-- server settings
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.hospital.room.id', '0', 'The hospital room.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('user.aggression_scheduler.tick', '5', 'The time (in minutes) users will get deducted aggression.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.bank.withdrawal.fee.default', '5', 'As percentage! :-) Used for ATMs.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.bank.deposit.fee.default', '0', 'As percentage! ATM usage. Deposits are free by default; only withdrawals are charged.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.bank.rooms', '', 'Semicolon-separated room ids where users may open a bank account.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.bank.account.opening.cost', '50', 'The cost for opening an account.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.police.corporation.id', '1', 'This is for police corporation (group id).');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.tazer.weapon.id', '5', 'rp_weapons id of the police tazer, issued on starting police work.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.police.station.rooms', '', 'Semicolon-separated room ids that recharge an on-duty officer''s tazer. Empty = the police corporation room.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.ambulance.corporation.id', '0', 'Ambulance/EMS corporation group id. Set this to the hospital/EMS group.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.ambulance.heal.amount', '25', 'HP restored by the :heal command.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.ambulance.revive.health', '35', 'HP restored when an EMS revives a dead user.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.ambulance.heal.energy.cost', '2', 'Energy cost for EMS heal/revive.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.trash.search.duration.seconds', '5', 'Seconds a player must wait while searching a trashbin.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.trash.search.cooldown.seconds', '60', 'Per-user cooldown after searching trash.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.trash.bin.cooldown.seconds', '120', 'Per-bin cooldown after it has been searched.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.trash.search.energy.cost', '2', 'Energy cost for searching a trashbin.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.trash.search.item.chance', '35', 'Base chance percentage to find an RP item from trash.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.trash.search.max.credits', '8', 'Maximum fallback credits found from trash.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.trash.search.range', '1', 'Tile range from a trashbin required for searching.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.jail.room.id', '1', 'Jail room ID.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.jail.outfit', 'outfit lol', 'Jail outfit.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.staff.corporation.id', '0', 'Staff corporation (group id). Working here grants the staff effect (102) and blocks weapons.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('livefeed.discord.webhook', '', 'Discord Webhook for Live Feed. Leave blank If disabled.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.wardrobe.room.body', '29', 'Room where everything, but hair can be changed.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.wardrobe.room.head', '29', 'Room where only hair can be changed.');

-- fix rooms?
ALTER TABLE rooms 
MODIFY allow_pets_eat TINYINT(1) NOT NULL DEFAULT 0;

ALTER TABLE rooms 
MODIFY allow_pets TINYINT(1) NOT NULL DEFAULT 0;

ALTER TABLE rooms 
MODIFY room_blocking_disabled TINYINT(1) NOT NULL DEFAULT 0;

ALTER TABLE rooms 
MODIFY allow_hidewall TINYINT(1) NOT NULL DEFAULT 0;

UPDATE rooms SET allow_hidewall = 0;

ALTER TABLE rooms 
MODIFY mute_settings TINYINT(1) NOT NULL DEFAULT 1;

ALTER TABLE rooms 
MODIFY kick_settings TINYINT(2) NOT NULL DEFAULT 1;

ALTER TABLE rooms 
MODIFY ban_settings TINYINT(1) NOT NULL DEFAULT 1;

ALTER TABLE rooms 
MODIFY push_enabled TINYINT(1) NOT NULL DEFAULT 1;

ALTER TABLE rooms 
MODIFY pull_enabled TINYINT(1) NOT NULL DEFAULT 1;

ALTER TABLE rooms 
MODIFY enables_enabled TINYINT(1) NOT NULL DEFAULT 1;

ALTER TABLE rooms 
MODIFY respect_notifications_enabled TINYINT(1) NOT NULL DEFAULT 1;

ALTER TABLE rooms 
MODIFY pet_morphs_allowed TINYINT(1) NOT NULL DEFAULT 1;

ALTER TABLE rooms 
MODIFY spull_enabled TINYINT(1) NOT NULL DEFAULT 1;

ALTER TABLE rooms 
MODIFY push_enabled TINYINT(1) NOT NULL DEFAULT 1;

-- new tables
DROP TABLE IF EXISTS `user_rp_statistics`;
CREATE TABLE `user_rp_statistics`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL DEFAULT 0,
  `health` int NOT NULL DEFAULT 100,
  `shield` int NOT NULL DEFAULT 0,
  `energy` int NOT NULL DEFAULT 100,
  `hunger` int NOT NULL DEFAULT 25,
  `experience` int NOT NULL DEFAULT 0,
  `knockouts` int NOT NULL DEFAULT 0,
  `deaths` int NOT NULL DEFAULT 0,
  `arrested` int NOT NULL DEFAULT 0,
  `escapes` int NOT NULL DEFAULT 0,
  `damage_dealt` bigint NOT NULL DEFAULT 0,
  `damage_taken` bigint NOT NULL DEFAULT 0,
  `attacks` int NOT NULL DEFAULT 0,
  `attacked` int NOT NULL DEFAULT 0,
  `knowledge` int NOT NULL DEFAULT 0,
  `strength` int NOT NULL DEFAULT 0,
  `is_dead` int NOT NULL DEFAULT 0,
  `hospital_release_time` double NOT NULL DEFAULT 0,
  `hospital_heal_start_time` double NOT NULL DEFAULT 0,
  `hospital_health_start` int NOT NULL DEFAULT 0,
  `aggression` int NOT NULL DEFAULT 0,
  `passive_mode` tinyint(1) NOT NULL DEFAULT 0,
  `robberies` int NOT NULL DEFAULT 0,
  `robbed` int NOT NULL DEFAULT 0,
  `is_cuffed` tinyint(1) NOT NULL DEFAULT 0,
  `jail_release_time` double NOT NULL DEFAULT 0,
  `jail_pending` tinyint(1) NOT NULL DEFAULT 0,
  `jail_stars` int NOT NULL DEFAULT 0,
  `jail_revert_look` varchar(512) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '',
  `jail_room_id` int NOT NULL DEFAULT 0,
  `jail_time_left` double NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

DROP TABLE IF EXISTS `user_rp_skills`;
CREATE TABLE `user_rp_skills`  (
  `id` int NOT NULL AUTO_INCREMENT,
 `user_id` int NOT NULL,
  `skill_id` int NOT NULL,
  `progress` int NOT NULL DEFAULT 0,
  `equipped` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

DROP TABLE IF EXISTS `rp_skills_levels`;
CREATE TABLE `rp_skills_levels`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `skill_id` int NOT NULL,
  `level` varchar(25) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '1',
  `required_progress` int NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 25 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

INSERT INTO `rp_skills_levels` VALUES (1, 1, '1', 1000);
INSERT INTO `rp_skills_levels` VALUES (2, 1, '2', 5000);
INSERT INTO `rp_skills_levels` VALUES (3, 1, '3', 10000);
INSERT INTO `rp_skills_levels` VALUES (4, 2, '1', 100);
INSERT INTO `rp_skills_levels` VALUES (5, 2, '2', 500);
INSERT INTO `rp_skills_levels` VALUES (6, 2, '3', 1000);
INSERT INTO `rp_skills_levels` VALUES (7, 3, '1', 10);
INSERT INTO `rp_skills_levels` VALUES (8, 3, '2', 50);
INSERT INTO `rp_skills_levels` VALUES (9, 3, '3', 100);
INSERT INTO `rp_skills_levels` VALUES (10, 4, '1', 10);
INSERT INTO `rp_skills_levels` VALUES (11, 4, '2', 50);
INSERT INTO `rp_skills_levels` VALUES (12, 4, '3', 100);
INSERT INTO `rp_skills_levels` VALUES (13, 5, '1', 1000);
INSERT INTO `rp_skills_levels` VALUES (14, 5, '2', 5000);
INSERT INTO `rp_skills_levels` VALUES (15, 5, '3', 10000);
INSERT INTO `rp_skills_levels` VALUES (16, 6, '1', 50);
INSERT INTO `rp_skills_levels` VALUES (17, 6, '2', 200);
INSERT INTO `rp_skills_levels` VALUES (18, 6, '3', 500);
INSERT INTO `rp_skills_levels` VALUES (19, 7, '1', 100);
INSERT INTO `rp_skills_levels` VALUES (20, 7, '2', 500);
INSERT INTO `rp_skills_levels` VALUES (21, 7, '3', 1000);
INSERT INTO `rp_skills_levels` VALUES (22, 8, '1', 100);
INSERT INTO `rp_skills_levels` VALUES (23, 8, '2', 500);
INSERT INTO `rp_skills_levels` VALUES (24, 8, '3', 1000);

DROP TABLE IF EXISTS `rp_skills`;
CREATE TABLE `rp_skills`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(25) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '',
  `progress_category` varchar(25) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `description` varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT 'No description yet.',
  `badge_code` varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 9 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

INSERT INTO `rp_skills` VALUES (1, 'Striker', 'Damage', 'Chance to stun your target while in combat.', 'Striker');
INSERT INTO `rp_skills` VALUES (2, 'Farmer', 'Harvest', 'Earn bonus in crop gathering when harvesting.', 'Farmer');
INSERT INTO `rp_skills` VALUES (3, 'Pickpocket', 'Rob', 'Increases success chance when robbing a user.', 'Pickpocket');
INSERT INTO `rp_skills` VALUES (4, 'Ghost', 'Heist', 'Reduces chance of triggering automatic police alerts.', 'Ghost');
INSERT INTO `rp_skills` VALUES (5, 'Hardworker', 'Commission', 'Boost income and commission from sales.', 'Hardworker');
INSERT INTO `rp_skills` VALUES (6, 'Lockpick', 'Lockpick', 'Increases your success rate when using lockpicks.', 'Lockpick');
INSERT INTO `rp_skills` VALUES (7, 'Stamina-zor', 'Gym', 'Boosts health, energy, and hunger max.', 'Stamina');
INSERT INTO `rp_skills` VALUES (8, 'Scavenger', 'Scavenge', 'Boosted chances of finding items when searching bins/crates.', 'Scavenger');

-- weapons
DROP TABLE IF EXISTS `user_rp_weapons`;
CREATE TABLE `user_rp_weapons`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `weapon_id` int NOT NULL,
  `durability_left` int NOT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 5 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

DROP TABLE IF EXISTS `rp_weapons`;
CREATE TABLE `rp_weapons`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(55) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '',
  `description` varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '',
  `hit_message` varchar(110) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '*swings at %username%, dealing %damage% damage*' COMMENT 'Variables available username and damage',
  `critical_hit_message` varchar(110) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '*lands a critical hit on %username%, dealing %damage% damage*',
  `minimum_damage` int NOT NULL DEFAULT 0,
  `maximum_damage` int NOT NULL DEFAULT 1,
  `critical_chance` smallint NOT NULL DEFAULT 10 COMMENT 'As %. For instance 50% is 50-50 on hitting it, without Skill boosts.',
  `critical_multiplier` double NOT NULL,
  `effect_id` smallint NOT NULL DEFAULT 0,
  `range` int NOT NULL DEFAULT 0,
  `durability` int NOT NULL DEFAULT 0,
  `image` varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '',
  `stun_chance` int NOT NULL COMMENT '0-100',
  `rarity` smallint NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 5 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;


INSERT INTO `rp_weapons` VALUES (1, 'Bat', 'Basketball bat, duh', '*swings at %username%, dealing %damage% damage*', '*lands a critical hit on %username%, dealing %damage% damage*', 4, 6, 0, 1, 0, 50, 50, 'none.png', 5, 0);
INSERT INTO `rp_weapons` VALUES (2, 'Axe', 'An literal axe', '*swings at %username%, dealing %damage% damage*', '*lands a critical hit on %username%, dealing %damage% damage*', 7, 9, 0, 1, 0, 50, 50, 'none.png', 7, 0);
INSERT INTO `rp_weapons` VALUES (3, 'Sword', 'Sword', '*swings at %username%, dealing %damage% damage*', '*lands a critical hit on %username%, dealing %damage% damage*', 12, 15, 50, 2, 162, 50, 50, 'none.png', 100, 1);
INSERT INTO `rp_weapons` VALUES (4, 'Kunai', 'you throw these shits', '*throws Kunai at %username%, dealing %damage% damage*', '*lands a critical hit on %username%, dealing %damage% damage*', 50, 60, 10, 0, 0, 50, 3, 'none.png', 0, 0);
-- Police-issue tazer: no damage, 100% stun, 10 charges, range 2 (double so diagonal tiles are reachable).
INSERT INTO `rp_weapons` VALUES (5, 'Tazer', 'Police-issue tazer', '*tazes %username%, stunning them*', '*tazes %username%, stunning them*', 0, 0, 0, 1, 0, 2, 10, 'none.png', 100, 0);

DROP TABLE IF EXISTS `user_rp_weapon_skins`;
CREATE TABLE `user_rp_weapon_skins`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `skin_id` int NOT NULL,
  `equipped` int NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

DROP TABLE IF EXISTS `rp_weapon_skins`;
CREATE TABLE `rp_weapon_skins`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `weapon_id` int NOT NULL,
  `name` varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `hit_message` varchar(110) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '*swings at %username%, dealing %damage% damage*',
  `critical_hit_message` varchar(110) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '*lands a critical hit on %username%, dealing %damage% damage*',
  `base_price` int NOT NULL DEFAULT 0,
  `rarity` smallint NOT NULL DEFAULT 0,
  `effect_id` int NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

INSERT INTO `rp_weapon_skins` VALUES (1, 3, 'Elden Wand', '*fires a fire blast at %username%, dealing %damage% damage*', '*fucking destroys %username%, dealing %damage% damage*', 0, 3, 26);

-- bankingz
DROP TABLE IF EXISTS `user_rp_bank`;
CREATE TABLE `user_rp_bank`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `balance` int NOT NULL DEFAULT 0,
  `account_opened` int NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

DROP TABLE IF EXISTS `user_rp_bank_logs`;
CREATE TABLE `user_rp_bank_logs`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `amount` int NOT NULL DEFAULT 0,
  `action_type` varchar(32) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT 'DEPOSIT',
  `management_type` varchar(32) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT 'ATM',
  `fee_paid` int NOT NULL,
  `timestamp` int NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;


ALTER TABLE `users`
ADD COLUMN `home_room_data` LONGTEXT NOT NULL DEFAULT '{"roomid":17,"x":18,"y":22,"z":0,"rotation":4,"weapon":0}';

ALTER TABLE `rooms`
ADD COLUMN `safezone` varchar(4) NOT NULL DEFAULT '0';

ALTER TABLE `subscriptions`
ADD COLUMN `static_aggression` smallint(6) NOT NULL DEFAULT 3;

ALTER TABLE `subscriptions`
ADD COLUMN `trash_search_cooldown` smallint(6) NOT NULL DEFAULT 60;

-- chat bubbles
DROP TABLE IF EXISTS `user_chat_bubbles`;
CREATE TABLE `user_chat_bubbles` (
  `id` int NOT NULL AUTO_INCREMENT,
  `chat_bubble_id` int NOT NULL,
  `user_id` int NOT NULL,
  `created_at` int NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `uniq_user_bubble` (`user_id`, `chat_bubble_id`),
  KEY `idx_user_chat_bubbles_user_id` (`user_id`),
  CONSTRAINT `fk_user_chat_bubbles_user`
    FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci ROW_FORMAT=Dynamic;

-- corporation shift logs
DROP TABLE IF EXISTS `rp_shift_logs`;
CREATE TABLE `rp_shift_logs` (
  `id` int NOT NULL AUTO_INCREMENT,
  `group_id` int NOT NULL,
  `user_id` int NOT NULL,
  `credits_paid` int NOT NULL DEFAULT 0,
  `shift_finished` int NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE,
  KEY `idx_rp_shift_logs_group_user` (`group_id`, `user_id`),
  KEY `idx_rp_shift_logs_finished` (`shift_finished`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci ROW_FORMAT=Dynamic;

-- rp items
DROP TABLE IF EXISTS `rp_items`;
CREATE TABLE `rp_items`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `item_name` varchar(55) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `handitem_data` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `effect_data` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `base_price` int NOT NULL DEFAULT 0,
  `attribute_effects` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `image_url` varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

-- example data
INSERT INTO `rp_items` VALUES (1, 'Bandage', '0', '69', 50, '{\"hp\": 15, \"consume_length\": 100, \"uses\": 1}', '');
INSERT INTO `rp_items` VALUES (2, 'Red Bull', '14', '0', 12, '{\"hp\": -1, \"energy\": 25, \"hunger\": 5, \"hunger_saturation\": 0.5, \"consume_length\": 400, \"shield\": 50, \"experience\": 10, \"uses\": 1}', '');


-- searchable trashbin reward pool (optional; server falls back to all rp_items if empty)
DROP TABLE IF EXISTS `rp_trash_rewards`;
CREATE TABLE `rp_trash_rewards` (
  `id` int NOT NULL AUTO_INCREMENT,
  `item_id` int NOT NULL,
  `chance_weight` int NOT NULL DEFAULT 1,
  `min_amount` int NOT NULL DEFAULT 1,
  `max_amount` int NOT NULL DEFAULT 1,
  `enabled` tinyint(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci ROW_FORMAT=Dynamic;

INSERT INTO `rp_trash_rewards` (`item_id`, `chance_weight`, `min_amount`, `max_amount`, `enabled`) VALUES (1, 5, 1, 1, 1);
INSERT INTO `rp_trash_rewards` (`item_id`, `chance_weight`, `min_amount`, `max_amount`, `enabled`) VALUES (2, 3, 1, 1, 1);

DROP TABLE IF EXISTS `user_rp_items`;
CREATE TABLE `user_rp_items`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `item_id` int NOT NULL,
  `user_id` int NOT NULL,
  `uses` smallint NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

-- web overlay (gaz-js) macros: keybind -> command sets
CREATE TABLE IF NOT EXISTS `users_macros` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `name` tinytext NOT NULL,
  `configs` varchar(16000) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `uniq_user_macro` (`user_id`, `name`(15))
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci ROW_FORMAT=Dynamic;

-- inventory grid positions (overlay drag & drop)
ALTER TABLE `user_rp_items`
ADD COLUMN `slot` int NOT NULL DEFAULT -1;

-- web overlay macros: per-user global enabled flag
CREATE TABLE IF NOT EXISTS `users_macro_settings` (
  `user_id` int NOT NULL,
  `enabled` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`user_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci ROW_FORMAT=Dynamic;

-- ── RP item stacking + item types (shields) ──────────────────────────────
-- stack_limit: how many copies stack into a single inventory slot (1 = never stacks).
-- item_type: 'consumable' (default), 'secondary' (shield slot), 'primary' (replaces weapon).
ALTER TABLE `rp_items`
ADD COLUMN `stack_limit` int NOT NULL DEFAULT 1,
ADD COLUMN `item_type` varchar(32) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT 'consumable';

-- example secondary (shield) item: 100 shield / durability, occupies the secondary equipped slot.
INSERT INTO `rp_items` (`item_name`, `handitem_data`, `effect_data`, `base_price`, `attribute_effects`, `image_url`, `stack_limit`, `item_type`)
VALUES ('Riot Shield', '0', '0', 250, '{\"shield\": 100}', '', 1, 'secondary');

-- example lockpick: `handpick` is the % chance to break free of cuffs (only usable while
-- cuffed AND free to move -- not while being escorted or jailed).
INSERT INTO `rp_items` (`item_name`, `handitem_data`, `effect_data`, `base_price`, `attribute_effects`, `image_url`, `stack_limit`, `item_type`)
VALUES ('Lockpick', '0', '0', 150, '{\"handpick\": 35, \"uses\": 1}', '', 1, 'consumable');

-- Standard body-armour vest (secondary slot). `shield` is its durability (hits it survives);
-- `vest_reduction_min`/`max` is the configurable flat damage removed from a non-fist hit;
-- `vest_fail_chance` (0-100) is the chance a hit isn't reduced but still eats durability.
INSERT INTO `rp_items` (`item_name`, `handitem_data`, `effect_data`, `base_price`, `attribute_effects`, `image_url`, `stack_limit`, `item_type`)
VALUES ('Body Armour', '0', '0', 300, '{\"vest\": 1, \"shield\": 20, \"vest_reduction_min\": 1, \"vest_reduction_max\": 3, \"vest_fail_chance\": 20}', '', 1, 'secondary');

-- Suicide vest (secondary slot). Double-click the equipped vest to detonate: 50-100 explosive
-- damage across the surrounding 3x3 grid, guaranteed self knockout, and the vest is consumed.
INSERT INTO `rp_items` (`item_name`, `handitem_data`, `effect_data`, `base_price`, `attribute_effects`, `image_url`, `stack_limit`, `item_type`)
VALUES ('Suicide Vest', '0', '0', 500, '{\"suicide_vest\": 1, \"explosion_min\": 50, \"explosion_max\": 100}', '', 1, 'secondary');

-- per-owned-item durability (shields) + which item is equipped in the secondary slot.
ALTER TABLE `user_rp_items`
ADD COLUMN `durability_left` int NOT NULL DEFAULT -1,
ADD COLUMN `equipped` tinyint(1) NOT NULL DEFAULT 0;

-- weapons share the same draggable grid as items, so they need a persisted slot too.
ALTER TABLE `user_rp_weapons`
ADD COLUMN `slot` int NOT NULL DEFAULT -1;


-- crimes
DROP TABLE IF EXISTS `rp_crime_log`;
CREATE TABLE `rp_crime_log`  (
  `id` int NOT NULL,
  `crime_id` int NOT NULL,
  `user_id` int NOT NULL,
  `timestamp` int NOT NULL DEFAULT 0,
  `expired` tinyint NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

DROP TABLE IF EXISTS `rp_crime_penalties`;
CREATE TABLE `rp_crime_penalties`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `star_level` int NOT NULL,
  `fine_amount` int NOT NULL DEFAULT 0,
  `jail_time` smallint NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 6 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

INSERT INTO `rp_crime_penalties` VALUES (1, 1, 5, 3);
INSERT INTO `rp_crime_penalties` VALUES (2, 2, 10, 6);
INSERT INTO `rp_crime_penalties` VALUES (3, 3, 15, 9);
INSERT INTO `rp_crime_penalties` VALUES (4, 4, 20, 12);
INSERT INTO `rp_crime_penalties` VALUES (5, 5, 50, 15);

-- ----------------------------
DROP TABLE IF EXISTS `rp_crimes`;
CREATE TABLE `rp_crimes`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `crime_tag` varchar(32) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `crime_name` varchar(55) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `severity` smallint NOT NULL DEFAULT 1,
  `time_active` smallint NOT NULL DEFAULT 1,
  `automatic_report` tinyint NOT NULL DEFAULT 0,
  `police_alert` tinyint NOT NULL DEFAULT 0,
  `note` varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT '',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uniq_rp_crimes_tag`(`crime_tag` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 15 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of rp_crimes
-- ----------------------------
INSERT INTO `rp_crimes` VALUES (1, 'murder', 'Murder', 2, 20, 1, 1, '');
INSERT INTO `rp_crimes` VALUES (2, 'rob', 'Robbery', 1, 10, 0, 0, '');
INSERT INTO `rp_crimes` VALUES (3, 'execution', 'Execution', 1, 1, 0, 0, '');
INSERT INTO `rp_crimes` VALUES (4, 'invasion', 'Invasion', 1, 1, 0, 0, '');
INSERT INTO `rp_crimes` VALUES (5, 'mass_murder', 'Mass Murder', 1, 1, 0, 0, '');
INSERT INTO `rp_crimes` VALUES (6, 'cop_murder', 'Cop Murder', 1, 1, 0, 0, '');
INSERT INTO `rp_crimes` VALUES (7, 'logout', 'Logout', 1, 1, 0, 0, '');
INSERT INTO `rp_crimes` VALUES (8, 'assault', 'Assault', 1, 1, 0, 0, '');
INSERT INTO `rp_crimes` VALUES (9, '911_abuse', '911 Abuse', 1, 1, 0, 0, '');
INSERT INTO `rp_crimes` VALUES (10, 'obstruction', 'Obstruction', 1, 1, 0, 0, '');
INSERT INTO `rp_crimes` VALUES (11, 'cop_assault', 'Cop Assault', 1, 1, 0, 0, '');
INSERT INTO `rp_crimes` VALUES (12, 'gang_homicide', 'Gang Homicide', 1, 1, 0, 0, '');

-- crime logs are inserted by the server on disconnect/reload; give the id auto-increment.
ALTER TABLE `rp_crime_log` MODIFY `id` int NOT NULL AUTO_INCREMENT;


-- performance indexes / live stability
-- These keep RP lookups fast once the hotel has 100+ online users and a larger database.
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.weboverlay.debug', '0', 'Set to 1 only while debugging packet 314 web overlay callbacks.');

ALTER TABLE `user_rp_statistics`
ADD UNIQUE KEY `uniq_user_rp_statistics_user_id` (`user_id`);

ALTER TABLE `user_rp_skills`
ADD UNIQUE KEY `uniq_user_rp_skills_user_skill` (`user_id`, `skill_id`),
ADD KEY `idx_user_rp_skills_skill_id` (`skill_id`);

ALTER TABLE `user_rp_weapons`
ADD KEY `idx_user_rp_weapons_user_id` (`user_id`),
ADD KEY `idx_user_rp_weapons_weapon_id` (`weapon_id`);

ALTER TABLE `user_rp_weapon_skins`
ADD KEY `idx_user_rp_weapon_skins_user_id` (`user_id`),
ADD KEY `idx_user_rp_weapon_skins_skin_id` (`skin_id`);

ALTER TABLE `user_rp_items`
ADD KEY `idx_user_rp_items_user_item` (`user_id`, `item_id`),
ADD KEY `idx_user_rp_items_equipped` (`user_id`, `equipped`);

ALTER TABLE `user_rp_bank`
ADD UNIQUE KEY `uniq_user_rp_bank_user_id` (`user_id`);

ALTER TABLE `user_rp_bank_logs`
ADD KEY `idx_user_rp_bank_logs_user_time` (`user_id`, `timestamp`);

ALTER TABLE `rp_crime_log`
ADD KEY `idx_rp_crime_log_user_expired_time` (`user_id`, `expired`, `timestamp`),
ADD KEY `idx_rp_crime_log_crime_id` (`crime_id`);

ALTER TABLE `rp_crimes`
ADD UNIQUE KEY `uniq_rp_crimes_tag` (`crime_tag`);

ALTER TABLE `rp_crime_penalties`
ADD UNIQUE KEY `uniq_rp_crime_penalties_star` (`star_level`);

ALTER TABLE `rp_trash_rewards`
ADD KEY `idx_rp_trash_rewards_enabled_weight` (`enabled`, `chance_weight`);

INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.cuff.figure.male', 'ch-3688-1323', 'Figure part applied to a cuffed male suspect (their old look is restored on release).');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.cuff.figure.female', 'ch-3689-1323', 'Figure part applied to a cuffed female suspect (their old look is restored on release).');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.jail.rooms', '', 'Semicolon-separated room ids that act as jails. Escorting a suspect into one auto-jails them.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.police.escort.jail.bonus', '20', 'Credits paid to the escorting officer when their suspect is jailed.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.jail.spawn', '{"roomid":4,"x":8,"y":9,"z":0,"rotation":6}', 'Jail spawn point: room + tile a jailed suspect is teleported to.');


DROP TABLE IF EXISTS `user_rp_settings`;
CREATE TABLE `user_rp_settings`  (
  `user_id` int NOT NULL,
  `clickthrough_enabled` tinyint NOT NULL DEFAULT 0,
  `auto_salary_deposit` tinyint NOT NULL DEFAULT 0,
  `furniture_trading_enabled` tinyint NOT NULL DEFAULT 1,
  `auto_event_transfer` tinyint NOT NULL DEFAULT 0,
  `voice_chat_enabled` tinyint NOT NULL DEFAULT 1,
  `livefeed_enabled` tinyint NOT NULL DEFAULT 1,
  `alerts_enabled` tinyint NOT NULL DEFAULT 1,
  PRIMARY KEY (`user_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
