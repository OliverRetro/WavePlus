ALTER TABLE `bots`
ADD COLUMN `effect_id` int NOT NULL DEFAULT 0;

CREATE TABLE `room_taxi_points` (
  `id` int NOT NULL AUTO_INCREMENT,
  `room_id` int NOT NULL,
  `x` int NOT NULL,
  `y` int NOT NULL,
  `z` double NOT NULL DEFAULT 0,
  `rotation` int NOT NULL DEFAULT 0,
  `cost` int NOT NULL DEFAULT 0,
  `delay` int NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  KEY `room_id` (`room_id`)
);

INSERT INTO `server_settings` (`key`, `value`, `description`)
VALUES ('rp.taxi.bots', '1:50;2:49;3:1', 'Taxi bot ids with probability weights, formatted as botId:weight;botId:weight');
