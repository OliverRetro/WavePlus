using System;
using System.Collections.Generic;
using System.Drawing;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.HabboHotel.Rooms.AI.Types;

namespace Plus.HabboHotel.Rooms.AI
{
    public class RoomBot
    {
        public int Id;
        public int BotId;
        public int VirtualId;

        public BotAIType AiType;

        public int DanceId;
        public int EffectId;
        public string Gender;

        public string Look;
        public string Motto;
        public string Name;
        public int RoomId;
        public int Rot;

        public string WalkingMode;

        public int X;
        public int Y;
        public double Z;
        public int MaxX;
        public int MaxY;
        public int MinX;
        public int MinY;

        public int OwnerId;

        public bool AutomaticChat;
        public int SpeakingInterval;
        public bool MixSentences;

        public RoomUser RoomUser;
        public List<RandomSpeech> RandomSpeech;

        public bool ForcedMovement { get; set; }
        public int ForcedUserTargetMovement { get; set; }
        public Point TargetCoordinate { get; set; }

        public int TargetUser { get; set; }

        public RoomBot(int id, int roomId, string type, string walkingMode, string name, string motto, string look, int x, int y, double z, int rotation,
            int minX, int minY, int maxX, int maxY, ref List<RandomSpeech> speeches, string gender, int dance, int ownerId,
            bool automaticChat, int speakingInterval, bool mixSentences, int chatBubble, int effectId = 0)
        {
            Id = id;
            BotId = id;
            RoomId = roomId;

            Name = name;
            Motto = motto;
            Look = look;
            Gender = gender.ToUpper();

            AiType = BotUtility.GetAIFromString(type);
            WalkingMode = walkingMode;

            X = x;
            Y = y;
            Z = z;
            Rot = rotation;
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;

            VirtualId = -1;
            RoomUser = null;
            DanceId = dance;
            EffectId = effectId;

            LoadRandomSpeech(speeches);

            OwnerId = ownerId;

            AutomaticChat = automaticChat;
            SpeakingInterval = speakingInterval;
            MixSentences = mixSentences;

            ChatBubble = chatBubble;
            ForcedMovement = false;
            TargetCoordinate = new Point();
            TargetUser = 0;
        }

        public bool IsPet => AiType == BotAIType.Pet;

        #region Speech Related

        public void LoadRandomSpeech(List<RandomSpeech> speeches)
        {
            RandomSpeech = new List<RandomSpeech>();
            foreach (RandomSpeech speech in speeches) {
                if (speech.BotId == BotId)
                    RandomSpeech.Add(speech);
            }
        }

        public RandomSpeech GetRandomSpeech()
        {
            var rand = new Random();

            if (RandomSpeech.Count < 1)
                return new RandomSpeech("", 0);
            return RandomSpeech[rand.Next(0, RandomSpeech.Count - 1)];
        }

        #endregion

        #region AI Related

        public BotAI GenerateBotAI(int virtualId)
        {
            switch (AiType) {
                case BotAIType.Pet:
                    return new PetBot(virtualId);
                case BotAIType.Generic:
                    return new GenericBot(virtualId);
                case BotAIType.Bartender:
                    return new BartenderBot(virtualId);
                default:
                    return new GenericBot(virtualId);
            }
        }

        #endregion

        public int ChatBubble { get; set; }
    }
}