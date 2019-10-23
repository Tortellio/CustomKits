using System.Collections.Generic;
using Rocket.Unturned.Player;

namespace Teyhota.CustomKits
{
    public static class SlotManager
    {
        public static Dictionary<ulong, List<Slot>> Slots;

        public class Slot
        {
            public ushort itemLimit;

            public Slot(ushort itemLimit)
            {
                this.itemLimit = itemLimit;
            }
        }

        public static int SlotCount(UnturnedPlayer player)
        {
            return Slots[player.CSteamID.m_SteamID].Count;
        }

        public static void AddSlot(UnturnedPlayer player, int amount, ushort itemLimit)
        {
            if (Slots.ContainsKey(player.CSteamID.m_SteamID))
            {
                for (int i = 0; i < amount; i++)
                {
                    Slots[player.CSteamID.m_SteamID].Add(new Slot(itemLimit));
                }
            }
            else
            {
                Slots.Add(player.CSteamID.m_SteamID, new List<Slot>());

                for (int i = 0; i < amount; i++)
                {
                    Slots[player.CSteamID.m_SteamID].Add(new Slot(itemLimit));
                }
            }
        }

        public static void RemoveSlot(UnturnedPlayer player, int amount)
        {
            if (!Slots.ContainsKey(player.CSteamID.m_SteamID)) return;

            for (int i = 0; i < amount; i++)
            {
                foreach (Slot slot in Slots[player.CSteamID.m_SteamID])
                {
                    Slots[player.CSteamID.m_SteamID].Remove(slot);
                }
            }
        }

        public static void ClearSlots(UnturnedPlayer player)
        {
            if (!Slots.ContainsKey(player.CSteamID.m_SteamID)) return;

            Slots.Remove(player.CSteamID.m_SteamID);
        }
    }
}