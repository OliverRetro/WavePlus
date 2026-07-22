using System;
using System.Drawing;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.AI.Responses;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.AI.Types
{
    internal class BartenderBot : BotAI
    {
        private readonly int _virtualId;
        private int _actionTimer;
        private int _speechTimer;

        public BartenderBot(int virtualId)
        {
            _virtualId = virtualId;
        }

        public override void OnSelfEnterRoom()
        {
        }

        public override void OnSelfLeaveRoom(bool kicked)
        {
        }

        public override void OnUserEnterRoom(RoomUser user)
        {
        }

        public override void OnUserLeaveRoom(GameClient client)
        {
        }

        public override void OnUserSay(RoomUser user, string message)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            if (Gamemap.TileDistance(GetRoomUser().X, GetRoomUser().Y, user.X, user.Y) > 8)
                return;

            BotResponse response = PlusEnvironment.GetGame().GetBotManager().GetResponse(GetBotData().AiType, message);
            if (response == null)
                return;

            switch (response.ResponseType.ToLower()) {
                case "say":
                    GetRoomUser().Chat(response.ResponseText.Replace("{username}", user.GetClient().GetHabbo().Username));
                    break;

                case "shout":
                    GetRoomUser().Chat(response.ResponseText.Replace("{username}", user.GetClient().GetHabbo().Username));
                    break;

                case "whisper":
                    user.GetClient().SendPacket(new WhisperComposer(GetRoomUser().VirtualId, response.ResponseText.Replace("{username}", user.GetClient().GetHabbo().Username), 0, 0));
                    break;
            }

            if (response.BeverageIds.Count > 0) {
                user.CarryItem(response.BeverageIds[RandomNumber.GenerateRandom(0, (response.BeverageIds.Count - 1))]);
            }
        }

        public override void OnUserShout(RoomUser user, string message)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            if (Gamemap.TileDistance(GetRoomUser().X, GetRoomUser().Y, user.X, user.Y) > 8)
                return;

            BotResponse response = PlusEnvironment.GetGame().GetBotManager().GetResponse(GetBotData().AiType, message);
            if (response == null)
                return;

            switch (response.ResponseType.ToLower()) {
                case "say":
                    GetRoomUser().Chat(response.ResponseText.Replace("{username}", user.GetClient().GetHabbo().Username));
                    break;

                case "shout":
                    GetRoomUser().Chat(response.ResponseText.Replace("{username}", user.GetClient().GetHabbo().Username));
                    break;

                case "whisper":
                    user.GetClient().SendPacket(new WhisperComposer(GetRoomUser().VirtualId, response.ResponseText.Replace("{username}", user.GetClient().GetHabbo().Username), 0, 0));
                    break;
            }

            if (response.BeverageIds.Count > 0) {
                user.CarryItem(response.BeverageIds[RandomNumber.GenerateRandom(0, (response.BeverageIds.Count - 1))]);
            }
        }

        public override void OnTimerTick()
        {
            if (GetBotData() == null)
                return;

            if (_speechTimer <= 0) {
                if (GetBotData().RandomSpeech.Count > 0) {
                    if (GetBotData().AutomaticChat == false)
                        return;

                    RandomSpeech speech = GetBotData().GetRandomSpeech();

                    string @string = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(speech.Message);
                    if (@string.Contains("<img src") || @string.Contains("<font ") || @string.Contains("</font>") || @string.Contains("</a>") || @string.Contains("<i>"))
                        @string = "I really shouldn't be using HTML within bot speeches.";
                    GetRoomUser().Chat(@string, GetBotData().ChatBubble);
                }

                _speechTimer = GetBotData().SpeakingInterval;
            } else
                _speechTimer--;

            if (_actionTimer <= 0) {
                switch (GetBotData().WalkingMode.ToLower()) {
                    default:
                    case "stand":
                        // (8) Why is my life so boring?
                        break;

                    case "freeroam":
                        if (GetBotData().ForcedMovement) {
                            if (GetRoomUser().Coordinate == GetBotData().TargetCoordinate) {
                                GetBotData().ForcedMovement = false;
                                GetBotData().TargetCoordinate = new Point();

                                GetRoomUser().MoveTo(GetBotData().TargetCoordinate.X, GetBotData().TargetCoordinate.Y);
                            }
                        } else if (GetBotData().ForcedUserTargetMovement > 0) {
                            RoomUser target = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(GetBotData().ForcedUserTargetMovement);
                            if (target == null) {
                                GetBotData().ForcedUserTargetMovement = 0;
                                GetRoomUser().ClearMovement(true);
                            } else {
                                Point? sq = GetBestAdjacentSquare(target);
                                if (sq.HasValue)
                                    GetRoomUser().MoveTo(sq.Value);
                            }
                        } else if (GetBotData().TargetUser == 0) {
                            Point nextCoord = GetRoom().GetGameMap().GetRandomWalkableSquare();
                            GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
                        }

                        break;

                    case "specified_range":

                        break;
                }

                _actionTimer = new Random((DateTime.Now.Millisecond + _virtualId) ^ 2).Next(5, 15);
            } else
                _actionTimer--;
        }
    }
}