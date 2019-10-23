using System.Collections.Generic;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.API;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Teyhota.CustomKits.Commands
{
    public class Command_GiveSlot : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "giveslot";

        public string Help => "Manually give players more slots to save their kits";

        public string Syntax => "/giveslot <player> <amount> <item limit>";

        public List<string> Aliases => new List<string> { "gs" };

        public List<string> Permissions => new List<string> { "ck.giveslot", "rocket.giveslot"};


        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 3)
            {
                if (caller is ConsolePlayer)
                {
                    //Plugin.CustomKitsPlugin.Write(Syntax, System.ConsoleColor.Red);
                    Logger.LogError($"Incorrect command usage! Try: {Syntax}");
                    return;
                }
                UnturnedChat.Say(caller, Syntax, Color.red);
                return;
            }

            UnturnedPlayer player = UnturnedPlayer.FromName(command[0]);
            ushort amount = ushort.Parse(command[1]);
            ushort limit = ushort.Parse(command[2]);

            if (player != null)
            {
                SlotManager.AddSlot(player, amount, limit);

                if (caller is ConsolePlayer)
                {
                    Logger.Log(Plugin.CustomKitsPlugin.Instance.Translate("gave_slot", player.DisplayName, amount, limit), System.ConsoleColor.Green);
                }
                else
                {
                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("gave_slot", player.DisplayName, amount, limit));
                }

                UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("received_slot", caller.DisplayName, amount, limit));
            }
            else
            {
                if (caller is ConsolePlayer)
                {
                    Logger.Log(Plugin.CustomKitsPlugin.Instance.Translate("player_doesn't_exist", player.CharacterName), System.ConsoleColor.Red);
                }
                else
                {
                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("player_doesn't_exist", player.CharacterName), Color.red);
                }
            }
        }
    }
}