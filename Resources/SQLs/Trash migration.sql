ALTER TABLE `rp_trash_rewards`
  ADD COLUMN `reward_type` tinyint(4) NOT NULL DEFAULT 0 COMMENT '0 = rp_items id, 1 = weapon id' AFTER `id`,
  MODIFY COLUMN `item_id` int NOT NULL COMMENT 'rp_items id or weapon id, per reward_type';

DELETE FROM `server_settings` WHERE `key` = 'rp.trash.search.duration.seconds';
