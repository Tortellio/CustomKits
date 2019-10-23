using Rocket.Unturned.Player;

namespace Teyhota.CustomKits
{

    public static class Events
    {
        // Kit saved
        public delegate void SaveKitDelegate(UnturnedPlayer player, UnturnedPlayer toPlayer, string kitName);
        public static event SaveKitDelegate OnKitSaved;

        public static void InvokeSaveKit(UnturnedPlayer player, UnturnedPlayer toPlayer, string kitName)
        {
            if (OnKitSaved != null)
            {
                OnKitSaved.Invoke(player, toPlayer, kitName);
            }
        }
        
        // Kit loaded
        public delegate void LoadKitDelegate(UnturnedPlayer player, UnturnedPlayer toPlayer, string kitName, bool clothes);
        public static event LoadKitDelegate OnKitLoaded;

        public static void InvokeLoadKit(UnturnedPlayer player, UnturnedPlayer toPlayer, string kitName, bool clothes)
        {
            if (OnKitLoaded != null)
            {
                OnKitLoaded.Invoke(player, toPlayer, kitName, clothes);
            }
        }
        
        // Kit deleted
        public delegate void DelKitDelegate(UnturnedPlayer player, string kitName);
        public static event DelKitDelegate OnKitDeleted;

        public static void InvokeDelKit(UnturnedPlayer player, string kitName)
        {
            if (OnKitDeleted != null)
            {
                OnKitDeleted.Invoke(player, kitName);
            }
        }

        // Inventory cleared
        public delegate void ClearInventoryDelegate(UnturnedPlayer player);
        public static event ClearInventoryDelegate OnInventoryCleared;

        public static void InvokeClearInventory(UnturnedPlayer player)
        {
            if (OnInventoryCleared != null)
            {
                OnInventoryCleared.Invoke(player);
            }
        }
    }
}