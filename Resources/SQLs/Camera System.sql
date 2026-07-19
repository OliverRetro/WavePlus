
INSERT INTO `server_settings` (`key`, `value`, `description`) VALUES
('camera.enabled',               '1',                                   'Master switch for the in-game camera (1/0).'),
('camera.price.credits',         '3',                                  'Credits charged when buying a photo as a furni item.'),
('camera.price.credits.publish', '10',                                  'Credits charged when publishing a photo to the website.'),
('camera.publish.cooldown',      '60',                                  'Seconds a user must wait between website publishes.'),
('camera.render.cooldown.ms',    '2000',                                'Minimum milliseconds between photo/thumbnail uploads per user.'),
('camera.photo.maxbytes',        '320000',                              'Maximum accepted PNG upload size in bytes (hard cap 450000).'),
('camera.photo.maxdimension',    '1024',                                'Maximum accepted PNG width/height in pixels.'),
('camera.photo.size',            '320',                                 'Exact width/height photos must have (client captures this size).'),
('camera.purchase.cooldown.ms',  '1000',                                'Minimum milliseconds between photo purchases per user.'),
('camera.path',                  'C:/xampp/htdocs/camera',                'Directory photos are written to (path of your choosing).'),
('camera.thumbnail.path',        'C:/xampp/htdocs/camera/thumbnails',     'Directory room thumbnails are written to (thumbnail_<roomid>.png).'),
('camera.url',                   'http://127.0.0.1/camera',             'Public base url the photo directory is served from (no trailing slash). Must match the Nitro config key camera.url.')
ON DUPLICATE KEY UPDATE `key` = `key`;

CREATE TABLE IF NOT EXISTS `camera_photos` (
    `id`        INT(11) NOT NULL AUTO_INCREMENT,
    `user_id`   INT(11) NOT NULL,
    `room_id`   INT(11) NOT NULL DEFAULT 0,
    `timestamp` INT(11) NOT NULL DEFAULT 0,
    `url`       VARCHAR(255) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `camera_web` (
    `id`        INT(11) NOT NULL AUTO_INCREMENT,
    `user_id`   INT(11) NOT NULL,
    `room_id`   INT(11) NOT NULL DEFAULT 0,
    `timestamp` INT(11) NOT NULL DEFAULT 0,
    `url`       VARCHAR(255) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- INSERT INTO `items_base` (`id`, `sprite_id`, `item_name`, `public_name`, `type`, `interaction_type`, `interaction_modes_count`)
-- VALUES (4597, 4597, 'external_image_wallitem_poster_small', 'Photo', 'i', 'camera_picture', 1);
-- UPDATE `server_settings` SET `value` = '4597' WHERE `key` = 'camera.item.id';
