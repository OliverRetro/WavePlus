using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Skill;
using System;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class ToggleSkillCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_toggle_skill";

        public string Parameters => "%skillid%";

        public string Description => "Toggle a skill.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Enter a Skill ID to toggle it.");
                return;
            }

            int skillId = Convert.ToInt32(@params[1]);

            if (session.GetHabbo().ToggleSkill(skillId)) {
                Skill skill = PlusEnvironment.GetSkillManager().GetSkillById(skillId);

                session.SendWhisper($"{skill.Name} is now {(session.GetHabbo().GetRpSkills().IsEquipped(skillId) ? "equipped" : "unequipped")}.", 1);
            } else {
                session.SendWhisper("Skill couldn't be toggled. Please note you need to have the skill unlocked and the limit is 4.", 1);
            }
        }
    }
}