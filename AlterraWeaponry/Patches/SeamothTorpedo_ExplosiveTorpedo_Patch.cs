using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace VELD.AlterraWeaponry.Patches;

[HarmonyPatch(typeof(SeamothTorpedo))]
public class SeamothTorpedo_ExplosiveTorpedo_Patch
{
    // Track which torpedoes have already exploded to prevent multiple explosions
    private static HashSet<int> explodedTorpedoes = new HashSet<int>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SeamothTorpedo), "Explode")]
    private static bool PatchExplode(SeamothTorpedo __instance)
    {
        int instanceId = __instance.gameObject.GetInstanceID();

        // Early exit if this torpedo already exploded
        if (explodedTorpedoes.Contains(instanceId))
        {
            return false; // Prevent default explosion
        }

        // Check for the marker component directly - don't rely on tracking
        var persistentMarker = __instance.GetComponent<ExplosiveTorpedoPersistentMarker>();
        var regularMarker = __instance.GetComponent<ExplosiveTorpedoMarker>();

        // If either marker exists, this is an ExplosiveTorpedo
        if (persistentMarker != null || regularMarker != null)
        {
            // Mark as exploded FIRST to prevent re-entry
            explodedTorpedoes.Add(instanceId);

            try
            {
                DamageSystem.RadiusDamage(
                    Main.Options.explosionDamage * Main.Options.explosionDamageMultiplier,
                    __instance.gameObject.transform.position,
                    Main.Options.explosionRadius,
                    DamageType.Explosive,
                    __instance.gameObject
                );

                // Spawn explosion VFX using Crashfish
                try
                {
                    // Use CoroutineHost to spawn Crashfish asynchronously
                    CoroutineHost.StartCoroutine(ExplosionVFX.SpawnCrashfishExplosionCoroutine(Main.Options.explosionRadius, __instance.gameObject.transform.position));
                }
                catch (Exception fxEx)
                {
                    Main.logger.LogWarning($"[ExplosiveTorpedo Patch] FX spawn failed: {fxEx.Message}");
                }

                // Destroy immediately
                UnityEngine.Object.Destroy(__instance.gameObject);
            }
            catch (Exception ex)
            {
                Main.logger.LogError($"[ExplosiveTorpedo Patch] Error: {ex.Message}\n{ex.StackTrace}");
            }

            return false;
        }

        // For regular torpedoes, allow default Explode() to run
        Main.logger.LogInfo("[ExplosiveTorpedo Patch] No marker found - allowing default explosion");
        return true;
    }
}
