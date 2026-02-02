namespace VELD.AlterraWeaponry.Mono.DepthCharge
{
    public class DepthChargeDamager : MonoBehaviour, IOnTakeDamage
    {
        public LiveMixin? liveMixin;

        public void OnTakeDamage(DamageInfo damageInfo)
        {
            var behavior = GetComponent<DepthChargeBehavior>();
            if (behavior == null || behavior.IsDetonated || damageInfo == null)
                return;


            // Only react to Explosive or Fire damage
            if (damageInfo.type != DamageType.Explosive && damageInfo.type != DamageType.Fire)
                return;

            // Force detonation shortly after taking damage (doesn't need to be primed)
            behavior.Detonate(0.5f, true);
        }
    }
}