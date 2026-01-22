using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace VELD.AlterraWeaponry.Patches;

[HarmonyPatch(typeof(global::SeamothTorpedo))]
public class SeamothTorpedo_ExplosiveTorpedo_Patch
{
    // Track which torpedoes have already exploded to prevent multiple explosions
    private static HashSet<int> explodedTorpedoes = new HashSet<int>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(global::SeamothTorpedo), "Explode")]
    private static bool PatchExplode(global::SeamothTorpedo __instance)
    {
        int instanceId = __instance.gameObject.GetInstanceID();

        // Early exit if this torpedo already exploded
        if (explodedTorpedoes.Contains(instanceId))
        {
            return false; // Prevent default explosion
        }

        Main.logger.LogInfo($"[ExplosiveTorpedo Patch] Explode() called for {__instance.gameObject.name}");

        // Check for the marker component directly - don't rely on tracking
        var persistentMarker = __instance.GetComponent<VELD.AlterraWeaponry.Behaviours.ExplosiveTorpedoPersistentMarker>();
        var regularMarker = __instance.GetComponent<VELD.AlterraWeaponry.Behaviours.ExplosiveTorpedoMarker>();

        Main.logger.LogInfo($"[ExplosiveTorpedo Patch] HasPersistentMarker: {persistentMarker != null}, HasMarker: {regularMarker != null}");

        // If either marker exists, this is an ExplosiveTorpedo
        if (persistentMarker != null || regularMarker != null)
        {
            // Mark as exploded FIRST to prevent re-entry
            explodedTorpedoes.Add(instanceId);

            Main.logger.LogInfo("[ExplosiveTorpedo Patch] MATCH! Found marker - applying custom explosion");
            try
            {
                Main.logger.LogInfo($"[ExplosiveTorpedo Patch] Calling DamageSystem.RadiusDamage at position {__instance.gameObject.transform.position}");

                DamageSystem.RadiusDamage(
                    (Main.Options.explosionDamage * Main.Options.explosionDamageMultiplier),
                    __instance.gameObject.transform.position,
                    Main.Options.explosionRadius,
                    DamageType.Explosive,
                    __instance.gameObject
                );

                Main.logger.LogInfo("[ExplosiveTorpedo Patch] Damage applied");

                // Spawn explosion VFX using Crashfish
                try
                {
                    // Use CoroutineHost to spawn Crashfish asynchronously
                    CoroutineHost.StartCoroutine(SpawnCrashfishExplosion(__instance.gameObject.transform.position));
                }
                catch (System.Exception fxEx)
                {
                    Main.logger.LogWarning($"[ExplosiveTorpedo Patch] FX spawn failed: {fxEx.Message}");
                }

                // Destroy immediately
                Main.logger.LogInfo("[ExplosiveTorpedo Patch] Destroying torpedo immediately");
                UnityEngine.Object.Destroy(__instance.gameObject);
            }
            catch (System.Exception ex)
            {
                Main.logger.LogError($"[ExplosiveTorpedo Patch] Error: {ex.Message}\n{ex.StackTrace}");
            }

            return false;
        }

        // For regular torpedoes, allow default Explode() to run
        Main.logger.LogInfo("[ExplosiveTorpedo Patch] No marker found - allowing default explosion");
        return true;
    }

    private static IEnumerator SpawnCrashfishExplosion(Vector3 position)
    {
        Main.logger.LogInfo("[ExplosiveTorpedo Patch] Loading Crashfish prefab for explosion particle...");
        var crashfishTask = CraftData.GetPrefabForTechTypeAsync(TechType.Crash);
        yield return crashfishTask;

        var crashfishPrefab = crashfishTask.GetResult();
        if (crashfishPrefab != null)
        {
            var crash = crashfishPrefab.GetComponent<Crash>();

            if (crash != null && crash.detonateParticlePrefab != null)
            {
                // Instantiate the explosion particle and scale it
                float scale = Main.Options.explosionRadius / 10f; // 10m radius = 1.00x scale
                var explosion = UnityEngine.Object.Instantiate(crash.detonateParticlePrefab, position, Quaternion.identity);
                explosion.transform.localScale = Vector3.one * scale;

                // Set proper scaling mode and apply scale to particle systems
                var particleSystems = explosion.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    var main = ps.main;
                    main.scalingMode = ParticleSystemScalingMode.Hierarchy; // Makes particles scale properly with transform
                    main.startSizeMultiplier *= scale; // Also scale the particle sizes
                    main.startSpeedMultiplier *= scale; // Scale particle speed to match size
                    main.startColor = new ParticleSystem.MinMaxGradient(Main.Options.explosionColor); // Apply custom color
                    ps.Play();
                }

                Main.logger.LogInfo($"[ExplosiveTorpedo Patch] Crashfish explosion particles spawned with scale {scale:F2}x");
                Main.logger.LogInfo($"[ExplosiveTorpedo Patch] Explosion color: R={Main.Options.explosionColor.r:F3}, G={Main.Options.explosionColor.g:F3}, B={Main.Options.explosionColor.b:F3}, A={Main.Options.explosionColor.a:F3}");
            }
            else
            {
                Main.logger.LogWarning("[ExplosiveTorpedo Patch] Could not get detonateParticlePrefab from Crashfish");
            }

        }
    }
}
