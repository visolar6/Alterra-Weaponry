using VELD.AlterraWeaponry.Items.DepthCharge;

namespace VELD.AlterraWeaponry.Patches;

/// <summary>
/// Patches Pickupable.Pickup to reset DepthCharge state when picked up.
/// </summary>
[HarmonyPatch(typeof(Pickupable))]
public class Pickupable_Pickup_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Pickupable.Pickup))]
    public static void Pickup(Pickupable __instance)
    {
        var depthChargeManager = __instance.GetComponent<DepthChargeManager>();
        depthChargeManager?.Deactivate();
    }
}
