using VELD.AlterraWeaponry.Mono.DepthCharge;

namespace VELD.AlterraWeaponry.Patches;

[HarmonyPatch(typeof(Pickupable))]
internal class ItemGoalTracker_Start_Patch
{

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Pickupable.Drop), [typeof(Vector3), typeof(Vector3), typeof(bool)])]
    public static void Drop_Postfix(Pickupable __instance)
    {
        // Trigger DepthCharge priming (if applicable)
        var depthChargeManager = __instance.GetComponent<DepthChargeManager>();
        depthChargeManager?.Prime();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Pickupable.Pickup))]
    public static void Pickup_Postfix(Pickupable __instance)
    {
        if (__instance == null)
            return;

        // Trigger DepthCharge state reset (if applicable)
        var depthChargeManager = __instance.GetComponent<DepthChargeManager>();
        if (depthChargeManager != null)
        {
            depthChargeManager.Deactivate();
            return;
        }

        TechType techType = __instance.GetTechType();
        // Creepvine → Unlock Coal recipe and Coal encyclopedia entry
        if (techType == TechType.CreepvineSeedCluster || techType == TechType.CreepvinePiece)
        {
            if (!KnownTech.Contains(Coal.TechType))
            {
                Main.logger.LogInfo($"Creepvine picked up! Unlocking Coal recipe.");
                KnownTech.Add(Coal.TechType);
            }
            // Unlock Coal encyclopedia entry
            if (!PDAEncyclopedia.ContainsEntry("Coal"))
            {
                Main.logger.LogInfo($"Creepvine picked up! Unlocking Coal encyclopedia entry.");
                PDAEncyclopedia.Add("Coal", true);
            }
        }
        // Coal → Unlock BlackPowder recipe and BlackPowder encyclopedia entry
        if (techType == Coal.TechType)
        {
            if (!KnownTech.Contains(BlackPowder.TechType))
            {
                Main.logger.LogInfo($"Coal picked up! Unlocking BlackPowder recipe.");
                KnownTech.Add(BlackPowder.TechType);
            }
            // Unlock BlackPowder encyclopedia entry
            if (!PDAEncyclopedia.ContainsEntry("BlackPowder"))
            {
                Main.logger.LogInfo($"Coal picked up! Unlocking BlackPowder encyclopedia entry.");
                PDAEncyclopedia.Add("BlackPowder", true);
            }
        }
        // BlackPowder → Unlock ExplosiveTorpedo/DepthCharge recipe and ExplosiveTorpedo/DepthCharge encyclopedia entry
        if (techType == BlackPowder.TechType)
        {
            // Unlock ExplosiveTorpedo and encyclopedia entry
            if (!KnownTech.Contains(ExplosiveTorpedo.TechType))
            {
                Main.logger.LogInfo($"BlackPowder picked up! Unlocking ExplosiveTorpedo recipe.");
                KnownTech.Add(ExplosiveTorpedo.TechType);
            }
            if (!PDAEncyclopedia.ContainsEntry("ExplosiveTorpedo"))
            {
                Main.logger.LogInfo($"BlackPowder picked up! Unlocking ExplosiveTorpedo encyclopedia entry.");
                PDAEncyclopedia.Add("ExplosiveTorpedo", true);
            }

            // Unlock DepthCharge and encyclopedia entry
            if (!KnownTech.Contains(DepthCharge.TechType))
            {
                Main.logger.LogInfo($"BlackPowder picked up! Unlocking DepthCharge recipe.");
                KnownTech.Add(DepthCharge.TechType);
            }
            if (!PDAEncyclopedia.ContainsEntry("DepthCharge"))
            {
                Main.logger.LogInfo($"BlackPowder picked up! Unlocking DepthCharge encyclopedia entry.");
                PDAEncyclopedia.Add("DepthCharge", true);
            }

            // Check if goal already completed
            if (StoryGoalManager.main != null && StoryGoalManager.main.IsGoalComplete("AWFirstLethal"))
            {
                Main.logger.LogInfo("AWFirstLethal goal already completed.");
            }
            else // Trigger the goal manually
            {
                Main.logger.LogInfo("Triggering AWFirstLethal story goal!");
                StoryGoal.Execute("AWFirstLethal", Story.GoalType.PDA);
            }
        }
    }
}
