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
            foreach (RoomUser user in room.GetRoomUserManager().GetUserList().ToList())
            {
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
            db.bots.Where(b => b.id == botDbId).ExecuteUpdate(s => s
                .SetProperty(b => b.room_id, roomDbId)
                .SetProperty(b => b.x, x)
                .SetProperty(b => b.y, y));

            List<RandomSpeech> botSpeechList = new();

            //TODO: Grab data?
            var getData = db.bots.Where(b => b.id == botDbId)
                .Select(b => new { b.ai_type, b.rotation, b.walk_mode, b.automatic_chat, b.speaking_interval, b.mix_sentences, b.chat_bubble, b.effect_id, b.gender, b.motto })
                .FirstOrDefault();

            foreach (string speechText in db.bots_speeches.Where(s => s.bot_id == botDbId).Select(s => s.text).ToList())
            {
                botSpeechList.Add(new RandomSpeech(speechText, bot.Id));
            }

            RoomUser botUser = room.GetRoomUserManager().DeployBot(new RoomBot(bot.Id, session.GetHabbo().CurrentRoomId, getData.ai_type, getData.walk_mode, bot.Name, getData.motto, bot.Figure, x, y, 0, getData.rotation, 0, 0, 0, 0, ref botSpeechList, getData.gender, 0, bot.OwnerId, PlusEnvironment.EnumToBool(getData.automatic_chat), getData.speaking_interval, PlusEnvironment.EnumToBool(getData.mix_sentences), getData.chat_bubble, getData.effect_id), null);

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
