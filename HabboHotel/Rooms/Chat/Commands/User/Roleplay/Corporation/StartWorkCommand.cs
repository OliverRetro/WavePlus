using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Users;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class StartWorkCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_start_work";

        public string Parameters => "";

        public string Description => "Start your work shift in the corporation.";
        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room?.Group == null || room == null)
                return;

            Habbo habbo = session.GetHabbo();
            Group group = room.Group;

            if (session.GetHabbo().GetRpStats().Energy < 1)
            {
                session.SendWhisper("You don't have enough energy!", 1);
                return;
            }

            if (group.Kind != GroupKind.Corporation)
            {
                session.SendWhisper("This command can only be used in corporation room!", 1);
                return;
            }

            if (!group.IsMember(habbo.Id))
            {
                session.SendWhisper("You don't work here! Feel free to apply to join the corporation.", 1);
                return;
            }

            if (group.TryGetRoleData(habbo.Id, out GroupRole role))
            {
                session.SendWhisper("Your role is: " + role.Name + ". Motto: " + role.ShiftMotto + ". Look " + role.ShiftCostume + ". Shift length: " + role.ShiftDuration + " minutes.", 1);

                if (role.ShiftMotto != string.Empty || role.ShiftMotto != null) {
                    habbo.OldMotto = habbo.Motto;
                    habbo.Motto = role.ShiftMotto;
                }

                if (role.ShiftCostume != string.Empty || role.ShiftCostume != null)
                {
                    habbo.OldLook = habbo.Look;

                    string gender = habbo.Gender.ToLower();
                    string figureString = role.ShiftCostume;

                    string selectedFigure = null;

                    if (figureString.Contains('|'))
                    {
                        Dictionary<string, string> figures = new();

                        foreach (string part in figureString.Split(';'))
                        {
                            if (string.IsNullOrWhiteSpace(part))
                                continue;

                            string[] split = part.Split('|');

                            if (split.Length != 2)
                                continue;

                            figures[split[0].ToLower()] = split[1];
                        }

                        if (figures.TryGetValue(gender, out string fig))
                        {
                            selectedFigure = fig;
                        } else if (figures.TryGetValue("m", out fig))
                        {
                            selectedFigure = fig;
                        }
                    } else {
                        selectedFigure = figureString;
                    }

                    if (string.IsNullOrWhiteSpace(selectedFigure))
                        return;

                    Dictionary<string, string> lookParts = new();

                    foreach (string part in habbo.Look.Split('.'))
                    {
                        if (string.IsNullOrWhiteSpace(part))
                            continue;

                        string[] split = part.Split('-');

                        if (split.Length == 0)
                            continue;

                        lookParts[split[0]] = part;
                    }

                    foreach (string part in selectedFigure.Split('.'))
                    {
                        if (string.IsNullOrWhiteSpace(part))
                            continue;

                        string[] split = part.Split('-');

                        if (split.Length == 0)
                            continue;

                        lookParts[split[0]] = part;
                    }

                    habbo.Look = string.Join(".", lookParts.Values);
                }

                // update the user, regardless if they have a costume or motto change
                RoomUser user = room?.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);

                room.SendPacket(new UserChangeComposer(user, false));
                return;
            } else {
                session.SendWhisper("You don't have a role in this corporation! Contact your superior to get assigned a role.", 1);
                return;
            }
        }
    }
}
