namespace VELD.AlterraWeaponry.Patches;

[HarmonyPatch(typeof(SeaMoth))]
internal class SeaMoth_OpenTorpedoStorage_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SeaMoth.OpenTorpedoStorage))]
    private static void OpenTorpedoStorage(SeaMoth __instance)
    {
        try
        {
            var slots = __instance.GetSlotCount();
            Main.logger.LogDebug("Trying to open Seamoth torpedo bay storage. Adding TechType: " + ExplosiveTorpedo.TechType);
            for (int i = 0; i < slots; i++)
            {
                ItemsContainer storageInSlot = __instance.GetStorageInSlot(i, TechType.SeamothTorpedoModule);
                if (storageInSlot == null)
                {
                    Main.logger.LogWarning($"No storage found in slot {i}");
                    continue;
                }
                AllowedTechUtils.AddTechTypeToAllowedTech(storageInSlot, ExplosiveTorpedo.TechType, Main.logger);
            }
            Main.logger.LogDebug("Added torpedo techtypes to Seamoth torpedo bay filter.");
        }
        catch (Exception e)
        {
            Main.logger.LogError($"OpenTorpedoStorageExternal patch error: {e.Message}\n{e.StackTrace}");
        }
    }
}