using VELD.AlterraWeaponry.Mono.DepthCharge;

namespace VELD.AlterraWeaponry.Patches;

[HarmonyPatch(typeof(LiveMixin))]
internal class LiveMixin_TakeDamage_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(LiveMixin.TakeDamage))]
    internal static void TakeDamage_Postfix(LiveMixin __instance, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
    {
        var depthChargeDamager = __instance.GetComponent<DepthChargeDamager>();
        if (depthChargeDamager == null)
            return;

        depthChargeDamager?.OnTakeDamage(new DamageInfo
        {
            damage = originalDamage,
            type = type,
            position = position,
            dealer = dealer
        });
    }
}