using System;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Police;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police
{
    internal class FineCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hit";
        public string Parameters => "%username% %amount%";
        public string Description => "Fine a user credits (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 3 || !int.TryParse(@params[2], out int amount) || amount <= 0) {
                session.SendWhisper("Usage: :fine <username> <amount>", 1);
                return;
            }

            if (!PoliceManager.HasJobPermission(session.GetHabbo(), "rp.police.corporation.id", "fine_user")) {
                session.SendWhisper("Only on-duty police officers can fine users.", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            amount = Math.Min(amount, targetClient.GetHabbo().Credits);
            if (amount <= 0) {
                session.SendWhisper(targetClient.GetHabbo().Username + " has no credits to fine.", 1);
                return;
            }

            targetClient.GetHabbo().Credits -= amount;
            targetClient.SendPacket(new CreditBalanceComposer(targetClient.GetHabbo().Credits));

            targetClient.SendWhisper("You were fined " + amount + " credit(s) by " + session.GetHabbo().Username + ".", 1);
            session.SendWhisper("You fined " + targetClient.GetHabbo().Username + " " + amount + " credit(s).", 1);
            RpCommandUtil.RoomAction(room, session.GetHabbo(), "*" + session.GetHabbo().Username + " issues a " + amount + " credit fine to " + targetClient.GetHabbo().Username + "*");
        }
    }
}
