SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

INSERT INTO `permissions_commands` (`command`) VALUES ('command_rp_user_info');
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_master_edit_rp', 6);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_room_heal', 5);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_global_heal', 6);
INSERT INTO `permissions_commands` (`command`) VALUES ('command_rp_toggle_skill');
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_rp_hit', 1);

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


SET FOREIGN_KEY_CHECKS = 1;
