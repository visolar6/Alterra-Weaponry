namespace VELD.AlterraWeaponry.Patches;

[HarmonyPatch(typeof(KnownTech))]
internal class PDAScanner_Add_Patch
{
    private static int prawnLaserArmFragmentsScanned = 0;
    private const int requiredFragments = 3;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(KnownTech.Add))]
    public static void Add_Postfix(TechType techType)
    {
        // Track PrawnLaserArm fragment scans - fires when tech is added to KnownTech
        if (techType == PrawnLaserArm.TechType)
        {
            // Check if this is a fragment scan (not the final blueprint unlock)
            // Fragments have the same TechType but are tracked separately in PDAScanner
            if (PDAScanner.GetEntryData(techType) != null &&
                PDAScanner.GetEntryData(techType).isFragment)
            {
                prawnLaserArmFragmentsScanned++;
                Main.logger.LogInfo($"PrawnLaserArm fragment scanned! Progress: {prawnLaserArmFragmentsScanned}/{requiredFragments}");

                // Unlock blueprint and encyclopedia entry after 3 scans
                if (prawnLaserArmFragmentsScanned >= requiredFragments)
                {
                    if (!KnownTech.Contains(PrawnLaserArm.TechType))
                    {
                        Main.logger.LogInfo($"Unlocked PrawnLaserArm blueprint after {requiredFragments} scans!");
                        KnownTech.Add(PrawnLaserArm.TechType);
                    }

                    // Encyclopedia entry is handled automatically when recipe is unlocked
                    // KnownTech.Add() automatically unlocks related encyclopedia entries
                }
            }
        }
    }
}
