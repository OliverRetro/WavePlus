using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Items.Wired.Boxes.Triggers
{
    internal class UserSaysCommandBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.TriggerUserSaysCommand;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public UserSaysCommandBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            StringData = "";
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket packet)
        {
            int unknown = packet.PopInt();
            int ownerOnly = packet.PopInt();
            string message = packet.PopString();

            BoolData = ownerOnly == 1;
            StringData = message;
        }

        public bool Execute(params object[] @params)
        {
            Habbo player = (Habbo)@params[0];
            if (player == null || player.CurrentRoom == null || !player.InRoom)
                return false;

            RoomUser user = player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(player.Username);
            if (user == null)
                return false;

            if ((BoolData && Instance.OwnerId != player.Id) || string.IsNullOrWhiteSpace(StringData))
                return false;

            if (!PlusEnvironment.GetGame().GetChatManager().GetCommands().TryGetCommand(StringData.Replace(":", "").ToLower(),
                out IChatCommand chatCommand))
                return false;

            if (player.ChatCommand == chatCommand)
            {
                player.WiredInteraction = true;
                ICollection<IWiredItem> effects = Instance.GetWired().GetEffects(this);
                ICollection<IWiredItem> conditions = Instance.GetWired().GetConditions(this);

                foreach (IWiredItem condition in conditions.ToList())
                {
                    if (!condition.Execute(player))
                        return false;

                    Instance.GetWired().OnEvent(condition.Item);
                }

                player.GetClient().SendPacket(new WhisperComposer(user.VirtualId, StringData, 0, 0));
                //Check the ICollection to find the random addon effect.
                bool hasRandomEffectAddon = effects.Count(x => x.Type == WiredBoxType.AddonRandomEffect) > 0;
                if (hasRandomEffectAddon)
                {
                    //Okay, so we have a random addon effect, now lets get the IWiredItem and attempt to execute it.
                    IWiredItem randomBox = effects.FirstOrDefault(x => x.Type == WiredBoxType.AddonRandomEffect);
                    if (!randomBox.Execute())
                        return false;

                    //Success! Let's get our selected box and continue.
                    IWiredItem selectedBox = Instance.GetWired().GetRandomEffect(effects.ToList());
                    if (!selectedBox.Execute())
                        return false;

                    //Woo! Almost there captain, now lets broadcast the update to the room instance.
                    if (Instance != null)
                    {
                        Instance.GetWired().OnEvent(randomBox.Item);
                        Instance.GetWired().OnEvent(selectedBox.Item);
                    }
                }
                else
                {
                    foreach (IWiredItem effect in effects.ToList())
                    {
                        if (!effect.Execute(player))
                            return false;

                        Instance.GetWired().OnEvent(effect.Item);
                    }
                }

                return true;
            }

            return false;
        }
    }
}