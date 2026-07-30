using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Emotions
{
    public sealed class ChatEmotionsManager
    {
        private readonly Dictionary<string, ChatEmotions> _emotions = new()
        {
            // Smile
            {":)", ChatEmotions.Smile},
            {";)", ChatEmotions.Smile},
            {":d", ChatEmotions.Smile},
            {";d", ChatEmotions.Smile},
            {":]", ChatEmotions.Smile},
            {";]", ChatEmotions.Smile},
            {"=)", ChatEmotions.Smile},
            {"=]", ChatEmotions.Smile},
            {":-)", ChatEmotions.Smile},

            // Angry
            {">:(", ChatEmotions.Angry},
            {">:[", ChatEmotions.Angry},
            {">;[", ChatEmotions.Angry},
            {">;(", ChatEmotions.Angry},
            {">=(", ChatEmotions.Angry},

            // Shocked
            {":o", ChatEmotions.Shocked},
            {";o", ChatEmotions.Shocked},
            {">;o", ChatEmotions.Shocked},
            {">:o", ChatEmotions.Shocked},
            {"=o", ChatEmotions.Shocked},
            {">=o", ChatEmotions.Shocked},

            // Sad
            {";'(", ChatEmotions.Sad},
            {";[", ChatEmotions.Sad},
            {":[", ChatEmotions.Sad},
            {";(", ChatEmotions.Sad},
            {"=(", ChatEmotions.Sad},
            {"='(", ChatEmotions.Sad},
            {"=[", ChatEmotions.Sad},
            {"='[", ChatEmotions.Sad},
            {":(", ChatEmotions.Sad},
            {":-(", ChatEmotions.Sad}
        };

        public int GetEmotionsForText(string text)
        {
            foreach (KeyValuePair<string, ChatEmotions> kvp in _emotions) {
                if (text.ToLower().Contains(kvp.Key.ToLower())) {
                    return GetEmoticonPacketNum(kvp.Value);
                }
            }

            return 0;
        }

        private static int GetEmoticonPacketNum(ChatEmotions e)
        {
            switch (e) {
                case ChatEmotions.Smile:
                    return 1;

                case ChatEmotions.Angry:
                    return 2;

                case ChatEmotions.Shocked:
                    return 3;

                case ChatEmotions.Sad:
                    return 4;

                case ChatEmotions.None:
                default:
                    return 0;
            }
        }
    }
}