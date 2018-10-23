using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaliceManager : MonoBehaviour
{
    private static ChaliceManager instance;

    public static bool TryGetInstance(out ChaliceManager manager)
    {
        manager = instance;
        return (manager != null);
    }

    public List<ChaliceController> chalices = new List<ChaliceController>();

    private void Awake()
    {
        instance = this;
    }

    public void RegisterDrinkPress(PlayerController player)
    {
        PlayerData playerData;
        if (PlayerData.TryGetInstance(out playerData))
        {
            foreach (ChaliceController chalice in chalices)
            {
                if (chalice.playerControllersInPickupZone.Contains(player))
                {
                    if (player.DrunkLevel() != playerData.maxDrunkLevel && chalice.Drink())
                    {
                        player.AddDrunkLevel();
                    }

                    break;
                }
            }
        }
    }
}
