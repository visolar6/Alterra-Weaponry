namespace VELD.AlterraWeaponry.Utilities;

/// <summary>
/// Patches the BreakableResource class to handle custom outcrop drops.
/// </summary>
#if SN1
internal static class BreakableResourcePatcher
{
    /// <summary>
    /// Tracks custom drop data for each outcrop TechType.
    /// </summary>
    public static IDictionary<TechType, List<OutcropDropData>> CustomDrops = new Dictionary<TechType, List<OutcropDropData>>();

    /// <summary>
    /// Applies the Harmony patches for outcrop drops.
    /// </summary>
    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(BreakableResourcePatcher));
        Main.logger.LogInfo("Finished patching BreakableResource.");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HandTarget), nameof(HandTarget.Awake))]
    private static void Awake(HandTarget __instance)
    {
        if (__instance is not BreakableResource breakableResource)
            return;

        TechType outcropTechType = CraftData.GetTechType(__instance.gameObject);
        try
        {
            foreach (BreakableResource.RandomPrefab randPrefab in breakableResource.prefabList)
                OutcropsUtils.SetOutcropDrop(outcropTechType, randPrefab.prefabTechType, randPrefab.chance);
        }
        catch (Exception e)
        {
            Main.logger.LogError($"An error occurred while patching HandTarget.Awake() when adding original drops for outcrop {outcropTechType}.\n{e}");
        }
    }
}
#endif