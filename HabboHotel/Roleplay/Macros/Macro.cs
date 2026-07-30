using System.Collections.Generic;

namespace Plus.HabboHotel.Roleplay.Macros
{
    public class Macro
    {
        public int Id { get; set; }
        public int UserId { get; }
        public string Name { get; set; }

        // keybind (e.g. "1", "F1") -> command text to run
        public Dictionary<string, string> Sets { get; }

        public Macro(int id, int userId, string name, Dictionary<string, string> sets = null)
        {
            Id = id;
            UserId = userId;
            Name = name;
            Sets = sets ?? new Dictionary<string, string>();
        }
    }
}