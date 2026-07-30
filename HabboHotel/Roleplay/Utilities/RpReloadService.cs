namespace Plus.HabboHotel.Roleplay.Utilities
{
    public static class RpReloadService
    {
        public const string Switches = "items, weapons, groups, crimes, all";

        public static string Reload(string sw)
        {
            switch ((sw ?? string.Empty).Trim().ToLower()) {
                case "items":
                case "rp_items":
                    PlusEnvironment.GetRpItemManager().Init();
                    return "Reloaded RP items.";

                case "weapons":
                case "rp_weapons":
                    PlusEnvironment.GetWeaponManager().Init();
                    return "Reloaded RP weapons.";

                case "groups":
                case "corps":
                    PlusEnvironment.GetGame().GetGroupManager().ReloadAll();
                    return "Reloaded groups (roles + permissions).";

                case "crimes":
                    PlusEnvironment.GetRpCrimeManager().Init();
                    return "Reloaded crimes + penalties.";

                case "all":
                    PlusEnvironment.GetRpItemManager().Init();
                    PlusEnvironment.GetWeaponManager().Init();
                    PlusEnvironment.GetGame().GetGroupManager().ReloadAll();
                    PlusEnvironment.GetRpCrimeManager().Init();
                    return "Reloaded RP items, weapons, groups and crimes.";

                default:
                    return null;
            }
        }
    }
}