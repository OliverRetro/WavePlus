using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Inventory.Bots;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.HabboHotel.Users.Inventory.Bots;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots
{
    internal class PlaceBotEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            if (!room.CheckRights(session, true))
                return;

            int botId = packet.PopInt();
            int x = packet.PopInt();
            int y = packet.PopInt();

            if (!room.GetGameMap().CanWalk(x, y, false) || !room.GetGameMap().ValidTile(x, y)) {
                session.SendNotification("You cannot place a bot here!");
                return;
            }

            if (!session.GetHabbo().GetInventoryComponent().TryGetBot(botId, out Bot bot))
                return;

            int botCount = 0;
            foreach (RoomUser user in room.GetRoomUserManager().GetUserList().ToList()) {
                if (user == null || user.IsPet || !user.IsBot)
                    continue;

                botCount += 1;
            }

            if (botCount >= 5 && !session.GetHabbo().GetPermissions().HasRight("bot_place_any_override")) {
                session.SendNotification("Sorry; 5 bots per room only!");
                return;
            }

            //TODO: Hmm, maybe not????
            uint botDbId = (uint)bot.Id;
            uint roomDbId = (uint)room.RoomId;
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.Bots.Where(b => b.Id == botDbId).ExecuteUpdate(s => s
                .SetProperty(b => b.RoomId, roomDbId)
                .SetProperty(b => b.X, x)
                .SetProperty(b => b.Y, y));

            List<RandomSpeech> botSpeechList = new();

            //TODO: Grab data?
            var getData = db.Bots.Where(b => b.Id == botDbId)
                .Select(b => new { b.AiType, b.Rotation, b.WalkMode, b.AutomaticChat, b.SpeakingInterval, b.MixSentences, b.ChatBubble, b.EffectId, b.Gender, b.Motto })
                .FirstOrDefault();

            foreach (string speechText in db.BotsSpeeches.Where(s => s.BotId == botDbId).Select(s => s.Text).ToList()) {
                botSpeechList.Add(new RandomSpeech(speechText, bot.Id));
            }

            RoomUser botUser = room.GetRoomUserManager().DeployBot(new RoomBot(bot.Id, session.GetHabbo().CurrentRoomId, getData.AiType, getData.WalkMode, bot.Name, getData.Motto, bot.Figure, x, y, 0, getData.Rotation, 0, 0, 0, 0, ref botSpeechList, getData.Gender, 0, bot.OwnerId, PlusEnvironment.EnumToBool(getData.AutomaticChat), getData.SpeakingInterval, PlusEnvironment.EnumToBool(getData.MixSentences), getData.ChatBubble, getData.EffectId), null);

            if (botUser.BotData.RandomSpeech.Count > 0)
                botUser.Chat(botUser.BotData.GetRandomSpeech().Message);

            room.GetGameMap().UpdateUserMovement(new Point(x, y), new Point(x, y), botUser);

            if (!session.GetHabbo().GetInventoryComponent().TryRemoveBot(botId, out Bot toRemove)) {
                Console.WriteLine("Error whilst removing Bot: " + toRemove.Id);
                return;
            }

            session.SendPacket(new BotInventoryComposer(session.GetHabbo().GetInventoryComponent().GetBots()));
        }
    }
}