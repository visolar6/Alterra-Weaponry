namespace VELD.AlterraWeaponry.Mono;

public class TorpedoExplosionBehaviour : MonoBehaviour
{
    public void Awake()
    {
    }

    public void Start()
    {
        Main.logger.LogInfo("Releasing explosion !");
        DamageSystem.RadiusDamage((250f * Main.Options.explosionDamageMultiplier), gameObject.transform.position, 10f, DamageType.Explosive, gameObject);
#if BELOWZERO
        try
        {
            var vfxMeteor = detonationPrefab.GetComponent<VFXMeteor>();
            if (vfxMeteor.impactPrefab == null)
                throw new Exception("vfxMeteor.impactPerfab is null.");
            global::Utils.PlayOneShotPS(vfxMeteor.impactPrefab, gameObject.transform.position, gameObject.transform.rotation);
            if (vfxMeteor.meteorCrashOSSound == null)
                throw new Exception("vfxMeteor.meteorCrashOSSound is null.");
            VFXWeatherManager.PlayOneShotSound(vfxMeteor.meteorCrashOSSound, gameObject.transform.position, 8f, Array.Empty<VFXWeatherManager.FmodParameter>());
        }
        catch(Exception e)
        {
            Main.logger.LogError($"An error has occured while exploding the torpedo.\n{e}");
        }
#elif SUBNAUTICA
        // FX loading is optional - damage is applied above and is what matters
#endif
        Destroy(gameObject, 10.0f);  // Long delay to let damage system finish
        Main.logger.LogInfo("Exploded !!!");
    }

    public static GameObject detonationPrefab;

    public static IEnumerator SetupDetonationPrefabAsync()
    {
        Main.logger.LogInfo($"{typeof(TorpedoExplosionBehaviour).FullName}: Setting up detonation prefab for explosive torpedo...");
        if (detonationPrefab != null)
        {
            Main.logger.LogInfo($"{typeof(TorpedoExplosionBehaviour).FullName}: detonationPrefab is already defined.");
            yield break;
        }

        // Try to get the Crashfish explosion prefab directly
        Main.logger.LogInfo($"{typeof(TorpedoExplosionBehaviour).FullName}: Loading ExplosionPrefab...");
        var explosionTask = PrefabDatabase.GetPrefabAsync("29619c37-13eb-4a21-b762-deac4cbe41fb"); // ExplosionPrefab
        yield return explosionTask;

        if (explosionTask.TryGetPrefab(out detonationPrefab))
        {
            Main.logger.LogInfo($"{typeof(TorpedoExplosionBehaviour).FullName}: Using ExplosionPrefab: {detonationPrefab.name}");
            yield break;
        }

        Main.logger.LogWarning($"{typeof(TorpedoExplosionBehaviour).FullName}: Failed to load ExplosionPrefab, falling back to meteor.");
    }
}
