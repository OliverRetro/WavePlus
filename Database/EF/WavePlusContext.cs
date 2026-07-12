using Microsoft.EntityFrameworkCore;
using Plus.Database.EF.Entities;

namespace Plus.Database.EF;

public partial class WavePlusContext : DbContext
{
    public WavePlusContext(DbContextOptions<WavePlusContext> options)
        : base(options)
    {
    }

    public virtual DbSet<achievement> achievements { get; set; }

    public virtual DbSet<achievements_talent> achievements_talents { get; set; }

    public virtual DbSet<badge_definition> badge_definitions { get; set; }

    public virtual DbSet<ban> bans { get; set; }

    public virtual DbSet<bot> bots { get; set; }

    public virtual DbSet<bots_pet_command> bots_pet_commands { get; set; }

    public virtual DbSet<bots_pet_response> bots_pet_responses { get; set; }

    public virtual DbSet<bots_petdatum> bots_petdata { get; set; }

    public virtual DbSet<bots_response> bots_responses { get; set; }

    public virtual DbSet<bots_speech> bots_speeches { get; set; }

    public virtual DbSet<catalog_bot_preset> catalog_bot_presets { get; set; }

    public virtual DbSet<catalog_clothing> catalog_clothings { get; set; }

    public virtual DbSet<catalog_deal> catalog_deals { get; set; }

    public virtual DbSet<catalog_item> catalog_items { get; set; }

    public virtual DbSet<catalog_marketplace_datum> catalog_marketplace_data { get; set; }

    public virtual DbSet<catalog_marketplace_offer> catalog_marketplace_offers { get; set; }

    public virtual DbSet<catalog_page> catalog_pages { get; set; }

    public virtual DbSet<catalog_pet_race> catalog_pet_races { get; set; }

    public virtual DbSet<catalog_promotion> catalog_promotions { get; set; }

    public virtual DbSet<catalog_voucher> catalog_vouchers { get; set; }

    public virtual DbSet<chatlog> chatlogs { get; set; }

    public virtual DbSet<chatlogs_console> chatlogs_consoles { get; set; }

    public virtual DbSet<chatlogs_console_invitation> chatlogs_console_invitations { get; set; }

    public virtual DbSet<client_external_badge_text> client_external_badge_texts { get; set; }

    public virtual DbSet<client_external_text> client_external_texts { get; set; }

    public virtual DbSet<furniture> furnitures { get; set; }

    public virtual DbSet<furniture_copy1> furniture_copy1s { get; set; }

    public virtual DbSet<games_config> games_configs { get; set; }

    public virtual DbSet<group> groups { get; set; }

    public virtual DbSet<group_membership> group_memberships { get; set; }

    public virtual DbSet<group_permission> group_permissions { get; set; }

    public virtual DbSet<group_permission_key> group_permission_keys { get; set; }

    public virtual DbSet<group_request> group_requests { get; set; }

    public virtual DbSet<group_role> group_roles { get; set; }

    public virtual DbSet<groups_item> groups_items { get; set; }

    public virtual DbSet<item> items { get; set; }

    public virtual DbSet<items_group> items_groups { get; set; }

    public virtual DbSet<items_youtube> items_youtubes { get; set; }

    public virtual DbSet<logs_client_namechange> logs_client_namechanges { get; set; }

    public virtual DbSet<logs_client_staff> logs_client_staffs { get; set; }

    public virtual DbSet<logs_client_trade> logs_client_trades { get; set; }

    public virtual DbSet<messenger_friendship> messenger_friendships { get; set; }

    public virtual DbSet<messenger_offline_message> messenger_offline_messages { get; set; }

    public virtual DbSet<messenger_request> messenger_requests { get; set; }

    public virtual DbSet<moderation_preset> moderation_presets { get; set; }

    public virtual DbSet<moderation_preset_action_category> moderation_preset_action_categories { get; set; }

    public virtual DbSet<moderation_preset_action_message> moderation_preset_action_messages { get; set; }

    public virtual DbSet<moderation_ticket> moderation_tickets { get; set; }

    public virtual DbSet<moderation_topic> moderation_topics { get; set; }

    public virtual DbSet<moderation_topic_action> moderation_topic_actions { get; set; }

    public virtual DbSet<navigator_category> navigator_categories { get; set; }

    public virtual DbSet<navigator_public> navigator_publics { get; set; }

    public virtual DbSet<permission> permissions { get; set; }

    public virtual DbSet<permissions_command> permissions_commands { get; set; }

    public virtual DbSet<permissions_group> permissions_groups { get; set; }

    public virtual DbSet<permissions_right> permissions_rights { get; set; }

    public virtual DbSet<permissions_subscription> permissions_subscriptions { get; set; }

    public virtual DbSet<quest> quests { get; set; }

    public virtual DbSet<rank> ranks { get; set; }

    public virtual DbSet<room> rooms { get; set; }

    public virtual DbSet<room_ban> room_bans { get; set; }

    public virtual DbSet<room_chat_style> room_chat_styles { get; set; }

    public virtual DbSet<room_filter> room_filters { get; set; }

    public virtual DbSet<room_items_moodlight> room_items_moodlights { get; set; }

    public virtual DbSet<room_items_tele_link> room_items_tele_links { get; set; }

    public virtual DbSet<room_items_toner> room_items_toners { get; set; }

    public virtual DbSet<room_model> room_models { get; set; }

    public virtual DbSet<room_promotion> room_promotions { get; set; }

    public virtual DbSet<room_right> room_rights { get; set; }

    public virtual DbSet<room_taxi_point> room_taxi_points { get; set; }

    public virtual DbSet<rp_crime> rp_crimes { get; set; }

    public virtual DbSet<rp_crime_log> rp_crime_logs { get; set; }

    public virtual DbSet<rp_crime_penalty> rp_crime_penalties { get; set; }

    public virtual DbSet<rp_item> rp_items { get; set; }

    public virtual DbSet<rp_shift_log> rp_shift_logs { get; set; }

    public virtual DbSet<rp_skill> rp_skills { get; set; }

    public virtual DbSet<rp_skills_level> rp_skills_levels { get; set; }

    public virtual DbSet<rp_trash_reward> rp_trash_rewards { get; set; }

    public virtual DbSet<rp_weapon> rp_weapons { get; set; }

    public virtual DbSet<rp_weapon_skin> rp_weapon_skins { get; set; }

    public virtual DbSet<server_landing> server_landings { get; set; }

    public virtual DbSet<server_locale> server_locales { get; set; }

    public virtual DbSet<server_reward> server_rewards { get; set; }

    public virtual DbSet<server_reward_log> server_reward_logs { get; set; }

    public virtual DbSet<server_setting> server_settings { get; set; }

    public virtual DbSet<server_status> server_statuses { get; set; }

    public virtual DbSet<subscription> subscriptions { get; set; }

    public virtual DbSet<talent> talents { get; set; }

    public virtual DbSet<talents_sub_level> talents_sub_levels { get; set; }

    public virtual DbSet<user> users { get; set; }

    public virtual DbSet<user_achievement> user_achievements { get; set; }

    public virtual DbSet<user_badge> user_badges { get; set; }

    public virtual DbSet<user_chat_bubble> user_chat_bubbles { get; set; }

    public virtual DbSet<user_clothing> user_clothings { get; set; }

    public virtual DbSet<user_effect> user_effects { get; set; }

    public virtual DbSet<user_favorite> user_favorites { get; set; }

    public virtual DbSet<user_ignore> user_ignores { get; set; }

    public virtual DbSet<user_info> user_infos { get; set; }

    public virtual DbSet<user_present> user_presents { get; set; }

    public virtual DbSet<user_quest> user_quests { get; set; }

    public virtual DbSet<user_relationship> user_relationships { get; set; }

    public virtual DbSet<user_roomvisit> user_roomvisits { get; set; }

    public virtual DbSet<user_rp_bank> user_rp_banks { get; set; }

    public virtual DbSet<user_rp_bank_log> user_rp_bank_logs { get; set; }

    public virtual DbSet<user_rp_item> user_rp_items { get; set; }

    public virtual DbSet<user_rp_skill> user_rp_skills { get; set; }

    public virtual DbSet<user_rp_statistic> user_rp_statistics { get; set; }

    public virtual DbSet<user_rp_weapon> user_rp_weapons { get; set; }

    public virtual DbSet<user_rp_weapon_skin> user_rp_weapon_skins { get; set; }

    public virtual DbSet<user_saved_search> user_saved_searches { get; set; }

    public virtual DbSet<user_stat> user_stats { get; set; }

    public virtual DbSet<user_voucher> user_vouchers { get; set; }

    public virtual DbSet<user_wardrobe> user_wardrobes { get; set; }

    public virtual DbSet<users_macro> users_macros { get; set; }

    public virtual DbSet<users_macro_setting> users_macro_settings { get; set; }

    public virtual DbSet<wired_item> wired_items { get; set; }

    public virtual DbSet<wordfilter> wordfilters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<achievement>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.category).HasDefaultValueSql("'''identity'''");
            entity.Property(e => e.group_name).HasDefaultValueSql("'''ACH_'''");
            entity.Property(e => e.level).HasDefaultValueSql("'1'");
            entity.Property(e => e.progress_needed).HasDefaultValueSql("'1'");
            entity.Property(e => e.reward_pixels).HasDefaultValueSql("'5'");
            entity.Property(e => e.reward_points).HasDefaultValueSql("'5'");
        });

        modelBuilder.Entity<achievements_talent>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.achievement_group).HasDefaultValueSql("'''ACH_'''");
            entity.Property(e => e.achievement_level).HasDefaultValueSql("'1'");
            entity.Property(e => e.parent_category).HasDefaultValueSql("'-1'");
            entity.Property(e => e.prize).HasDefaultValueSql("'''A1 KUMIANKKA'''");
            entity.Property(e => e.type).HasDefaultValueSql("'''citizenship'''");
        });

        modelBuilder.Entity<badge_definition>(entity =>
        {
            entity.HasKey(e => e.code).HasName("PRIMARY");

            entity.Property(e => e.required_right).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<ban>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.appeal_state).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.bantype).HasDefaultValueSql("'''user'''");
        });

        modelBuilder.Entity<bot>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.ai_type).HasDefaultValueSql("'''generic'''");
            entity.Property(e => e.automatic_chat).HasDefaultValueSql("'''false'''");
            entity.Property(e => e.chat_bubble).HasDefaultValueSql("'2'");
            entity.Property(e => e.gender).HasDefaultValueSql("'''M'''");
            entity.Property(e => e.mix_sentences).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.speaking_interval).HasDefaultValueSql("'30'");
            entity.Property(e => e.walk_mode).HasDefaultValueSql("'''freeroam'''");
        });

        modelBuilder.Entity<bots_pet_command>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.input).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<bots_pet_response>(entity =>
        {
            entity.HasKey(e => e.pet_id).HasName("PRIMARY");
        });

        modelBuilder.Entity<bots_petdatum>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.anyone_ride).HasDefaultValueSql("'0'");
            entity.Property(e => e.color).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.createstamp).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.energy).HasDefaultValueSql("'0'");
            entity.Property(e => e.experience).HasDefaultValueSql("'0'");
            entity.Property(e => e.gnome_clothing).HasDefaultValueSql("'''-1'''");
            entity.Property(e => e.hairdye).HasDefaultValueSql("'1'");
            entity.Property(e => e.have_saddle).HasDefaultValueSql("'0'");
            entity.Property(e => e.nutrition).HasDefaultValueSql("'0'");
            entity.Property(e => e.pethair).HasDefaultValueSql("'-1'");
            entity.Property(e => e.race).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.respect).HasDefaultValueSql("'0'");
            entity.Property(e => e.type).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<bots_response>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.bot_ai).HasDefaultValueSql("'''generic'''");
            entity.Property(e => e.response_beverage).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.response_mode).HasDefaultValueSql("'''say'''");
            entity.Property(e => e.response_text).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<bots_speech>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.shout).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.type).HasDefaultValueSql("'''normal'''");
        });

        modelBuilder.Entity<catalog_bot_preset>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.ai_type).HasDefaultValueSql("'''generic'''");
        });

        modelBuilder.Entity<catalog_clothing>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.clothing_name).HasDefaultValueSql("''''''");
            entity.Property(e => e.clothing_parts).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<catalog_deal>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<catalog_item>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.amount).HasDefaultValueSql("'1'");
            entity.Property(e => e.badge).HasDefaultValueSql("''''''");
            entity.Property(e => e.cost_credits).HasDefaultValueSql("'3'");
            entity.Property(e => e.extradata).HasDefaultValueSql("''''''");
            entity.Property(e => e.offer_active).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.offer_id).HasDefaultValueSql("'-1'");
            entity.Property(e => e.points_type).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<catalog_marketplace_datum>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<catalog_marketplace_offer>(entity =>
        {
            entity.HasKey(e => e.offer_id).HasName("PRIMARY");

            entity.Property(e => e.item_type).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.state).HasDefaultValueSql("'''1'''");
        });

        modelBuilder.Entity<catalog_page>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.caption).HasDefaultValueSql("'''noname'''");
            entity.Property(e => e.enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.icon_image).HasDefaultValueSql("'1'");
            entity.Property(e => e.min_rank).HasDefaultValueSql("'1'");
            entity.Property(e => e.order_num).HasDefaultValueSql("'69'");
            entity.Property(e => e.page_layout).HasDefaultValueSql("'''default_3x3'''");
            entity.Property(e => e.page_link).HasDefaultValueSql("''''''");
            entity.Property(e => e.page_strings_1).HasDefaultValueSql("''''''");
            entity.Property(e => e.page_strings_2).HasDefaultValueSql("''''''");
            entity.Property(e => e.parent_id).HasDefaultValueSql("'-1'");
            entity.Property(e => e.visible).HasDefaultValueSql("'''1'''");
        });

        modelBuilder.Entity<catalog_pet_race>(entity =>
        {
            entity.Property(e => e.color1).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.color2).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.has1color).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.has2color).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.raceid).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<catalog_promotion>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.image).HasDefaultValueSql("''''''");
            entity.Property(e => e.page_link).HasDefaultValueSql("''''''");
            entity.Property(e => e.parent_id).HasDefaultValueSql("'0'");
            entity.Property(e => e.title).HasDefaultValueSql("''''''");
            entity.Property(e => e.unknown).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<catalog_voucher>(entity =>
        {
            entity.HasKey(e => e.voucher).HasName("PRIMARY");

            entity.Property(e => e.enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.max_uses).HasDefaultValueSql("'1'");
            entity.Property(e => e.type).HasDefaultValueSql("'''credits'''");
            entity.Property(e => e.value).HasDefaultValueSql("'100'");
        });

        modelBuilder.Entity<chatlog>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<chatlogs_console>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<chatlogs_console_invitation>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<client_external_badge_text>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.badge_code).HasDefaultValueSql("''''''");
            entity.Property(e => e.badge_desc).HasDefaultValueSql("''''''");
            entity.Property(e => e.badge_title).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<client_external_text>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<furniture>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.allow_gift).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.allow_inventory_stack).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.allow_lay).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.allow_marketplace_sell).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.allow_recycle).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.allow_trade).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.can_stack).HasDefaultValueSql("'1'");
            entity.Property(e => e.extra_rot).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.height_adjustable).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.interaction_modes_count).HasDefaultValueSql("'1'");
            entity.Property(e => e.interaction_type).HasDefaultValueSql("'''default'''");
            entity.Property(e => e.is_rare).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.is_walkable).HasDefaultValueSql("'0'");
            entity.Property(e => e.length).HasDefaultValueSql("'1'");
            entity.Property(e => e.public_name).HasDefaultValueSql("''''''");
            entity.Property(e => e.sprite_id).HasDefaultValueSql("'0'");
            entity.Property(e => e.type).HasDefaultValueSql("'''s'''");
            entity.Property(e => e.vending_ids).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.width).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<furniture_copy1>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.allow_gift).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.allow_inventory_stack).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.allow_marketplace_sell).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.allow_recycle).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.allow_trade).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.can_sit).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.can_stack).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.extra_rot).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.height_adjustable).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.interaction_modes_count).HasDefaultValueSql("'1'");
            entity.Property(e => e.interaction_type).HasDefaultValueSql("'''default'''");
            entity.Property(e => e.is_rare).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.is_walkable).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.length).HasDefaultValueSql("'1'");
            entity.Property(e => e.public_name).HasDefaultValueSql("''''''");
            entity.Property(e => e.type).HasDefaultValueSql("'''s'''");
            entity.Property(e => e.vending_ids).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.width).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<games_config>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.colour_one).HasDefaultValueSql("''''''");
            entity.Property(e => e.colour_two).HasDefaultValueSql("''''''");
            entity.Property(e => e.game_assets).HasDefaultValueSql("''''''");
            entity.Property(e => e.game_enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.game_server_host).HasDefaultValueSql("''''''");
            entity.Property(e => e.game_server_port).HasDefaultValueSql("''''''");
            entity.Property(e => e.game_swf).HasDefaultValueSql("''''''");
            entity.Property(e => e.last_reset).HasDefaultValueSql("'0'");
            entity.Property(e => e.name).HasDefaultValueSql("''''''");
            entity.Property(e => e.socket_policy_port).HasDefaultValueSql("''''''");
            entity.Property(e => e.string_three).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<group>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.admindeco).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.colour1).HasDefaultValueSql("'242424'");
            entity.Property(e => e.colour2).HasDefaultValueSql("'242424'");
            entity.Property(e => e.forum_enabled).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.state).HasDefaultValueSql("'''0'''");
        });

        modelBuilder.Entity<group_membership>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.level).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<group_permission>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.group_id).HasComment("groups.id");
            entity.Property(e => e.level_id).HasComment("groups_roles.level");
            entity.Property(e => e.permission_id).HasComment("group_permissiosn_keys.id");
            entity.Property(e => e.updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<group_permission_key>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.description).HasDefaultValueSql("'''nodesc'''");
        });

        modelBuilder.Entity<group_role>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.created_at).HasDefaultValueSql("'''0000-00-00 00:00:00'''");
            entity.Property(e => e.group_id).HasComment("Group's ID.");
            entity.Property(e => e.level)
                .HasDefaultValueSql("'1'")
                .HasComment("Levels go in order of Slave -> CEO. 1 = Slave, 100 = CEO.");
            entity.Property(e => e.name)
                .HasDefaultValueSql("''''''")
                .HasComment("Literal name displayed; ex. general manager");
            entity.Property(e => e.shift_costume)
                .HasDefaultValueSql("''''''")
                .HasComment("Shift-Costume (full) to replace users own clothes. Blank for no swap.");
            entity.Property(e => e.shift_duration)
                .HasDefaultValueSql("'5'")
                .HasComment("The duration at which worker will be auto-rewarded. In minutes.");
            entity.Property(e => e.shift_motto)
                .HasDefaultValueSql("''''''")
                .HasComment("Replace users motto for the duration of shift? Blank for no swap.");
            entity.Property(e => e.shift_pay).HasComment("Credits paid via intervals.");
            entity.Property(e => e.updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<groups_item>(entity =>
        {
            entity.HasKey(e => new { e.id, e.type }).HasName("PRIMARY");

            entity.Property(e => e.enabled).HasDefaultValueSql("'''1'''");
        });

        modelBuilder.Entity<item>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.limited_number).HasDefaultValueSql("'0'");
            entity.Property(e => e.limited_stack).HasDefaultValueSql("'0'");
            entity.Property(e => e.wall_pos).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<items_group>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<items_youtube>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.description).HasDefaultValueSql("''''''");
            entity.Property(e => e.enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.title).HasDefaultValueSql("''''''");
            entity.Property(e => e.youtube_id).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<logs_client_namechange>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.new_name).HasDefaultValueSql("''''''");
            entity.Property(e => e.old_name).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<logs_client_staff>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.machine_id).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<logs_client_trade>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e._1id).HasDefaultValueSql("'0'");
            entity.Property(e => e._1items).HasDefaultValueSql("'NULL'");
            entity.Property(e => e._2id).HasDefaultValueSql("'0'");
            entity.Property(e => e._2items).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.timestamp)
                .HasDefaultValueSql("''''''")
                .IsFixedLength();
        });

        modelBuilder.Entity<messenger_friendship>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<messenger_offline_message>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<messenger_request>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<moderation_preset>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.enabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.type).HasDefaultValueSql("'''user'''");
        });

        modelBuilder.Entity<moderation_preset_action_category>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.caption).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<moderation_preset_action_message>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.ban_hours).HasDefaultValueSql("'24'");
        });

        modelBuilder.Entity<moderation_ticket>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.status).HasDefaultValueSql("'''open'''");
        });

        modelBuilder.Entity<moderation_topic>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.caption).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<moderation_topic_action>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.caption).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<navigator_category>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.category).HasDefaultValueSql("'''hotel_view'''");
            entity.Property(e => e.category_identifier).HasDefaultValueSql("''''''");
            entity.Property(e => e.category_type).HasDefaultValueSql("'''category'''");
            entity.Property(e => e.enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.public_name).HasDefaultValueSql("''''''");
            entity.Property(e => e.required_rank).HasDefaultValueSql("'1'");
            entity.Property(e => e.search_allowance).HasDefaultValueSql("'''SHOW_MORE'''");
            entity.Property(e => e.view_mode).HasDefaultValueSql("'''REGULAR'''");
        });

        modelBuilder.Entity<navigator_public>(entity =>
        {
            entity.HasKey(e => e.room_id).HasName("PRIMARY");

            entity.Property(e => e.enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.order_num).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<permission>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<permissions_command>(entity =>
        {
            entity.HasKey(e => e.command).HasName("PRIMARY");

            entity.Property(e => e.command).HasDefaultValueSql("''''''");
            entity.Property(e => e.group_id).HasDefaultValueSql("'4'");
        });

        modelBuilder.Entity<permissions_group>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.badge_code).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<permissions_right>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<permissions_subscription>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<quest>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.action).HasDefaultValueSql("''''''");
            entity.Property(e => e.data_bit).HasDefaultValueSql("''''''");
            entity.Property(e => e.pixel_reward).HasDefaultValueSql("'10'");
            entity.Property(e => e.reward_type).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.type).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<rank>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<room>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.allow_hidewall).HasDefaultValueSql("'1'");
            entity.Property(e => e.ban_settings).HasDefaultValueSql("'1'");
            entity.Property(e => e.caption).HasDefaultValueSql("'''Room'''");
            entity.Property(e => e.chat_hearing_distance).HasDefaultValueSql("'14'");
            entity.Property(e => e.description).HasDefaultValueSql("''''''");
            entity.Property(e => e.enables_enabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.floor).HasDefaultValueSql("'''0.0'''");
            entity.Property(e => e.kick_settings).HasDefaultValueSql("'1'");
            entity.Property(e => e.landscape).HasDefaultValueSql("'''0.0'''");
            entity.Property(e => e.lay_enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.mute_settings).HasDefaultValueSql("'1'");
            entity.Property(e => e.owner).HasDefaultValueSql("''''''");
            entity.Property(e => e.password).HasDefaultValueSql("''''''");
            entity.Property(e => e.pet_morphs_allowed).HasDefaultValueSql("'1'");
            entity.Property(e => e.pull_enabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.push_enabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.respect_notifications_enabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.roomtype).HasDefaultValueSql("'''private'''");
            entity.Property(e => e.safezone).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.spull_enabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.spush_enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.state).HasDefaultValueSql("'''open'''");
            entity.Property(e => e.tags).HasDefaultValueSql("''''''");
            entity.Property(e => e.taxi_x).HasDefaultValueSql("'-1'");
            entity.Property(e => e.taxi_y).HasDefaultValueSql("'-1'");
            entity.Property(e => e.trade_settings).HasDefaultValueSql("'2'");
            entity.Property(e => e.users_max).HasDefaultValueSql("'25'");
            entity.Property(e => e.wallpaper).HasDefaultValueSql("'''0.0'''");
        });

        modelBuilder.Entity<room_ban>(entity =>
        {
            entity.HasKey(e => new { e.user_id, e.room_id }).HasName("PRIMARY");
        });

        modelBuilder.Entity<room_chat_style>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.name).HasDefaultValueSql("''''''");
            entity.Property(e => e.required_right).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<room_filter>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.word).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<room_items_moodlight>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.enabled).HasDefaultValueSql("'''0'''");
        });

        modelBuilder.Entity<room_items_tele_link>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<room_items_toner>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.enabled).HasDefaultValueSql("'''0'''");
        });

        modelBuilder.Entity<room_model>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.club_only).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.custom).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.door_dir).HasDefaultValueSql("'2'");
            entity.Property(e => e.poolmap).HasDefaultValueSql("''''''");
            entity.Property(e => e.public_items).HasDefaultValueSql("''''''");
            entity.Property(e => e.wall_height).HasDefaultValueSql("'-1'");
        });

        modelBuilder.Entity<room_promotion>(entity =>
        {
            entity.HasKey(e => e.room_id).HasName("PRIMARY");

            entity.Property(e => e.description).HasDefaultValueSql("''''''");
            entity.Property(e => e.title).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<room_right>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<room_taxi_point>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<rp_crime>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.note).HasDefaultValueSql("''''''");
            entity.Property(e => e.severity).HasDefaultValueSql("'1'");
            entity.Property(e => e.time_active).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<rp_crime_log>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<rp_crime_penalty>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<rp_item>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.image_url).HasDefaultValueSql("''''''");
            entity.Property(e => e.item_type).HasDefaultValueSql("'''consumable'''");
            entity.Property(e => e.stack_limit).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<rp_shift_log>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<rp_skill>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.badge_code).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.description).HasDefaultValueSql("'''No description yet.'''");
            entity.Property(e => e.name).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<rp_skills_level>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.level).HasDefaultValueSql("'''1'''");
        });

        modelBuilder.Entity<rp_trash_reward>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.chance_weight).HasDefaultValueSql("'1'");
            entity.Property(e => e.enabled).HasDefaultValueSql("'1'");
            entity.Property(e => e.max_amount).HasDefaultValueSql("'1'");
            entity.Property(e => e.min_amount).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<rp_weapon>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.critical_chance)
                .HasDefaultValueSql("'10'")
                .HasComment("As %. For instance 50% is 50-50 on hitting it, without Skill boosts.");
            entity.Property(e => e.critical_hit_message).HasDefaultValueSql("'''*lands a critical hit on %username%, dealing %damage% damage*'''");
            entity.Property(e => e.description).HasDefaultValueSql("''''''");
            entity.Property(e => e.hit_message)
                .HasDefaultValueSql("'''*swings at %username%, dealing %damage% damage*'''")
                .HasComment("Variables available username and damage");
            entity.Property(e => e.image).HasDefaultValueSql("''''''");
            entity.Property(e => e.maximum_damage).HasDefaultValueSql("'1'");
            entity.Property(e => e.name).HasDefaultValueSql("''''''");
            entity.Property(e => e.stun_chance).HasComment("0-100");
        });

        modelBuilder.Entity<rp_weapon_skin>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.critical_hit_message).HasDefaultValueSql("'''*lands a critical hit on %username%, dealing %damage% damage*'''");
            entity.Property(e => e.hit_message).HasDefaultValueSql("'''*swings at %username%, dealing %damage% damage*'''");
        });

        modelBuilder.Entity<server_landing>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.button_link).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.button_text).HasDefaultValueSql("''''''");
            entity.Property(e => e.button_type).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.image_link).HasDefaultValueSql("''''''");
            entity.Property(e => e.text).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.title).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<server_locale>(entity =>
        {
            entity.HasKey(e => e.key).HasName("PRIMARY");

            entity.Property(e => e.value).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<server_reward>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.enabled).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.reward_type).HasDefaultValueSql("'''none'''");
        });

        modelBuilder.Entity<server_reward_log>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<server_setting>(entity =>
        {
            entity.HasKey(e => e.key).HasName("PRIMARY");

            entity.Property(e => e.key).HasDefaultValueSql("'''server.variable'''");
        });

        modelBuilder.Entity<subscription>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.credits).HasDefaultValueSql("'100'");
            entity.Property(e => e.duckets).HasDefaultValueSql("'100'");
            entity.Property(e => e.respects).HasDefaultValueSql("'3'");
            entity.Property(e => e.static_aggression).HasDefaultValueSql("'3'");
            entity.Property(e => e.trash_search_cooldown).HasDefaultValueSql("'60'");
        });

        modelBuilder.Entity<talent>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.level).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<talents_sub_level>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.badge_code).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => new { e.id, e.auth_ticket }).HasName("PRIMARY");

            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.account_created)
                .HasDefaultValueSql("'''0'''")
                .IsFixedLength();
            entity.Property(e => e.activity_points).HasDefaultValueSql("'5000'");
            entity.Property(e => e.advertising_report_blocked).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.allow_gifts).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.allow_mimic).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.block_newfriends).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.bots_muted).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.chat_preference).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.credits).HasDefaultValueSql("'50000'");
            entity.Property(e => e.disable_forced_effects).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.focus_preference).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.friend_bar_state).HasDefaultValueSql("'1'");
            entity.Property(e => e.gender).HasDefaultValueSql("'''M'''");
            entity.Property(e => e.gotw_points).HasDefaultValueSql("'0'");
            entity.Property(e => e.hide_inroom).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.hide_online).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.home_room).HasDefaultValueSql("'0'");
            entity.Property(e => e.home_room_data).HasDefaultValueSql("'''{\"roomid\":3,\"x\":18,\"y\":22,\"z\":0,\"rotation\":4,\"weapon\":0}'''");
            entity.Property(e => e.ignore_invites).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.ip_last).HasDefaultValueSql("''''''");
            entity.Property(e => e.ip_reg).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.is_muted).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.last_change).HasDefaultValueSql("'0'");
            entity.Property(e => e.last_online).HasDefaultValueSql("'0'");
            entity.Property(e => e.look)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
            entity.Property(e => e.machine_id).HasDefaultValueSql("''''''");
            entity.Property(e => e.mail).HasDefaultValueSql("'''defaultuser@meth0d.org'''");
            entity.Property(e => e.motto)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
            entity.Property(e => e.online).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.password).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.pets_muted).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.rank).HasDefaultValueSql("'1'");
            entity.Property(e => e.rank_vip).HasDefaultValueSql("'1'");
            entity.Property(e => e.time_muted).HasDefaultValueSql("'0'");
            entity.Property(e => e.trading_locked).HasDefaultValueSql("'0'");
            entity.Property(e => e.vip).HasDefaultValueSql("'''1'''");
            entity.Property(e => e.vip_points).HasDefaultValueSql("'0'");
            entity.Property(e => e.volume).HasDefaultValueSql("'''100,100,100'''");
        });

        modelBuilder.Entity<user_achievement>(entity =>
        {
            entity.HasKey(e => new { e.userid, e.group }).HasName("PRIMARY");
        });

        modelBuilder.Entity<user_badge>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<user_chat_bubble>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.created_at).HasDefaultValueSql("'current_timestamp()'");

            entity.HasOne(d => d.user).WithMany(p => p.user_chat_bubbles)
                .HasPrincipalKey(p => p.id)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("fk_user_chat_bubbles_user");
        });

        modelBuilder.Entity<user_clothing>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.part).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<user_effect>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.activated_stamp).HasDefaultValueSql("'0'");
            entity.Property(e => e.effect_id).HasDefaultValueSql("'1'");
            entity.Property(e => e.is_activated).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.quantity).HasDefaultValueSql("'0'");
            entity.Property(e => e.total_duration).HasDefaultValueSql("'3600'");
            entity.Property(e => e.user_id).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<user_favorite>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<user_ignore>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<user_info>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("PRIMARY");
        });

        modelBuilder.Entity<user_present>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<user_quest>(entity =>
        {
            entity.HasKey(e => new { e.user_id, e.quest_id }).HasName("PRIMARY");

            entity.Property(e => e.progress).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<user_relationship>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<user_roomvisit>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<user_rp_bank>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.account_opened)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("'''0000-00-00 00:00:00.000000'''");
        });

        modelBuilder.Entity<user_rp_bank_log>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.action_type).HasDefaultValueSql("'''DEPOSIT'''");
            entity.Property(e => e.management_type).HasDefaultValueSql("'''ATM'''");
            entity.Property(e => e.timestamp)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("'''0000-00-00 00:00:00.000000'''");
        });

        modelBuilder.Entity<user_rp_item>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.durability_left).HasDefaultValueSql("'-1'");
            entity.Property(e => e.slot).HasDefaultValueSql("'-1'");
        });

        modelBuilder.Entity<user_rp_skill>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<user_rp_statistic>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.energy).HasDefaultValueSql("'100'");
            entity.Property(e => e.health).HasDefaultValueSql("'100'");
            entity.Property(e => e.hunger).HasDefaultValueSql("'25'");
            entity.Property(e => e.jail_revert_look).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<user_rp_weapon>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.slot).HasDefaultValueSql("'-1'");
        });

        modelBuilder.Entity<user_rp_weapon_skin>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<user_saved_search>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.filter).HasDefaultValueSql("''''''");
            entity.Property(e => e.search_code).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<user_stat>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.DailyPetRespectPoints).HasDefaultValueSql("'3'");
            entity.Property(e => e.DailyRespectPoints).HasDefaultValueSql("'3'");
            entity.Property(e => e.respectsTimestamp).HasDefaultValueSql("'''10/19'''");
        });

        modelBuilder.Entity<user_voucher>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");
        });

        modelBuilder.Entity<user_wardrobe>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.gender).HasDefaultValueSql("'''M'''");
        });

        modelBuilder.Entity<users_macro>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e.configs).HasDefaultValueSql("''''''");
        });

        modelBuilder.Entity<users_macro_setting>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("PRIMARY");
        });

        modelBuilder.Entity<wired_item>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.Property(e => e._bool).HasDefaultValueSql("'''0'''");
        });

        modelBuilder.Entity<wordfilter>(entity =>
        {
            entity.HasKey(e => e.word).HasName("PRIMARY");

            entity.Property(e => e.addedby).HasDefaultValueSql("''''''");
            entity.Property(e => e.bannable).HasDefaultValueSql("'''0'''");
            entity.Property(e => e.replacement).HasDefaultValueSql("'''Habboon'''");
            entity.Property(e => e.strict).HasDefaultValueSql("'''1'''");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
