using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands
{
    public interface IChatCommand
    {
        string PermissionRequired { get; }
        string GroupPermissionRequired => string.Empty;
        string Parameters { get; }
        string Description { get; }
        bool UsableWhileDead => false;
        bool UsableWhileCuffed => false;

        void Execute(GameClient session, Room room, string[] @params);
    }
}