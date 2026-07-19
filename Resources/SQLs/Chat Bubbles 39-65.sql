-- =====================================================================
-- Chat bubble pack 39-65
-- Registers the bubbles shipped in WaveRP-Nitro
-- (src/assets/images/chat/chatbubbles/bubble_39..65).
--
-- The emulator validates every bubble id against this table (see
-- ChatStyleManager / :chatbubble), so a bubble that is styled in the
-- client but missing here can never be granted or used.
--
-- required_right is left empty = usable by everyone. Set it to a
-- permission (e.g. 'mod_tool') to gate a bubble to staff.
--
-- Re-runnable: existing rows are left untouched.
-- =====================================================================

INSERT INTO `room_chat_styles` (`id`, `name`, `required_right`) VALUES
('39', 'bubble_39', ''),
('40', 'bubble_40', ''),
('41', 'bubble_41', ''),
('42', 'bubble_42', ''),
('43', 'bubble_43', ''),
('44', 'bubble_44', ''),
('45', 'bubble_45', ''),
('46', 'bubble_46', ''),
('47', 'bubble_47', ''),
('48', 'bubble_48', ''),
('49', 'bubble_49', ''),
('50', 'bubble_50', ''),
('51', 'bubble_51', ''),
('52', 'bubble_52', ''),
('53', 'bubble_53', ''),
('54', 'bubble_54', ''),
('55', 'bubble_55', ''),
('56', 'bubble_56', ''),
('57', 'bubble_57', ''),
('58', 'bubble_58', ''),
('59', 'bubble_59', ''),
('60', 'bubble_60', ''),
('61', 'bubble_61', ''),
('62', 'bubble_62', ''),
('63', 'bubble_63', ''),
('64', 'bubble_64', ''),
('65', 'bubble_65', '')
ON DUPLICATE KEY UPDATE `id` = `id`;
