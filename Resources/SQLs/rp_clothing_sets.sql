/*
 Navicat Premium Data Transfer

 Source Server         : wave-ubuntu
 Source Server Type    : MySQL
 Source Server Version : 80410
 Source Host           : 169.58.63.4:7184
 Source Schema         : waveplus

 Target Server Type    : MySQL
 Target Server Version : 80410
 File Encoding         : 65001

 Date: 10/08/2026 20:12:10
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for rp_clothing_sets
-- ----------------------------
DROP TABLE IF EXISTS `rp_clothing_sets`;
CREATE TABLE `rp_clothing_sets`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `tab_id` smallint NULL DEFAULT NULL,
  `set_type` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Example pt for pets.',
  `part_types` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'm,f',
  `color` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `name` varchar(80) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `stock` int NOT NULL DEFAULT 0,
  `base_price` int NOT NULL DEFAULT 0,
  `discount_price` int NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of rp_clothing_sets
-- ----------------------------

SET FOREIGN_KEY_CHECKS = 1;
