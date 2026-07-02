using System;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.Interfaces;
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
                foreach (string man in stuff[1].Split('.'))
                {
                    foreach (string fig in session.GetHabbo().Look.Split('.'))
                    {
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
                foreach (string str in newFig.Values)
                {
                    final += str + ".";
                }


                session.GetHabbo().Look = final.TrimEnd('.');

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE users SET look = @look, gender = @gender WHERE id = '" + session.GetHabbo().Id + "' LIMIT 1");
                    dbClient.AddParameter("look", session.GetHabbo().Look);
                    dbClient.AddParameter("gender", session.GetHabbo().Gender);
                    dbClient.RunQuery();
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