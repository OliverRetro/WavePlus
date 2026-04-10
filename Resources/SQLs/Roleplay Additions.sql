SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

INSERT INTO `permissions_commands` (`command`) VALUES ('command_rp_user_info');
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_master_edit_rp', 6);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_room_heal', 5);
INSERT INTO `permissions_commands` (`command`, `group_id`) VALUES ('command_global_heal', 6);

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


SET FOREIGN_KEY_CHECKS = 1;
