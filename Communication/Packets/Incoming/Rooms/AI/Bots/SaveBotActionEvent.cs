using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots
{
    internal class SaveBotActionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            Room room = session.GetHabbo().CurrentRoom;
            if (room == null)
                return;

            int botId = packet.PopInt();
            int actionId = packet.PopInt();
            string dataString = packet.PopString();

            if (actionId < 1 || actionId > 5)
                return;

            if (!room.GetRoomUserManager().TryGetBot(botId, out RoomUser bot))
                return;

            if (bot.BotData.OwnerId != session.GetHabbo().Id && !session.GetHabbo().GetPermissions().HasRight("bot_edit_any_override"))
                return;

            RoomBot roomBot = bot.BotData;
            if (roomBot == null)
                return;

            /* 1 = Copy looks
             * 2 = Setup Speech
             * 3 = Relax
             * 4 = Dance
             * 5 = Change Name
             */

            switch (actionId) {
                #region Copy Looks (1)

                case 1: {
                        //Change the defaults
                        bot.BotData.Look = session.GetHabbo().Look;
                        bot.BotData.Gender = session.GetHabbo().Gender;

                        UserChangeComposer userChangeComposer = new(bot.VirtualId, bot.BotData);

                        room.SendPacket(userChangeComposer);

                        uint copyBotId = (uint)bot.BotData.Id;
                        string copyLook = session.GetHabbo().Look;
                        string copyGender = session.GetHabbo().Gender;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext())
                            db.Bots.Where(b => b.Id == copyBotId).ExecuteUpdate(s => s.SetProperty(b => b.Look, copyLook).SetProperty(b => b.Gender, copyGender));

                        //Room.SendMessage(new UserChangeComposer(BotUser.GetClient(), true));
                        break;
                    }

                #endregion

                #region Setup Speech (2)

                case 2: {
                        string[] configData = dataString.Split(new[]
                        {
                        ";#;"
                    }, StringSplitOptions.None);

                        string[] speechData = configData[0].Split(new[]
                        {
                        '\r',
                        '\n'
                    }, StringSplitOptions.RemoveEmptyEntries);

                        string automaticChat = Convert.ToString(configData[1]);
                        string speakingInterval = Convert.ToString(configData[2]);
                        string mixChat = Convert.ToString(configData[3]);

                        if (string.IsNullOrEmpty(speakingInterval) || Convert.ToInt32(speakingInterval) <= 0 || Convert.ToInt32(speakingInterval) < 7)
                            speakingInterval = "7";

                        roomBot.AutomaticChat = Convert.ToBoolean(automaticChat);
                        roomBot.SpeakingInterval = Convert.ToInt32(speakingInterval);
                        roomBot.MixSentences = Convert.ToBoolean(mixChat);

                        using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                            uint deleteBotId = (uint)bot.BotData.Id;
                            db.BotsSpeeches.Where(sp => sp.BotId == deleteBotId).ExecuteDelete();

                            uint updBotId = (uint)botId;
                            string autoLower = automaticChat.ToLower();
                            int interval = Convert.ToInt32(speakingInterval);
                            string mixEnum = PlusEnvironment.BoolToEnum(Convert.ToBoolean(mixChat));
                            db.Bots.Where(b => b.Id == updBotId).ExecuteUpdate(s => s
                                .SetProperty(b => b.AutomaticChat, autoLower)
                                .SetProperty(b => b.SpeakingInterval, interval)
                                .SetProperty(b => b.MixSentences, mixEnum));

                            for (int i = 0; i <= speechData.Length - 1; i++)
                                db.BotsSpeeches.Add(new Database.EF.Entities.BotsSpeechEntity { BotId = updBotId, Text = speechData[i] });

                            db.SaveChanges();

                            roomBot.RandomSpeech.Clear();

                            foreach (string text in db.BotsSpeeches.Where(sp => sp.BotId == updBotId).Select(sp => sp.Text).ToList())
                                roomBot.RandomSpeech.Add(new RandomSpeech(text, botId));
                        }

                        break;
                    }

                #endregion

                #region Relax (3)

                case 3: {
                        if (bot.BotData.WalkingMode == "stand")
                            bot.BotData.WalkingMode = "freeroam";
                        else
                            bot.BotData.WalkingMode = "stand";

                        uint relaxBotId = (uint)bot.BotData.Id;
                        string walkMode = bot.BotData.WalkingMode;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext())
                            db.Bots.Where(b => b.Id == relaxBotId).ExecuteUpdate(s => s.SetProperty(b => b.WalkMode, walkMode));

                        break;
                    }

                #endregion

                #region Dance (4)

                case 4: {
                        if (bot.BotData.DanceId > 0)
                            bot.BotData.DanceId = 0;
                        else {
                            Random randomDance = new();
                            bot.BotData.DanceId = randomDance.Next(1, 4);
                        }

                        room.SendPacket(new DanceComposer(bot.VirtualId, bot.BotData.DanceId));
                        break;
                    }

                #endregion

                #region Change Name (5)

                case 5: {
                        if (dataString.Length == 0) {
                            session.SendWhisper("Come on, atleast give the bot a name!");
                            return;
                        }

                        if (dataString.Length >= 16) {
                            session.SendWhisper("Come on, the bot doesn't need a name that long!");
                            return;
                        }

                        if (dataString.Contains("<img src") || dataString.Contains("<font ") || dataString.Contains("</font>") || dataString.Contains("</a>") || dataString.Contains("<i>")) {
                            session.SendWhisper("No HTML, please :<");
                            return;
                        }

                        bot.BotData.Name = dataString;
                        uint nameBotId = (uint)bot.BotData.Id;
                        string newName = dataString;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext())
                            db.Bots.Where(b => b.Id == nameBotId).ExecuteUpdate(s => s.SetProperty(b => b.Name, newName));

                        room.SendPacket(new UsersComposer(bot));
                        break;
                    }

                #endregion
            }
        }
    }
}