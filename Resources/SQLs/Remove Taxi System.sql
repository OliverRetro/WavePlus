-- Remove the taxi system (WavePlus).
-- NOTE: `bots`.`effect_id` is intentionally NOT dropped — it was added by the
-- taxi migration but is now used by general room bots (PlaceBotEvent, Room bot load).

-- 1. Taxi pickup/dropoff points table.
DROP TABLE IF EXISTS `room_taxi_points`;

-- 2. Taxi bot probability setting.
DELETE FROM `server_settings` WHERE `key` = 'rp.taxi.bots';

-- 3. :taxi command permission mapping (command_room is shared — do NOT delete it).
DELETE FROM `permissions_commands` WHERE `command` = 'command_rp_taxi';
