using System;
using System.Data;

namespace Plus.HabboHotel.Users.Authenticator
{
    public static class HabboFactory
    {
        public static Habbo GenerateHabbo(DataRow row, DataRow userInfo)
        {
            Habbo habbo = new Habbo(Convert.ToInt32(row["id"]), Convert.ToString(row["username"]), Convert.ToInt32(row["rank"]), Convert.ToString(row["motto"]), Convert.ToString(row["look"]),
                Convert.ToString(row["gender"]), Convert.ToInt32(row["credits"]), Convert.ToInt32(row["activity_points"]),
                Convert.ToInt32(row["home_room"]), PlusEnvironment.EnumToBool(row["block_newfriends"].ToString()), Convert.ToInt32(row["last_online"]),
                PlusEnvironment.EnumToBool(row["hide_online"].ToString()), PlusEnvironment.EnumToBool(row["hide_inroom"].ToString()),
                Convert.ToDouble(row["account_created"]), Convert.ToInt32(row["vip_points"]), Convert.ToString(row["machine_id"]), Convert.ToString(row["volume"]),
                PlusEnvironment.EnumToBool(row["chat_preference"].ToString()), PlusEnvironment.EnumToBool(row["focus_preference"].ToString()), PlusEnvironment.EnumToBool(row["pets_muted"].ToString()), PlusEnvironment.EnumToBool(row["bots_muted"].ToString()),
                PlusEnvironment.EnumToBool(row["advertising_report_blocked"].ToString()), Convert.ToDouble(row["last_change"].ToString()), Convert.ToInt32(row["gotw_points"]),
                PlusEnvironment.EnumToBool(Convert.ToString(row["ignore_invites"])), Convert.ToDouble(row["time_muted"]), Convert.ToDouble(userInfo["trading_locked"]),
                PlusEnvironment.EnumToBool(row["allow_gifts"].ToString()), Convert.ToInt32(row["friend_bar_state"]), PlusEnvironment.EnumToBool(row["disable_forced_effects"].ToString()),
                PlusEnvironment.EnumToBool(row["allow_mimic"].ToString()), Convert.ToInt32(row["rank_vip"]));

                habbo.LoadHomeRoomData(Convert.ToString(row["home_room_data"]));

            return habbo;
        }
    }
}
