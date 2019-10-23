using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Rocket.Unturned.Player;
using Rocket.Unturned.Items;
using Rocket.Unturned.Chat;
using Rocket.Unturned;
using Rocket.API;
using SDG.Unturned;
using UnityEngine;
using Newtonsoft.Json;
using Logger = Rocket.Core.Logging.Logger;

namespace Teyhota.CustomKits
{
    public static class KitManager
    {
        public static Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> Kits;
        public static Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> AutoSaveKits;

        internal static void AutoSave(UnturnedPlayer player)
        {
            string kitName = Commands.Command_AutoSave.AutoSave[player.CSteamID];
            InventoryManager.Inventory inventory = AutoSaveKits[player.CSteamID.m_SteamID][kitName];
            int inventoryCount = inventory.items.Count;

            if (!player.IsAdmin)
            {
                string[] blackList = new string[0];
                foreach (Plugin.CustomKitsConfig.Preset Preset in Plugin.CustomKitsPlugin.Instance.Configuration.Instance.Presets)
                {
                    if (player.HasPermission(Plugin.CustomKitsPlugin.PERMISSION + Preset.Name) && Preset.Blacklist != "")
                    {
                        blackList = Preset.Blacklist.Split(',');
                        break;
                    }
                    UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("set_permissions"), Color.red);
                    return;
                }

                if (KitCount(player, Kits) >= SlotManager.SlotCount(player))
                {
                    UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("no_kits_left"), Color.red);
                    return;
                }

                var v = KitCount(player, Kits);
                var slot = SlotManager.Slots[player.CSteamID.m_SteamID][v];

                int itemLimit = slot.itemLimit;
                if (blackList.Length > 0)
                {
                    foreach (InventoryManager.Item item in inventory.items)
                    {
                        List<int> bList = new List<int>();
                        foreach (var itemID in blackList)
                        {
                            bList.Add(int.Parse(itemID));
                        }

                        if (bList.Contains(item.id))
                        {
                            UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("blacklisted", UnturnedItems.GetItemAssetById(item.id)), Color.red);
                        }
                    }
                }

                if (inventoryCount > itemLimit)
                {
                    UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("item_limit", itemLimit), Color.red);
                    return;
                }
            }

            if (inventoryCount < 1 || inventory.items == null)
            {
                UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("empty_inventory"), Color.red);
                return;
            }

            if (HasKit(player, kitName, KitManager.Kits))
            {
                DeleteKit(player, kitName, KitManager.Kits);
            }

            SaveKit(player, player, kitName, KitManager.Kits);
            UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("kit_saved", kitName), Color.green);

            // Auto off
            Commands.Command_AutoSave.AutoSave.Remove(player.CSteamID);
            UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("autosave_off"), Color.green);
        }

        public static void SaveKit(UnturnedPlayer fromPlayer, UnturnedPlayer toPlayer, string kitName, Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> database)
        {
            PlayerClothing clothing = fromPlayer.Player.clothing;
            InventoryManager.Hat hat = new InventoryManager.Hat(clothing.hat, clothing.hatQuality, clothing.hatState);
            InventoryManager.Mask mask = new InventoryManager.Mask(clothing.mask, clothing.maskQuality, clothing.maskState);
            InventoryManager.Shirt shirt = new InventoryManager.Shirt(clothing.shirt, clothing.shirtQuality, clothing.shirtState);
            InventoryManager.Vest vest = new InventoryManager.Vest(clothing.vest, clothing.vestQuality, clothing.vestState);
            InventoryManager.Backpack backpack = new InventoryManager.Backpack(clothing.backpack, clothing.backpackQuality, clothing.backpackState);
            InventoryManager.Pants pants = new InventoryManager.Pants(clothing.pants, clothing.pantsQuality, clothing.pantsState);
            InventoryManager.Clothing clothesList = new InventoryManager.Clothing(hat, mask, shirt, vest, backpack, pants);

            List<InventoryManager.Item> itemList = new List<InventoryManager.Item>();
            string[] blackList = new string[0];
            foreach (Plugin.CustomKitsConfig.Preset Preset in Plugin.CustomKitsPlugin.Instance.Configuration.Instance.Presets)
            {
                if (toPlayer.HasPermission(Plugin.CustomKitsPlugin.PERMISSION + Preset.Name))
                {
                    if (Preset.Blacklist != "")
                    {
                        blackList = Preset.Blacklist.Split(',');
                        break;
                    }
                }
            }

            List<int> bList = new List<int>();
            if (blackList.Length > 0)
            {
                foreach (var itemID in blackList)
                {
                    bList.Add(int.Parse(itemID));
                }
            }

            //foreach (Items page in fromPlayer.Inventory.items)
            //{
            //    if (page == null || page.items == null || page.items.Count == 0)
            //        continue;
            //    int itemsCount = page.items.Count;
            //    foreach (ItemJar item in page.items)
            //    {

            //    }
            //}

            for (byte page = 0; page < fromPlayer.Inventory.items.Length; page++)
            {
                if (fromPlayer.Inventory.items[page] == null || fromPlayer.Inventory.items[page].items == null || fromPlayer.Inventory.items[page].items.Count == 0)
                    continue;
                for (byte index = 0; index < fromPlayer.Inventory.getItemCount(page); index++)
                {
                    ItemJar iJar = fromPlayer.Inventory.getItem(page, index);
                    if (!toPlayer.IsAdmin && bList.Contains(iJar.item.id))
                        continue;

                    itemList.Add(new InventoryManager.Item(iJar.item.id, iJar.item.metadata, page, iJar.x, iJar.y, iJar.rot));
                }
            }

            if (database.ContainsKey(toPlayer.CSteamID.m_SteamID))
            {
                if (database[toPlayer.CSteamID.m_SteamID].ContainsKey(kitName))
                {
                    database[toPlayer.CSteamID.m_SteamID][kitName] = new InventoryManager.Inventory(itemList, clothesList);
                }
                else
                {
                    database[toPlayer.CSteamID.m_SteamID].Add(kitName, new InventoryManager.Inventory(itemList, clothesList));
                }
            }
            else
            {
                Dictionary<string, InventoryManager.Inventory> kit = new Dictionary<string, InventoryManager.Inventory>
                {
                    { kitName, new InventoryManager.Inventory(itemList, clothesList) }
                };

                database.Add(toPlayer.CSteamID.m_SteamID, kit);
            }

            Events.InvokeSaveKit(fromPlayer, toPlayer, kitName);
        }

        public static void LoadKit(UnturnedPlayer fromPlayer, UnturnedPlayer toPlayer, string kitName, bool clothes, Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> database)
        {
            if (clothes == true)
            {
                InventoryManager.AddClothing(toPlayer, database[fromPlayer.CSteamID.m_SteamID][kitName].clothes.backpack, database[fromPlayer.CSteamID.m_SteamID][kitName].clothes);
            }
            else
            {
                InventoryManager.AddClothing(toPlayer, database[fromPlayer.CSteamID.m_SteamID][kitName].clothes.backpack);
            }

            for (int i = 0; i < database[fromPlayer.CSteamID.m_SteamID][kitName].items.Count; i++)
            {
                InventoryManager.AddItem(toPlayer, database[fromPlayer.CSteamID.m_SteamID][kitName].items[i]);
            }

            Events.InvokeLoadKit(fromPlayer, toPlayer, kitName, clothes);
        }

        public static void DeleteKit(UnturnedPlayer player, string kitName, Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> database)
        {
            if (kitName == "*")
            {
                database[player.CSteamID.m_SteamID].Clear();
            }
            else
            {
                database[player.CSteamID.m_SteamID].Remove(kitName);
            }

            Events.InvokeDelKit(player, kitName);
        }

        public static int KitCount(UnturnedPlayer player, Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> database) => database[player.CSteamID.m_SteamID].Count;

        public static bool HasKit(UnturnedPlayer player, string kitName, Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> database) => database[player.CSteamID.m_SteamID].ContainsKey(kitName);

        public static bool HasSavedKits(UnturnedPlayer player, Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> database) => database[player.CSteamID.m_SteamID].Count >= 0 || database.ContainsKey(player.CSteamID.m_SteamID);

        public static void TryStoreKits()
        {
            if (Plugin.CustomKitsPlugin.Instance.Configuration.Instance.KeepKitsOnRestart == true)
            {
                string json;
                try
                {
                    json = JsonConvert.SerializeObject(Kits);
                }
                catch
                {
                    Logger.LogError("An error has occured while serializing kits");
                    return;
                }

                if (File.Exists(Plugin.CustomKitsPlugin.ThisDirectory + "StoredKits.json"))
                {
                    File.WriteAllText(Plugin.CustomKitsPlugin.ThisDirectory + "StoredKits.json", json);
                }
                else
                {
                    File.Create(Plugin.CustomKitsPlugin.ThisDirectory + "StoredKits.json").Close();
                    File.WriteAllText(Plugin.CustomKitsPlugin.ThisDirectory + "StoredKits.json", json);
                }
            }
            else
            {
                Logger.LogError("KeepKitsOnRestart must be disabled!");
            }
        }

        internal static IEnumerator DelayedLoad(UnturnedPlayer player, string kitName, float delay)
        {
            yield return new WaitForSeconds(delay);

            LoadKit(player, player, kitName, Plugin.CustomKitsPlugin.Instance.Configuration.Instance.IncludeClothingInKits, Kits);
            UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("kit_loaded", kitName), Color.green);
        }

        internal static IEnumerator AutoStoreKits()
        {
            int time = U.Settings.Instance.AutomaticSave.Interval;

            if (time <= 30)
            {
                time = 30;
            }

            while (U.Settings.Instance.AutomaticSave.Enabled)
            {
                yield return new WaitForSeconds(time);

                TryStoreKits();

                Logger.Log(Plugin.CustomKitsPlugin.Instance.Translate("auto_stored"), ConsoleColor.Green);
            }
        }
    }
}