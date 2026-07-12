using System;
using Plus.Database.EF.Entities;

namespace Plus.HabboHotel.Users.Authenticator
{
    public static class HabboFactory
    {
        public static Habbo GenerateHabbo(user row, user_info userInfo)
        {
            Habbo habbo = new Habbo(row.id, row.username, (int)(row.rank ?? 0), row.motto, row.look,
                row.gender, row.credits ?? 0, row.activity_points ?? 0,
                row.home_room ?? 0, PlusEnvironment.EnumToBool(row.block_newfriends), row.last_online ?? 0,
                PlusEnvironment.EnumToBool(row.hide_online), PlusEnvironment.EnumToBool(row.hide_inroom),
                Convert.ToDouble(row.account_created), row.vip_points ?? 0, row.machine_id, row.volume,
                PlusEnvironment.EnumToBool(row.chat_preference), PlusEnvironment.EnumToBool(row.focus_preference), PlusEnvironment.EnumToBool(row.pets_muted), PlusEnvironment.EnumToBool(row.bots_muted),
                PlusEnvironment.EnumToBool(row.advertising_report_blocked), (double)(row.last_change ?? 0), row.gotw_points ?? 0,
                PlusEnvironment.EnumToBool(row.ignore_invites), row.time_muted ?? 0, userInfo.trading_locked,
                PlusEnvironment.EnumToBool(row.allow_gifts), (row.friend_bar_state ?? false) ? 1 : 0, PlusEnvironment.EnumToBool(row.disable_forced_effects),
                PlusEnvironment.EnumToBool(row.allow_mimic), row.rank_vip ?? 0, row.bubble);

            habbo.LoadHomeRoomData(row.home_room_data);

            return habbo;
        }
    }
}
