using System;
using Plus.Database.EF.Entities;

namespace Plus.HabboHotel.Users.Authenticator
{
    public static class HabboFactory
    {
        public static Habbo GenerateHabbo(UserEntity row, UserInfoEntity userInfo)
        {
            Habbo habbo = new Habbo(row.Id, row.Username, (int)(row.Rank ?? 0), row.Motto, row.Look,
                row.Gender, row.Credits ?? 0, row.ActivityPoints ?? 0,
                row.HomeRoom ?? 0, PlusEnvironment.EnumToBool(row.BlockNewfriends), row.LastOnline ?? 0,
                PlusEnvironment.EnumToBool(row.HideOnline), PlusEnvironment.EnumToBool(row.HideInroom),
                Convert.ToDouble(row.AccountCreated), row.VipPoints ?? 0, row.MachineId, row.Volume,
                PlusEnvironment.EnumToBool(row.ChatPreference), PlusEnvironment.EnumToBool(row.FocusPreference), PlusEnvironment.EnumToBool(row.PetsMuted), PlusEnvironment.EnumToBool(row.BotsMuted),
                PlusEnvironment.EnumToBool(row.AdvertisingReportBlocked), (double)(row.LastChange ?? 0), row.GotwPoints ?? 0,
                PlusEnvironment.EnumToBool(row.IgnoreInvites), row.TimeMuted ?? 0, userInfo.TradingLocked,
                PlusEnvironment.EnumToBool(row.AllowGifts), (row.FriendBarState ?? false) ? 1 : 0, PlusEnvironment.EnumToBool(row.DisableForcedEffects),
                PlusEnvironment.EnumToBool(row.AllowMimic), row.RankVip ?? 0, row.VipExpire ?? 0, row.Bubble);

            habbo.LoadHomeRoomData(row.HomeRoomData);

            return habbo;
        }
    }
}