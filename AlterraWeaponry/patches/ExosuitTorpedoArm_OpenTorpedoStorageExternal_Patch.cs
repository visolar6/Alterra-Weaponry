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
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Main.logger.LogInfo($"Searching {fields.Length} fields for ItemsContainer...");

            foreach (var field in fields)
            {
                try
                {
                    if (field.FieldType.Name.Contains("ItemsContainer"))
                    {
                        Main.logger.LogInfo($"Found ItemsContainer field: {field.Name}");

                        var container = field.GetValue(__instance);
                        if (container != null)
                        {
                            var containerType = container.GetType();
                            Main.logger.LogInfo($"  Container type: {containerType.FullName}");

                            var allowedTechField = containerType.GetField("allowedTech", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                            if (allowedTechField != null)
                            {
                                var allowedTechValue = allowedTechField.GetValue(container);
                                Main.logger.LogInfo($"  allowedTech type: {allowedTechValue?.GetType().FullName ?? "null"}");

                                var list = allowedTechValue as System.Collections.IList;
                                if (list != null)
                                {
                                    Main.logger.LogInfo($"  allowedTech count before: {list.Count}");
                                    Main.logger.LogInfo($"  ExplosiveTorpedo.TechType: {ExplosiveTorpedo.TechType}");

                                    if (!list.Contains(ExplosiveTorpedo.TechType))
                                    {
                                        list.Add(ExplosiveTorpedo.TechType);
                                        Main.logger.LogInfo($"  ✓ Added ExplosiveTorpedo!");
                                        Main.logger.LogInfo($"  allowedTech count after: {list.Count}");
                                    }
                                    else
                                    {
                                        Main.logger.LogInfo($"  ExplosiveTorpedo already in list");
                                    }
                                }
                                else
                                {
                                    Main.logger.LogWarning($"  allowedTech is not an IList, trying HashSet...");

                                    // Try HashSet<TechType>
                                    if (allowedTechValue != null)
                                    {
                                        var hashSetType = allowedTechValue.GetType();
                                        var addMethod = hashSetType.GetMethod("Add", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                                        var containsMethod = hashSetType.GetMethod("Contains", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                                        if (addMethod != null && containsMethod != null)
                                        {
                                            var contains = (bool)containsMethod.Invoke(allowedTechValue, new object[] { ExplosiveTorpedo.TechType });
                                            Main.logger.LogInfo($"  ✓ Found HashSet! Contains ExplosiveTorpedo: {contains}");

                                            if (!contains)
                                            {
                                                addMethod.Invoke(allowedTechValue, new object[] { ExplosiveTorpedo.TechType });
                                                Main.logger.LogInfo($"  ✓ Added ExplosiveTorpedo to HashSet!");
                                            }
                                        }
                                        else
                                        {
                                            Main.logger.LogWarning($"  No Add/Contains methods found");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Main.logger.LogWarning($"  allowedTech field not found on container");
                            }
                        }
                        else
                        {
                            Main.logger.LogWarning($"  Container is null");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Main.logger.LogError($"Error processing field {field.Name}: {ex.Message}");
                }
            }

            Main.logger.LogInfo("===== PATCH COMPLETE =====");
        }
        catch (Exception e)
        {
            Main.logger.LogError($"OpenTorpedoStorageExternal patch error: {e.Message}\n{e.StackTrace}");
        }
    }
}
#endif