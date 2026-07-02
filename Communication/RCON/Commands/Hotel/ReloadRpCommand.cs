using Plus.HabboHotel.Roleplay.Utilities;

namespace Plus.Communication.Rcon.Commands.Hotel
{
    /// <summary>Reloads RP definition tables. Parameter: switch (items/weapons/groups/crimes/all).</summary>
    internal class ReloadRpCommand : IRconCommand
    {
        public string Description => "Reload RP definitions. Parameter: items, weapons, groups, crimes or all.";

        public string Parameters => "%switch%";

        public bool TryExecute(string[] parameters)
        {
            string sw = parameters != null && parameters.Length > 0 ? parameters[0] : "all";
            return RpReloadService.Reload(sw) != null;
        }
    }
}
