-- Combat range rework: tile-based ranges + per-weapon diagonal permission.
--
-- `range` is now interpreted in TILES (Chebyshev rings around the attacker):
--   range 1 = same tile as the target or any adjacent tile
--   range 2 = the above, plus one full tile of gap between the two users
--   range N = up to N tiles of separation
--
-- `allow_diagonal` controls whether the weapon can hit across diagonals:
--   1 = any direction within range counts (default; fists behave like this)
--   0 = the target must be on the same row or column as the attacker

ALTER TABLE `rp_weapons`
ADD COLUMN `allow_diagonal` tinyint(1) NOT NULL DEFAULT '1' AFTER `range`;

-- Existing rows keep working: ranges below 1 are clamped to 1 in code, and every
-- weapon defaults to allowing diagonals until tuned per weapon.
UPDATE `rp_weapons` SET `range` = 1 WHERE `range` < 1;
