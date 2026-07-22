using System.Collections.Generic;
using Plus.HabboHotel.Rooms.AI.Speech;

namespace Plus.HabboHotel.Bots
{
    public class TaxiBotTemplate
    {
        public TaxiBotTemplate(int botId, string aiType, string walkMode, string name, string motto, string look, string gender, int rotation, bool automaticChat, int speakingInterval, bool mixSentences, int chatBubble, int effectId, List<RandomSpeech> speeches)
        {
            BotId = botId;
            AiType = aiType;
            WalkMode = walkMode;
            Name = name;
            Motto = motto;
            Look = look;
            Gender = gender;
            Rotation = rotation;
            AutomaticChat = automaticChat;
            SpeakingInterval = speakingInterval;
            MixSentences = mixSentences;
            ChatBubble = chatBubble;
            EffectId = effectId;
            Speeches = speeches ?? new List<RandomSpeech>();
        }

        public int BotId { get; }
        public string AiType { get; }
        public string WalkMode { get; }
        public string Name { get; }
        public string Motto { get; }
        public string Look { get; }
        public string Gender { get; }
        public int Rotation { get; }
        public bool AutomaticChat { get; }
        public int SpeakingInterval { get; }
        public bool MixSentences { get; }
        public int ChatBubble { get; }
        public int EffectId { get; }
        public List<RandomSpeech> Speeches { get; }
    }
}