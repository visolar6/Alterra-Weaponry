namespace VELD.AlterraWeaponry.Patches;

[HarmonyPatch(typeof(Pickupable))]
internal class ItemGoalTracker_Start_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Pickupable.Pickup))]
    public static void Pickup_Postfix(Pickupable __instance)
    {
        if (__instance == null)
            return;

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

        // BlackPowder → Unlock ExplosiveTorpedo recipe and ExplosiveTorpedo encyclopedia entry
        if (techType == BlackPowder.TechType)
        {
            if (!KnownTech.Contains(ExplosiveTorpedo.TechType))
            {
                Main.logger.LogInfo($"BlackPowder picked up! Unlocking ExplosiveTorpedo recipe.");
                KnownTech.Add(ExplosiveTorpedo.TechType);
            }
            // Unlock ExplosiveTorpedo encyclopedia entry
            if (!PDAEncyclopedia.ContainsEntry("ExplosiveTorpedo"))
            {
                Main.logger.LogInfo($"BlackPowder picked up! Unlocking ExplosiveTorpedo encyclopedia entry.");
                PDAEncyclopedia.Add("ExplosiveTorpedo", true);
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
