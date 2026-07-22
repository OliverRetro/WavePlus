using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Interactor
{
    internal class InteractorMannequin : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
        }

        public void OnRemove(GameClient session, Item item)
        {
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (item.ExtraData.Contains(Convert.ToChar(5).ToString())) {
                string[] stuff = item.ExtraData.Split(Convert.ToChar(5));
                session.GetHabbo().Gender = stuff[0].ToUpper();
                Dictionary<string, string> newFig = new();
                newFig.Clear();
                foreach (string man in stuff[1].Split('.')) {
                    foreach (string fig in session.GetHabbo().Look.Split('.')) {
                        if (fig.Split('-')[0] == man.Split('-')[0]) {
                            if (newFig.ContainsKey(fig.Split('-')[0]) && !newFig.ContainsValue(man)) {
                                newFig.Remove(fig.Split('-')[0]);
                                newFig.Add(fig.Split('-')[0], man);
                            } else if (!newFig.ContainsKey(fig.Split('-')[0]) && !newFig.ContainsValue(man)) {
                                newFig.Add(fig.Split('-')[0], man);
                            }
                        } else {
                            if (!newFig.ContainsKey(fig.Split('-')[0])) {
                                newFig.Add(fig.Split('-')[0], fig);
                            }
                        }
                    }
                }

                string final = "";
                foreach (string str in newFig.Values) {
                    final += str + ".";
                }

                session.GetHabbo().Look = final.TrimEnd('.');

                int habboId = session.GetHabbo().Id;
                string look = session.GetHabbo().Look;
                string gender = session.GetHabbo().Gender;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.Users.Where(u => u.Id == habboId).ExecuteUpdate(s => s.SetProperty(u => u.Look, look).SetProperty(u => u.Gender, gender));
                }

                Room room = session.GetHabbo().CurrentRoom;
                RoomUser user = room?.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);
                if (user != null) {
                    session.SendPacket(new UserChangeComposer(user, true));
                    session.GetHabbo().CurrentRoom.SendPacket(new UserChangeComposer(user, false));
                }
            }
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}