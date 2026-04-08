/*
 Navicat Premium Data Transfer

 Source Server         : mysql
 Source Server Type    : MySQL
 Source Server Version : 101111
 Source Host           : localhost:3306
 Source Schema         : plus

 Target Server Type    : MySQL
 Target Server Version : 101111
 File Encoding         : 65001

 Date: 07/04/2026 10:16:49
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for user_rp_statistics
-- ----------------------------
DROP TABLE IF EXISTS `user_rp_statistics`;
CREATE TABLE `user_rp_statistics`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL DEFAULT 0,
  `health` int NOT NULL DEFAULT 100,
  `energy` int NOT NULL DEFAULT 100,
  `hunger` int NOT NULL DEFAULT 50,
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

-- ----------------------------
-- Records of user_rp_statistics
-- ----------------------------

SET FOREIGN_KEY_CHECKS = 1;
