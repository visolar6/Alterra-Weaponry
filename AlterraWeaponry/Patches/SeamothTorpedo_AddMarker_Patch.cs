// Disabled - instances are renamed to SeamothGasTorpedoProjectile(Clone) so name check won't work
// Replaced with SeamothTorpedo_MarkOnFire_Patch.cs which marks at fire time via Load() and OnHit()

/*
using HarmonyLib;
using UnityEngine;

namespace VELD.AlterraWeaponry.Patches;

public class SeamothTorpedo_AddMarker_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(global::SeamothTorpedo), "Awake")]
    private static void PostfixAwake(global::SeamothTorpedo __instance)
    {
        var existingMarker = __instance.GetComponent<VELD.AlterraWeaponry.Behaviours.ExplosiveTorpedoMarker>();
        if (existingMarker != null)
            return;

        if (__instance.name.Contains("ExplosiveTorpedo"))
        {
            Main.logger.LogInfo($"[AddMarker Patch] Adding marker to {__instance.name}");
            __instance.gameObject.AddComponent<VELD.AlterraWeaponry.Behaviours.ExplosiveTorpedoMarker>();
        }
    }
}
*/
