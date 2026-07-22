using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;

namespace Plus.HabboHotel.Items.Data.Moodlight
{
    public class MoodlightData
    {
        public int ItemId;
        public int CurrentPreset;
        public bool Enabled;

        public List<MoodlightPreset> Presets;

        public MoodlightData(int itemId)
        {
            ItemId = itemId;

            uint itemKey = (uint)itemId;
            using WavePlusContext db = PlusEnvironment.GetDbContext();

            var row = db.RoomItemsMoodlights.Where(m => m.ItemId == itemKey).Select(m => new { m.Enabled, m.CurrentPreset, m.PresetOne, m.PresetTwo, m.PresetThree }).FirstOrDefault();

            if (row == null) {
                db.RoomItemsMoodlights.Add(new Database.EF.Entities.RoomItemsMoodlightEntity
                {
                    ItemId = itemKey,
                    Enabled = "0",
                    CurrentPreset = 1,
                    PresetOne = "#000000,255,0",
                    PresetTwo = "#000000,255,0",
                    PresetThree = "#000000,255,0"
                });
                db.SaveChanges();
                row = db.RoomItemsMoodlights.Where(m => m.ItemId == itemKey).Select(m => new { m.Enabled, m.CurrentPreset, m.PresetOne, m.PresetTwo, m.PresetThree }).FirstOrDefault();
            }

            Enabled = PlusEnvironment.EnumToBool(row.Enabled);
            CurrentPreset = row.CurrentPreset;
            Presets = new List<MoodlightPreset>();

            Presets.Add(GeneratePreset(row.PresetOne));
            Presets.Add(GeneratePreset(row.PresetTwo));
            Presets.Add(GeneratePreset(row.PresetThree));
        }

        public void Enable()
        {
            Enabled = true;

            uint itemKey = (uint)ItemId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.RoomItemsMoodlights.Where(m => m.ItemId == itemKey).ExecuteUpdate(s => s.SetProperty(m => m.Enabled, "1"));
        }

        public void Disable()
        {
            Enabled = false;

            uint itemKey = (uint)ItemId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.RoomItemsMoodlights.Where(m => m.ItemId == itemKey).ExecuteUpdate(s => s.SetProperty(m => m.Enabled, "0"));
        }

        public void UpdatePreset(int preset, string color, int intensity, bool bgOnly, bool hax = false)
        {
            if (!IsValidColor(color) || !IsValidIntensity(intensity) && !hax) {
                return;
            }

            string presetVal = color + "," + intensity + "," + PlusEnvironment.BoolToEnum(bgOnly);
            uint itemKey = (uint)ItemId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                switch (preset) {
                    case 3:
                        db.RoomItemsMoodlights.Where(m => m.ItemId == itemKey).ExecuteUpdate(s => s.SetProperty(m => m.PresetThree, presetVal));
                        break;

                    case 2:
                        db.RoomItemsMoodlights.Where(m => m.ItemId == itemKey).ExecuteUpdate(s => s.SetProperty(m => m.PresetTwo, presetVal));
                        break;

                    default:
                        db.RoomItemsMoodlights.Where(m => m.ItemId == itemKey).ExecuteUpdate(s => s.SetProperty(m => m.PresetOne, presetVal));
                        break;
                }
            }

            GetPreset(preset).ColorCode = color;
            GetPreset(preset).ColorIntensity = intensity;
            GetPreset(preset).BackgroundOnly = bgOnly;
        }

        public static MoodlightPreset GeneratePreset(string data)
        {
            string[] bits = data.Split(',');

            if (!IsValidColor(bits[0])) {
                bits[0] = "#000000";
            }

            return new MoodlightPreset(bits[0], int.Parse(bits[1]), PlusEnvironment.EnumToBool(bits[2]));
        }

        public MoodlightPreset GetPreset(int i)
        {
            i--;

            if (Presets[i] != null) {
                return Presets[i];
            }

            return new MoodlightPreset("#000000", 255, false);
        }

        public static bool IsValidColor(string colorCode)
        {
            switch (colorCode) {
                case "#000000":
                case "#0053F7":
                case "#EA4532":
                case "#82F349":
                case "#74F5F5":
                case "#E759DE":
                case "#F2F851":

                    return true;

                default:

                    return false;
            }
        }

        public static bool IsValidIntensity(int intensity)
        {
            return intensity >= 0 && intensity <= 255;
        }

        public string GenerateExtraData()
        {
            MoodlightPreset preset = GetPreset(CurrentPreset);
            var sb = new StringBuilder();

            sb.Append(Enabled ? 2 : 1);

            sb.Append(",");
            sb.Append(CurrentPreset);
            sb.Append(",");

            sb.Append(preset.BackgroundOnly ? 2 : 1);

            sb.Append(",");
            sb.Append(preset.ColorCode);
            sb.Append(",");
            sb.Append(preset.ColorIntensity);
            return sb.ToString();
        }
    }
}