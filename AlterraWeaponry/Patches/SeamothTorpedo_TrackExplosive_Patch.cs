using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace VELD.AlterraWeaponry.Patches;

/// <summary>
/// Legacy tracking - not currently used but kept for reference
/// Current approach: Check components directly in Explode patch
/// </summary>
public class SeamothTorpedo_TrackExplosive_Patch
{
    // Store instance IDs of ExplosiveTorpedos (unused - kept for API compatibility)
    private static readonly HashSet<int> explosiveTorpedoIds = new();

    /// <summary>
    /// Check if a torpedo instance should explode (unused - kept for API compatibility)
    /// </summary>
    public static bool IsExplosiveTorpedo(GameObject torpedoGameObject)
    {
        if (torpedoGameObject == null) return false;
        int id = torpedoGameObject.GetInstanceID();
        bool result = explosiveTorpedoIds.Contains(id);
        Main.logger.LogInfo($"[TrackExplosive] IsExplosiveTorpedo check: ID {id}, Found: {result}, Set size: {explosiveTorpedoIds.Count}");
        return result;
    }

    /// <summary>
    /// Remove ID from tracking after explosion (unused - kept for API compatibility)
    /// </summary>
    public static void ClearTracking(GameObject torpedoGameObject)
    {
        if (torpedoGameObject == null) return;
        int id = torpedoGameObject.GetInstanceID();
        explosiveTorpedoIds.Remove(id);
        Main.logger.LogInfo($"[TrackExplosive] Cleared tracking for ID {id} (remaining: {explosiveTorpedoIds.Count})");
    }
}
