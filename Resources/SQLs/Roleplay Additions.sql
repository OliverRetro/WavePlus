SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

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
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_passive_mode', 1);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_group_permissions', 6);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_hire_anygroup', 6);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_gang_chat', 1);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_add_chat_bubble', 6);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_rob', 1);

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
  `created_at` datetime NOT NULL DEFAULT '0000-00-00 00:00:00',
  `updated_at` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
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
) ENGINE = InnoDB AUTO_INCREMENT = 23 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

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

DROP TABLE IF EXISTS `group_permissions`;
CREATE TABLE `group_permissions`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `group_id` int NOT NULL COMMENT 'groups.id',
  `level_id` int NOT NULL COMMENT 'groups_roles.level',
  `permission_id` int NOT NULL COMMENT 'group_permissiosn_keys.id',
  `created_at` datetime NOT NULL,
  `updated_at` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

-- hospital
ALTER TABLE `user_rp_statistics`
ADD COLUMN `is_dead` int(11) NOT NULL DEFAULT 0,
ADD COLUMN `hospital_release_time` double NOT NULL DEFAULT 0,
ADD COLUMN `hospital_heal_start_time` double NOT NULL DEFAULT 0,
ADD COLUMN `hospital_health_start` int(11) NOT NULL DEFAULT 0;

-- server settings
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.hospital.room.id', '0', 'The hospital room.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('user.aggression_scheduler.tick', '5', 'The time (in minutes) users will get deducted aggression.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.bank.withdrawal.fee.default', '5', 'As percentage! :-) Used for ATMs.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.bank.deposit.fee.default', '5', 'As percentage! ATM usage.');
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES ('rp.bank.account.opening.cost', '50', 'The cost for opening an account.');

-- fix rooms?
ALTER TABLE rooms 
MODIFY allow_pets_eat TINYINT(1) NOT NULL DEFAULT 0;

ALTER TABLE rooms 
MODIFY allow_pets TINYINT(1) NOT NULL DEFAULT 0;

ALTER TABLE rooms 
MODIFY room_blocking_disabled TINYINT(1) NOT NULL DEFAULT 0;

ALTER TABLE rooms 
MODIFY allow_hidewall TINYINT(1) NOT NULL DEFAULT 1;

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
  `robberies` int NOT NULL DEFAULT 0,
  `robbed` int NOT NULL DEFAULT 0,
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
  `account_opened` datetime(6) NOT NULL DEFAULT '0000-00-00 00:00:00.000000' ON UPDATE CURRENT_TIMESTAMP(6),
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
  `timestamp` datetime(6) NOT NULL DEFAULT '0000-00-00 00:00:00.000000' ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;


ALTER TABLE `users`
ADD COLUMN `home_room_data` LONGTEXT NOT NULL DEFAULT '{"roomid":17,"x":18,"y":22,"z":0,"rotation":4,"weapon":0}';

ALTER TABLE `rooms`
ADD COLUMN `safezone` varchar(4) NOT NULL DEFAULT '0';

ALTER TABLE `user_rp_statistics`
ADD COLUMN `aggression` smallint(6) NOT NULL DEFAULT 0;

ALTER TABLE `subscriptions`
ADD COLUMN `static_aggression` smallint(6) NOT NULL DEFAULT 3;

ALTER TABLE `user_rp_statistics`
ADD COLUMN `passive_mode` tinyint(4) NOT NULL DEFAULT 0;

-- chat bubbles
DROP TABLE IF EXISTS `user_chat_bubbles`;
CREATE TABLE `user_chat_bubbles` (
  `id` int NOT NULL AUTO_INCREMENT,
  `chat_bubble_id` int NOT NULL,
  `user_id` int NOT NULL,
  `created_at` datetime NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `uniq_user_bubble` (`user_id`, `chat_bubble_id`),
  KEY `idx_user_chat_bubbles_user_id` (`user_id`),
  CONSTRAINT `fk_user_chat_bubbles_user`
    FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci ROW_FORMAT=Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
