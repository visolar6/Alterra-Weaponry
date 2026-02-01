using System;
using System.Collections;
using System.Reflection;

namespace VELD.AlterraWeaponry.Utilities
{
    public static class AllowedTechUtils
    {
        public static void AddTechTypeToAllowedTech(object container, object techType, dynamic logger)
        {
            if (container == null)
            {
                Main.logger.LogWarning($"Container is null");
                return;
            }

            var containerType = container.GetType();
            var allowedTechField = containerType.GetField("allowedTech", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (allowedTechField != null)
            {
                var allowedTechValue = allowedTechField.GetValue(container);
                Main.logger.LogDebug($"  allowedTech type: {allowedTechValue?.GetType().FullName ?? "null"}");

                if (allowedTechValue is IList list)
                {
                    Main.logger.LogDebug($"  allowedTech count before: {list.Count}");
                    if (!list.Contains(techType))
                    {
                        list.Add(techType);
                        Main.logger.LogDebug($"  ✓ Added TechType!");
                    }
                    else
                    {
                        Main.logger.LogDebug($"  TechType already in list");
                    }
                }
                else
                {
                    Main.logger.LogWarning($"  allowedTech is not an IList, trying HashSet...");
                    if (allowedTechValue != null)
                    {
                        var hashSetType = allowedTechValue.GetType();
                        var addMethod = hashSetType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
                        var containsMethod = hashSetType.GetMethod("Contains", BindingFlags.Public | BindingFlags.Instance);

                        if (addMethod != null && containsMethod != null)
                        {
                            var contains = (bool)containsMethod.Invoke(allowedTechValue, [techType]);
                            Main.logger.LogDebug($"  ✓ Found HashSet! Contains TechType: {contains}");
                            if (!contains)
                            {
                                addMethod.Invoke(allowedTechValue, [techType]);
                                Main.logger.LogDebug($"  ✓ Added TechType to HashSet!");
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
    }
}
