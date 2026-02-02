using VELD.AlterraWeaponry.Mono.DepthCharge;

namespace VELD.AlterraWeaponry.Patches;

/// <summary>
/// Patches IngameMenu.SaveGameAsync to save air bladder state when the player saves the game.
/// </summary>
[HarmonyPatch(typeof(IngameMenu), "SaveGameAsync")]
public class SaveGameAsync_Patch
{
    [HarmonyPrefix]
    public static void SaveGameAsync_Prefix()
    {
        // Update and save all depth charge states before the game is saved
        DepthChargeStateManager.Save();
    }
}
