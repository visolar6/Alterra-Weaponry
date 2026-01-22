using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace VELD.AlterraWeaponry.Patches;

/// <summary>
/// Patches SeamothTorpedo to track ExplosiveTorpedos via their prefab origin
/// </summary>
public class SeamothTorpedo_MarkOnFire_Patch
{
    // Track which prefabs are ExplosiveTorpedos
    private static HashSet<string> explosivePrefabNames = new() { "ExplosiveTorpedo" };

    /// <summary>
    /// Patch OnHit to add marker when torpedo impacts
    /// Check if this torpedo instance came from ExplosiveTorpedo prefab
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(global::SeamothTorpedo), "OnHit")]
    private static void PrefixOnHit(global::SeamothTorpedo __instance)
    {
        // Try to get the prefab info
        string prefabName = __instance.gameObject.name;
        Main.logger.LogInfo($"[MarkOnFire] OnHit: {prefabName}");

        // Check if this looks like it came from ExplosiveTorpedo
        // The prefab is cloned from GasTorpedo but registered as ExplosiveTorpedo
        // We need to find another way to identify it

        var obj = __instance.gameObject;

        // Check hierarchy - look for custom components or markers
        var allComponents = obj.GetComponents<Component>();
        foreach (var comp in allComponents)
        {
            Main.logger.LogInfo($"[MarkOnFire] Component: {comp.GetType().Name}");
            if (comp.GetType().Name.Contains("Explosive"))
            {
                Main.logger.LogInfo($"[MarkOnFire] Found explosive component!");
                return;
            }
        }

        // Alternative: Check for marker that was added by prefab
        var existingMarker = __instance.GetComponent<VELD.AlterraWeaponry.Behaviours.ExplosiveTorpedoMarker>();
        if (existingMarker != null)
        {
            Main.logger.LogInfo("[MarkOnFire] Already has marker!");
            return;
        }
    }
}
