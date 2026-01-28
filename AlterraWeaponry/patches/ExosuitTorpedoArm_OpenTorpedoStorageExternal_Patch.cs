#if SN1

namespace VELD.AlterraWeaponry.Patches;

[HarmonyPatch(typeof(ExosuitTorpedoArm))]
public class ExosuitTorpedoArm_OpenTorpedoStorageExternal_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ExosuitTorpedoArm), nameof(ExosuitTorpedoArm.OpenTorpedoStorageExternal))]
    private static void PatchOpenTorpedoStorageExternal(ExosuitTorpedoArm __instance)
    {
        try
        {
            Main.logger.LogInfo("===== OPENTORPEDOSTORAGEEXTERNAL CALLED =====");
            Main.logger.LogInfo($"ExosuitTorpedoArm: {__instance.name}");

            var type = __instance.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Main.logger.LogInfo($"Searching {fields.Length} fields for ItemsContainer...");

            foreach (var field in fields)
            {
                try
                {
                    if (field.FieldType.Name.Contains("ItemsContainer"))
                    {
                        Main.logger.LogInfo($"Found ItemsContainer field: {field.Name}");
                        var container = field.GetValue(__instance);
                        AllowedTechUtils.AddTechTypeToAllowedTech(container, ExplosiveTorpedo.TechType, Main.logger);
                    }
                }
                catch (Exception ex)
                {
                    Main.logger.LogError($"Error processing field {field.Name}: {ex.Message}");
                }
            }
        }
        catch (Exception e)
        {
            Main.logger.LogError($"OpenTorpedoStorageExternal patch error: {e.Message}\n{e.StackTrace}");
        }
    }
}
#endif