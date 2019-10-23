using System;
using System.Collections.Generic;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.API;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Teyhota.CustomKits
{
    public static class InventoryManager
    {
        public class Inventory
        {
            public List<Item> items;
            public Clothing clothes;

            public Inventory(List<Item> items, Clothing clothes)
            {
                this.items = items;
                this.clothes = clothes;
            }
        }

        public class Item
        {
            public ushort id;
            public byte[] meta;
            public byte page;
            public byte x;
            public byte y;
            public byte rot;

            public Item(ushort id, byte[] meta, byte page, byte x, byte y, byte rot)
            {
                this.id = id;
                this.meta = meta;
                this.page = page;
                this.x = x;
                this.y = y;
                this.rot = rot;
            }
        }

        public class ClothingData
        {
            public ushort id;
            public byte quality;
            public byte[] state;

            public ClothingData(ushort id, byte quality, byte[] state)
            {
                this.id = id;
                this.quality = quality;
                this.state = state;
            }
        }

        public class Clothing
        {
            public Hat hat;
            public Mask mask;
            public Shirt shirt;
            public Vest vest;
            public Backpack backpack;
            public Pants pants;

            public Clothing(Hat hat, Mask mask, Shirt shirt, Vest vest, Backpack backpack, Pants pants)
            {
                this.hat = hat;
                this.mask = mask;
                this.shirt = shirt;
                this.vest = vest;
                this.backpack = backpack;
                this.pants = pants;
            }
        }

        #region Clothing
        public class Backpack : ClothingData
        {
            public Backpack(ushort id, byte quality, byte[] state) : base(id, quality, state) { }
        }

        public class Hat : ClothingData
        {
            public Hat(ushort id, byte quality, byte[] state) : base(id, quality, state) { }
        }

        public class Mask : ClothingData
        {
            public Mask(ushort id, byte quality, byte[] state) : base(id, quality, state) { }
        }

        public class Pants : ClothingData
        {
            public Pants(ushort id, byte quality, byte[] state) : base(id, quality, state) { }
        }

        public class Shirt : ClothingData
        {
            public Shirt(ushort id, byte quality, byte[] state) : base(id, quality, state) { }
        }

        public class Vest : ClothingData
        {
            public Vest(ushort id, byte quality, byte[] state) : base(id, quality, state) { }
        }
        #endregion

        public static List<Item> ListItems(UnturnedPlayer player)
        {
            List<Item> itemList = new List<Item>();

            for (byte page = 0; page < PlayerInventory.PAGES - 1; page++)
            {
                for (byte index = 0; index < player.Inventory.getItemCount(page); index++)
                {
                    ItemJar iJar = player.Inventory.getItem(page, index);

                    itemList.Add(new Item(iJar.item.id, iJar.item.metadata, page, iJar.x, iJar.y, iJar.rot));
                }
            }

            return itemList;
        }

        public static void AddItem(UnturnedPlayer player, Item item)
        {
            SDG.Unturned.Item uItem = new SDG.Unturned.Item(item.id, true)
            {
                metadata = item.meta
            };

            if (item.page == 0 && item.x == 0 && item.y == 0 && item.rot == 0)
            {
                player.Inventory.tryAddItem(uItem, true, true);
            }
            else
            {
                player.Inventory.tryAddItem(uItem, item.x, item.y, item.page, item.rot);
            }
        }

        public static void AddClothing(UnturnedPlayer player, Backpack backpack, Clothing clothing = null)
        {
            if (clothing != null)
            {
                Hat hat = clothing.hat;
                Mask mask = clothing.mask;
                Shirt shirt = clothing.shirt;
                Vest vest = clothing.vest;
                Pants pants = clothing.pants;

                if (hat != null)
                {
                    player.Player.clothing.askWearHat(hat.id, hat.quality, hat.state, true);
                }
                if (mask != null)
                {
                    player.Player.clothing.askWearMask(mask.id, mask.quality, mask.state, true);
                }
                if (shirt != null)
                {
                    player.Player.clothing.askWearShirt(shirt.id, shirt.quality, shirt.state, true);
                }
                if (vest != null)
                {
                    player.Player.clothing.askWearVest(vest.id, vest.quality, vest.state, true);
                }
                if (pants != null)
                {
                    player.Player.clothing.askWearPants(pants.id, pants.quality, pants.state, true);
                }
            }

            if (backpack != null)
            {
                player.Player.clothing.askWearBackpack(backpack.id, backpack.quality, backpack.state, true);
            }
        }

        public static void Clear(UnturnedPlayer player, bool clothes)
        {
            // inventory...
            try
            {
                player.Player.equipment.dequip();
                for (byte p = 0; p < (PlayerInventory.PAGES - 1); p++)
                {
                    byte itemc = player.Player.inventory.getItemCount(p);
                    if (itemc > 0)
                    {
                        for (byte p1 = 0; p1 < itemc; p1++)
                        {
                            player.Player.inventory.removeItem(p, 0);
                        }
                    }
                }
                player.Player.channel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                {
                    (byte)0,
                    (byte)0,
                    new byte[0]
                });
                player.Player.channel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                {
                    (byte)1,
                    (byte)0,
                    new byte[0]
                });
            }
            catch (Exception e)
            {
                Logger.LogError("There was an error clearing " + player.CharacterName + "'s inventory.  Here is the error.");
                Logger.LogException(e);
            }

            // clothes...
            if (clothes)
            {
                try
                {
                    player.Player.clothing.askWearBackpack(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                    player.Player.clothing.askWearGlasses(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                    player.Player.clothing.askWearHat(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                    player.Player.clothing.askWearMask(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                    player.Player.clothing.askWearPants(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                    player.Player.clothing.askWearShirt(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                    player.Player.clothing.askWearVest(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError("There was an error clearing " + player.CharacterName + "'s inventory.  Here is the error.");
                    Logger.LogException(e);
                }
            }

            Events.InvokeClearInventory(player);
        }

        public static void Copy(UnturnedPlayer fromPlayer, UnturnedPlayer toPlayer, bool clothes)
        {
            PlayerClothing clothing = fromPlayer.Player.clothing;
            Hat hat = new Hat(clothing.hat, clothing.hatQuality, clothing.hatState);
            Mask mask = new Mask(clothing.mask, clothing.maskQuality, clothing.maskState);
            Shirt shirt = new Shirt(clothing.shirt, clothing.shirtQuality, clothing.shirtState);
            Vest vest = new Vest(clothing.vest, clothing.vestQuality, clothing.vestState);
            Backpack backpack = new Backpack(clothing.backpack, clothing.backpackQuality, clothing.backpackState);
            Pants pants = new Pants(clothing.pants, clothing.pantsQuality, clothing.pantsState);
            Clothing clothesList = new Clothing(hat, mask, shirt, vest, backpack, pants);

            List<Item> itemList = ListItems(fromPlayer);
            int inventoryCount = itemList.Count;

            Clear(toPlayer, clothes);

            if (clothes == true)
            {
                AddClothing(toPlayer, backpack, clothesList);
            }
            else
            {
                AddClothing(toPlayer, backpack);
            }

            for (int i = 0; i < itemList.Count; i++)
            {
                AddItem(toPlayer, itemList[i]);
            }
        }

        internal static void AutoCopy(UnturnedPlayer player)
        {
            if (Commands.Command_AutoCopy.Murdered.ContainsKey(player.CSteamID))
            {
                UnturnedPlayer murderer = UnturnedPlayer.FromCSteamID(Commands.Command_AutoCopy.Murdered[player.CSteamID]);

                if (murderer.HasPermission("ck.copyinventory.bypass"))
                {
                    UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("copy_bypass", murderer.CharacterName), Color.red);
                    return;
                }

                Copy(murderer, player, Plugin.CustomKitsPlugin.Instance.Configuration.Instance.IncludeClothingInKits);

                UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("copied", murderer.CharacterName), Color.green);
            }
        }
    }
}