using System;
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

            var row = db.room_items_moodlights.Where(m => m.item_id == itemKey).Select(m => new { m.enabled, m.current_preset, m.preset_one, m.preset_two, m.preset_three }).FirstOrDefault();

            if (row == null) {
                db.room_items_moodlights.Add(new Database.EF.Entities.room_items_moodlight
                {
                    item_id = itemKey,
                    enabled = "0",
                    current_preset = 1,
                    preset_one = "#000000,255,0",
                    preset_two = "#000000,255,0",
                    preset_three = "#000000,255,0"
                });
                db.SaveChanges();
                row = db.room_items_moodlights.Where(m => m.item_id == itemKey).Select(m => new { m.enabled, m.current_preset, m.preset_one, m.preset_two, m.preset_three }).FirstOrDefault();
            }

            Enabled = PlusEnvironment.EnumToBool(row.enabled);
            CurrentPreset = row.current_preset;
            Presets = new List<MoodlightPreset>();

            Presets.Add(GeneratePreset(row.preset_one));
            Presets.Add(GeneratePreset(row.preset_two));
            Presets.Add(GeneratePreset(row.preset_three));
        }

        public void Enable()
        {
            Enabled = true;

            uint itemKey = (uint)ItemId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.room_items_moodlights.Where(m => m.item_id == itemKey).ExecuteUpdate(s => s.SetProperty(m => m.enabled, "1"));
        }

        public void Disable()
        {
            Enabled = false;

            uint itemKey = (uint)ItemId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.room_items_moodlights.Where(m => m.item_id == itemKey).ExecuteUpdate(s => s.SetProperty(m => m.enabled, "0"));
        }

        public void UpdatePreset(int preset, string color, int intensity, bool bgOnly, bool hax = false)
        {
            if (!IsValidColor(color) || !IsValidIntensity(intensity) && !hax) {
                return;
            }

            string presetVal = color + "," + intensity + "," + PlusEnvironment.BoolToEnum(bgOnly);
            uint itemKey = (uint)ItemId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                switch (preset)
                {
                    case 3:
                        db.room_items_moodlights.Where(m => m.item_id == itemKey).ExecuteUpdate(s => s.SetProperty(m => m.preset_three, presetVal));
                        break;

                    case 2:
                        db.room_items_moodlights.Where(m => m.item_id == itemKey).ExecuteUpdate(s => s.SetProperty(m => m.preset_two, presetVal));
                        break;

                    default:
                        db.room_items_moodlights.Where(m => m.item_id == itemKey).ExecuteUpdate(s => s.SetProperty(m => m.preset_one, presetVal));
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
            switch (colorCode)
            {
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
