using System;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class StopWorkCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_stop_work";

        public string Parameters => "";

        public string Description => "Stop your work shift in the corporation.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            // 5s cooldown shared with :startwork so the shift can't be toggle-spammed.
            double now = PlusEnvironment.GetUnixTimestamp();
            if (session.GetHabbo().LastWorkActionTime > now) {
                session.SendWhisper("You must wait " + (int)Math.Ceiling(session.GetHabbo().LastWorkActionTime - now) + " seconds before changing your work status again.", 1);
                return;
            }
            session.GetHabbo().LastWorkActionTime = now + 5;

            PlusEnvironment.GetGame().GetShiftManager().TryStopWork(session, room);
        }
    }
}