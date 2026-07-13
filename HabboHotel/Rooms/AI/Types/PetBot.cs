using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.PathFinding;
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.AI.Types
{
    public class PetBot : BotAI
    {
        private int _actionTimer;
        private int _energyTimer;
        private int _speechTimer;

        public PetBot(int virtualId)
        {
            _speechTimer = new Random((virtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
            _actionTimer = new Random((virtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 30 + virtualId);
            _energyTimer = new Random((virtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
        }

        private void RemovePetStatus()
        {
            RoomUser pet = GetRoomUser();
            if (pet != null)
            {
                foreach (KeyValuePair<string, string> kvp in pet.Statusses.ToList())
                {
                    if (pet.Statusses.ContainsKey(kvp.Key))
                        pet.Statusses.Remove(kvp.Key);
                }
            }
        }

        public override void OnSelfEnterRoom()
        {
            Point nextCoord = GetRoom().GetGameMap().GetRandomWalkableSquare();
            //int randomX = PlusEnvironment.GetRandomNumber(0, GetRoom().Model.MapSizeX);
            //int randomY = PlusEnvironment.GetRandomNumber(0, GetRoom().Model.MapSizeY);
            if (GetRoomUser() != null)
                GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
        }

        public override void OnSelfLeaveRoom(bool kicked)
        {
        }


        public override void OnUserEnterRoom(RoomUser user)
        {
            if (user.GetClient() != null && user.GetClient().GetHabbo() != null)
            {
                RoomUser pet = GetRoomUser();
                if (pet != null)
                {
                    if (user.GetClient().GetHabbo().Username == pet.PetData.OwnerName)
                    {
                        string[] speech = PlusEnvironment.GetGame().GetChatManager().GetPetLocale().GetValue("welcome.speech.pet" + pet.PetData.Type);
                        string rSpeech = speech[RandomNumber.GenerateRandom(0, speech.Length - 1)];
                        pet.Chat(rSpeech);
                    }
                }
            }
        }

        public override void OnUserLeaveRoom(GameClient client)
        {
        }

        public override void OnUserShout(RoomUser user, string message)
        {
        }

        public override void OnTimerTick()
        {
            RoomUser pet = GetRoomUser();
            if (pet == null)
                return;

            #region Speech

            if (_speechTimer <= 0)
            {
                if (pet.PetData.DbState != PetDatabaseUpdateState.NeedsInsert)
                    pet.PetData.DbState = PetDatabaseUpdateState.NeedsUpdate;

                if (pet != null)
                {
                    var randomSpeech = new Random();
                    RemovePetStatus();

                    string[] speech = PlusEnvironment.GetGame().GetChatManager().GetPetLocale().GetValue("speech.pet" + pet.PetData.Type);
                    string rSpeech = speech[RandomNumber.GenerateRandom(0, speech.Length - 1)];

                    if (rSpeech.Length != 3)
                    {
                        pet.Chat(rSpeech);
                    }
                    else
                        pet.Statusses.Add(rSpeech, TextHandling.GetString(pet.Z));
                }

                _speechTimer = PlusEnvironment.GetRandomNumber(20, 120);
            }
            else
            {
                _speechTimer--;
            }

            #endregion

            #region Actions

            if (_actionTimer <= 0)
            {
                try
                {
                    RemovePetStatus();
                    _actionTimer = RandomNumber.GenerateRandom(15, 40 + GetRoomUser().PetData.VirtualId);
                    if (!GetRoomUser().RidingHorse)
                    {
                        // Remove Status
                        RemovePetStatus();

                        Point nextCoord = GetRoom().GetGameMap().GetRandomWalkableSquare();
                        if (GetRoomUser().CanWalk)
                            GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
                    }
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }
            }
            else
            {
                _actionTimer--;
            }

            #endregion

            #region Energy

            if (_energyTimer <= 0)
            {
                RemovePetStatus(); // Remove Status

                pet.PetData.PetEnergy(true); // Add Energy

                _energyTimer = RandomNumber.GenerateRandom(30, 120); // 2 Min Max
            }
            else
            {
                _energyTimer--;
            }

            #endregion
        }

        #region Commands

        public override void OnUserSay(RoomUser user, string message)
        {
            if (user == null)
                return;

            RoomUser pet = GetRoomUser();
            if (pet == null)
                return;

            if (pet.PetData.DbState != PetDatabaseUpdateState.NeedsInsert)
                pet.PetData.DbState = PetDatabaseUpdateState.NeedsUpdate;

            if (message.ToLower().Equals(pet.PetData.Name.ToLower()))
            {
                pet.SetRot(Rotation.Calculate(pet.X, pet.Y, user.X, user.Y), false);
                return;
            }

            //if (!Pet.Statusses.ContainsKey("gst thr"))
            //    Pet.Statusses.Add("gst thr", TextHandling.GetString(Pet.Z));

            if ((message.ToLower().StartsWith(pet.PetData.Name.ToLower() + " ") && user.GetClient().GetHabbo().Username.ToLower() == pet.PetData.OwnerName.ToLower()) || (message.ToLower().StartsWith(pet.PetData.Name.ToLower() + " ") && PlusEnvironment.GetGame().GetChatManager().GetPetCommands().TryInvoke(message.Substring(pet.PetData.Name.ToLower().Length + 1)) == 8))
            {
                string command = message.Substring(pet.PetData.Name.ToLower().Length + 1);

                int r = RandomNumber.GenerateRandom(1, 8); // Made Random
                if (pet.PetData.Energy > 10 && r < 6 || pet.PetData.Level > 15 || PlusEnvironment.GetGame().GetChatManager().GetPetCommands().TryInvoke(command) == 8)
                {
                    RemovePetStatus(); // Remove Status

                    switch (PlusEnvironment.GetGame().GetChatManager().GetPetCommands().TryInvoke(command))
                    {
                        // TODO - Level you can use the commands at...

                        #region free

                        case 1:
                            RemovePetStatus();

                            //int randomX = PlusEnvironment.GetRandomNumber(0, GetRoom().Model.MapSizeX);
                            //int randomY = PlusEnvironment.GetRandomNumber(0, GetRoom().Model.MapSizeY);
                            Point nextCoord = GetRoom().GetGameMap().GetRandomWalkableSquare();
                            pet.MoveTo(nextCoord.X, nextCoord.Y);

                            pet.PetData.AddExperience(10); // Give XP

                            break;

                        #endregion

                        #region here

                        case 2:
                            RemovePetStatus();

                            int newX = user.X;
                            int newY = user.Y;

                            _actionTimer = 30; // Reset ActionTimer

                            #region Rotation

                            if (user.RotBody == 4)
                            {
                                newY = user.Y + 1;
                            }
                            else if (user.RotBody == 0)
                            {
                                newY = user.Y - 1;
                            }
                            else if (user.RotBody == 6)
                            {
                                newX = user.X - 1;
                            }
                            else if (user.RotBody == 2)
                            {
                                newX = user.X + 1;
                            }
                            else if (user.RotBody == 3)
                            {
                                newX = user.X + 1;
                                newY = user.Y + 1;
                            }
                            else if (user.RotBody == 1)
                            {
                                newX = user.X + 1;
                                newY = user.Y - 1;
                            }
                            else if (user.RotBody == 7)
                            {
                                newX = user.X - 1;
                                newY = user.Y - 1;
                            }
                            else if (user.RotBody == 5)
                            {
                                newX = user.X - 1;
                                newY = user.Y + 1;
                            }

                            #endregion

                            pet.PetData.AddExperience(10); // Give XP

                            pet.MoveTo(newX, newY);
                            break;

                        #endregion

                        #region sit

                        case 3:
                            // Remove Status
                            RemovePetStatus();

                            pet.PetData.AddExperience(10); // Give XP

                            // Add Status
                            pet.Statusses.Add("sit", TextHandling.GetString(pet.Z));
                            pet.UpdateNeeded = true;

                            _actionTimer = 25;
                            _energyTimer = 10;
                            break;

                        #endregion

                        #region lay

                        case 4:
                            // Remove Status
                            RemovePetStatus();

                            // Add Status
                            pet.Statusses.Add("lay", TextHandling.GetString(pet.Z));
                            pet.UpdateNeeded = true;

                            pet.PetData.AddExperience(10); // Give XP

                            _actionTimer = 30;
                            _energyTimer = 5;
                            break;

                        #endregion

                        #region dead

                        case 5:
                            // Remove Status
                            RemovePetStatus();

                            // Add Status 
                            pet.Statusses.Add("ded", TextHandling.GetString(pet.Z));
                            pet.UpdateNeeded = true;

                            pet.PetData.AddExperience(10); // Give XP

                            // Don't move to speak for a set amount of time.
                            _speechTimer = 45;
                            _actionTimer = 30;

                            break;

                        #endregion

                        #region sleep

                        case 6:
                            // Remove Status
                            RemovePetStatus();

                            pet.Chat("ZzzZZZzzzzZzz");
                            pet.Statusses.Add("lay", TextHandling.GetString(pet.Z));
                            pet.UpdateNeeded = true;

                            pet.PetData.AddExperience(10); // Give XP

                            // Don't move to speak for a set amount of time.
                            _energyTimer = 5;
                            _speechTimer = 30;
                            _actionTimer = 45;
                            break;

                        #endregion

                        #region jump

                        case 7:
                            // Remove Status
                            RemovePetStatus();

                            // Add Status 
                            pet.Statusses.Add("jmp", TextHandling.GetString(pet.Z));
                            pet.UpdateNeeded = true;

                            pet.PetData.AddExperience(10); // Give XP

                            // Don't move to speak for a set amount of time.
                            _energyTimer = 5;
                            _speechTimer = 10;
                            _actionTimer = 5;
                            break;

                        #endregion

                        #region breed

                        case 46:
                            break;

                        #endregion

                        default:
                            string[] speech = PlusEnvironment.GetGame().GetChatManager().GetPetLocale().GetValue("pet.unknowncommand");

                            pet.Chat(speech[RandomNumber.GenerateRandom(0, speech.Length - 1)]);
                            break;
                    }

                    pet.PetData.PetEnergy(false); // Remove Energy
                }
                else
                {
                    RemovePetStatus(); // Remove Status

                    if (pet.PetData.Energy < 10)
                    {
                        string[] speech = PlusEnvironment.GetGame().GetChatManager().GetPetLocale().GetValue("pet.tired");

                        var randomSpeech = new Random();
                        pet.Chat(speech[RandomNumber.GenerateRandom(0, speech.Length - 1)]);

                        pet.Statusses.Add("lay", TextHandling.GetString(pet.Z));
                        pet.UpdateNeeded = true;

                        _speechTimer = 50;
                        _actionTimer = 45;
                        _energyTimer = 5;
                    }
                    else
                    {
                        string[] speech = PlusEnvironment.GetGame().GetChatManager().GetPetLocale().GetValue("pet.lazy");

                        var randomSpeech = new Random();
                        pet.Chat(speech[RandomNumber.GenerateRandom(0, speech.Length - 1)]);

                        pet.PetData.PetEnergy(false); // Remove Energy
                    }
                }
            }
            //Pet = null;
        }

        #endregion
    }
}