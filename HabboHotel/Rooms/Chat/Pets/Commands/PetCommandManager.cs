using System;
using System.Collections.Generic;
using System.Data;
using Plus.Database.Interfaces;

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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `bots_pet_commands`");
                DataTable table = dbClient.GetTable();

                if (table != null) {
                    foreach (DataRow row in table.Rows)
                    {
                        _commandRegister.Add(Convert.ToInt32(row[0]), row[1].ToString());
                        _commandDatabase.Add(row[1] + ".input", row[2].ToString());
                    }
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