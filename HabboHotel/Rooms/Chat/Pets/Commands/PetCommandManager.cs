using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Rooms.Chat.Pets.Commands
{
    public class PetCommandManager
    {
        private readonly Dictionary<int, string> _commandRegister;
        private readonly Dictionary<string, string> _commandDatabase;
        private readonly Dictionary<string, PetCommand> _petCommands;

        public PetCommandManager()
        {
            _petCommands = new Dictionary<string, PetCommand>();
            _commandRegister = new Dictionary<int, string>();
            _commandDatabase = new Dictionary<string, string>();

            Init();
        }

        public void Init()
        {
            _petCommands.Clear();
            _commandRegister.Clear();
            _commandDatabase.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var commands = db.bots_pet_commands.Select(c => new { c.id, c.input_title, c.input }).ToList();

                foreach (var row in commands)
                {
                    _commandRegister.Add(row.id, row.input_title);
                    _commandDatabase.Add(row.input_title + ".input", row.input);
                }
            }

            foreach (var pair in _commandRegister)
            {
                int commandId = pair.Key;
                string commandStringedId = pair.Value;
                string[] commandInput = _commandDatabase[commandStringedId + ".input"].Split(',');

                foreach (string command in commandInput)
                {
                    _petCommands.Add(command, new PetCommand(commandId, command));
                }
            }
        }

        public int TryInvoke(string input)
        {
            if (_petCommands.TryGetValue(input.ToLower(), out PetCommand command))
                return command.Id;
            return 0;
        }
    }
}