using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class RoomCommand : IChatCommand
    {
        public string PermissionRequired => "command_room";

        public string Parameters => "push/pull/enables/respect";

        public string Description => "Gives you the ability to enable or disable basic room commands.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Oops, you must choose a room option to disable.");
                return;
            }

            if (!room.CheckRights(session, true)) {
                session.SendWhisper("Oops, only the room owner or staff can use this command.");
                return;
            }

            string option = @params[1];
            switch (option)
            {
                case "list":
                {
                    StringBuilder list = new("");
                    list.AppendLine("Room Command List");
                    list.AppendLine("-------------------------");
                    list.AppendLine("Pet Morphs: " + (room.PetMorphsAllowed ? "enabled" : "disabled"));
                    list.AppendLine("Pull: " + (room.PullEnabled ? "enabled" : "disabled"));
                    list.AppendLine("Push: " + (room.PushEnabled ? "enabled" : "disabled"));
                    list.AppendLine("Super Pull: " + (room.SuperPullEnabled ? "enabled" : "disabled"));
                    list.AppendLine("Super Push: " + (room.SuperPushEnabled ? "enabled" : "disabled"));
                    list.AppendLine("Respect: " + (room.RespectNotificationsEnabled ? "enabled" : "disabled"));
                    list.AppendLine("Enables: " + (room.EnablesEnabled ? "enabled" : "disabled"));
                    list.AppendLine("Safezone: " + (room.Safezone ? "enabled" : "disabled"));
                        session.SendNotification(list.ToString());
                    break;
                }

                case "push":
                {
                    room.PushEnabled = !room.PushEnabled;
                    bool pushEnabled = room.PushEnabled;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    {
                        db.rooms.Where(r => r.id == room.Id).ExecuteUpdate(s => s.SetProperty(r => r.push_enabled, pushEnabled));
                    }

                    session.SendWhisper("Push mode is now " + (room.PushEnabled ? "enabled!" : "disabled!"));
                    break;
                }

                case "spush":
                {
                    room.SuperPushEnabled = !room.SuperPushEnabled;
                    string superPushEnabled = PlusEnvironment.BoolToEnum(room.SuperPushEnabled);
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    {
                        db.rooms.Where(r => r.id == room.Id).ExecuteUpdate(s => s.SetProperty(r => r.spush_enabled, superPushEnabled));
                    }

                    session.SendWhisper("Super Push mode is now " + (room.SuperPushEnabled ? "enabled!" : "disabled!"));
                    break;
                }

                case "spull":
                {
                    room.SuperPullEnabled = !room.SuperPullEnabled;
                    bool superPullEnabled = room.SuperPullEnabled;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    {
                        db.rooms.Where(r => r.id == room.Id).ExecuteUpdate(s => s.SetProperty(r => r.spull_enabled, superPullEnabled));
                    }

                    session.SendWhisper("Super Pull mode is now " + (room.SuperPullEnabled ? "enabled!" : "disabled!"));
                    break;
                }

                case "pull":
                {
                    room.PullEnabled = !room.PullEnabled;
                    bool pullEnabled = room.PullEnabled;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    {
                        db.rooms.Where(r => r.id == room.Id).ExecuteUpdate(s => s.SetProperty(r => r.pull_enabled, pullEnabled));
                    }

                    session.SendWhisper("Pull mode is now " + (room.PullEnabled ? "enabled!" : "disabled!"));
                    break;
                }

                case "enable":
                case "enables":
                {
                    room.EnablesEnabled = !room.EnablesEnabled;
                    bool enablesEnabled = room.EnablesEnabled;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    {
                        db.rooms.Where(r => r.id == room.Id).ExecuteUpdate(s => s.SetProperty(r => r.enables_enabled, enablesEnabled));
                    }

                    session.SendWhisper("Enables mode set to " + (room.EnablesEnabled ? "enabled!" : "disabled!"));
                    break;
                }

                case "respect":
                {
                    room.RespectNotificationsEnabled = !room.RespectNotificationsEnabled;
                    bool respectNotificationsEnabled = room.RespectNotificationsEnabled;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    {
                        db.rooms.Where(r => r.id == room.Id).ExecuteUpdate(s => s.SetProperty(r => r.respect_notifications_enabled, respectNotificationsEnabled));
                    }

                    session.SendWhisper("Respect notifications mode set to " + (room.RespectNotificationsEnabled ? "enabled!" : "disabled!"));
                    break;
                }

                case "safezone":
                {
                    room.Safezone = !room.Safezone;
                    string safezone = PlusEnvironment.BoolToEnum(room.Safezone);
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    {
                        db.rooms.Where(r => r.id == room.Id).ExecuteUpdate(s => s.SetProperty(r => r.safezone, safezone));
                    }

                     session.SendWhisper("Safezone mode set to " + (room.Safezone ? "enabled!" : "disabled!"));
                     break;
                }


                case "pets":
                case "morphs":
                {
                    room.PetMorphsAllowed = !room.PetMorphsAllowed;
                    bool petMorphsAllowed = room.PetMorphsAllowed;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    {
                        db.rooms.Where(r => r.id == room.Id).ExecuteUpdate(s => s.SetProperty(r => r.pet_morphs_allowed, petMorphsAllowed));
                    }

                    session.SendWhisper("Human pet morphs notifications mode set to " + (room.PetMorphsAllowed ? "enabled!" : "disabled!"));

                    if (!room.PetMorphsAllowed) {
                        foreach (RoomUser user in room.GetRoomUserManager().GetRoomUsers())
                        {
                            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                                continue;

                            user.GetClient().SendWhisper("The room owner has disabled the ability to use a pet morph in this room.");
                            if (user.GetClient().GetHabbo().PetId > 0) {
                                //Tell the user what is going on.
                                user.GetClient().SendWhisper("Oops, the room owner has just disabled pet-morphs, un-morphing you.");

                                //Change the users Pet Id.
                                user.GetClient().GetHabbo().PetId = 0;

                                //Quickly remove the old user instance.
                                room.SendPacket(new UserRemoveComposer(user.VirtualId));

                                //Add the new one, they won't even notice a thing!!11 8-)
                                room.SendPacket(new UsersComposer(user));
                            }
                        }
                    }

                    break;
                }
            }
        }
    }
}