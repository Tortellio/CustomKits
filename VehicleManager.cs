using System.Collections;
using System.Collections.Generic;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using Steamworks;

namespace Teyhota.CustomKits
{
    public static class VehicleManager
    {
        public static Dictionary<CSteamID, List<InteractableVehicle>> CurrentVehicles;

        public static void DestroyVehicle(InteractableVehicle vehicle)
        {
            if (!vehicle.isEmpty)
            {
                vehicle.forceRemoveAllPlayers();
            }

            SDG.Unturned.VehicleManager.askVehicleDestroy(vehicle);
        }

        public static IEnumerator LimitVehicles(UnturnedPlayer player)
        {
            yield return new WaitForSeconds(2.5f);

            if (CurrentVehicles.ContainsKey(player.CSteamID))
            {
                foreach (var car in CurrentVehicles[player.CSteamID])
                {
                    DestroyVehicle(car);
                }

                CurrentVehicles[player.CSteamID].Clear();
            }
            else
            {
                CurrentVehicles.Add(player.CSteamID, new List<InteractableVehicle>());
            }

            foreach (var car in SDG.Unturned.VehicleManager.vehicles)
            {
                if ((car.transform.position - player.Position).magnitude < 28f)
                {
                    CurrentVehicles[player.CSteamID].Add(car);
                }
            }
        }
    }
}