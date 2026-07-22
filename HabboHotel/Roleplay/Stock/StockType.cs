namespace Plus.HabboHotel.Roleplay.Stock
{
    public static class StockType
    {
        public const string RpItem = "rp_item";
        public const string RpWeapon = "rp_weapon";
        public const string Clothing = "clothing";

        public static bool IsValid(string type) => type is RpItem or RpWeapon or Clothing;
    }
}