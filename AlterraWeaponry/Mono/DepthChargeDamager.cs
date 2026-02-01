namespace VELD.AlterraWeaponry.Mono
{
    public class DepthChargeDamager : MonoBehaviour, IOnTakeDamage
    {
        public LiveMixin liveMixin;

        public void OnTakeDamage(DamageInfo damageInfo)
        {
            var behavior = GetComponent<DepthChargeBehavior>();
            if (behavior == null || behavior.IsDetonated || damageInfo == null)
            {
                Main.logger.LogInfo("DepthChargeDamaged: No behavior or already detonated or no damage info");
                return;
            }

            // Only react to Explosive or Fire damage
            if (damageInfo.type != DamageType.Explosive && damageInfo.type != DamageType.Fire)
            {
                Main.logger.LogInfo($"DepthChargeDamaged: Ignoring damage type {damageInfo.type}");
                return;
            }

            // Detonate shortly after taking damage
            Main.logger.LogInfo($"DepthChargeDamaged: Taking damage of type {damageInfo.type}, triggering detonation");
            behavior.Detonate(0.1f);
        }

        public void OnKill()
        {
            Main.logger.LogInfo("DepthChargeDamaged: OnKill called, detonating depth charge");
        }
    }
}