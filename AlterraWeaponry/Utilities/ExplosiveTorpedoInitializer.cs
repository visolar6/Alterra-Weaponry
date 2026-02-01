namespace VELD.AlterraWeaponry.Utilities;

internal class ExplosiveTorpedoInitializer // Thanks to Grimm The Second !
{
    public static TorpedoType torpedoType { get; private set; }

    public static void InitPrefab(GameObject prefab)
    {
        if (torpedoType == null)
        {
            if (!prefab)
            {
                Main.logger.LogError("ExplosiveTorpedoBehaviour.InitPrefab() -> invalid prefab for torpedo.");
            }
            else
            {
                try
                {
                    // Save the original explosion prefab before modifying anything
                    var originalExplosionPrefab = prefab.GetComponent<SeamothTorpedo>().explosionPrefab;
                    Main.logger.LogInfo($"[ExplosiveTorpedoInitializer] Original explosionPrefab: {(originalExplosionPrefab != null ? originalExplosionPrefab.name : "null")}");

                    CoroutineHost.StartCoroutine(TorpedoExplosionBehaviour.SetupDetonationPrefabAsync());
                    Main.logger.LogInfo("Initializing TorpedoExplosionBehaviour TorpedoExplosion prefab...");

                    // Don't override the explosionPrefab - keep the original GasTorpedo explosion
                    // The damage is handled in our Explode patch, we just need the visual effect from the original
                    prefab.GetComponent<SeamothTorpedo>().homingTorpedo = true;

                    // Add marker component to the prefab so instances inherit it
                    Main.logger.LogInfo("[ExplosiveTorpedoInitializer] Adding ExplosiveTorpedoMarker to prefab");
                    prefab.AddComponent<VELD.AlterraWeaponry.Mono.ExplosiveTorpedoMarker>();

                    torpedoType = new()
                    {
                        techType = ExplosiveTorpedo.TechType,
                        prefab = prefab
                    };
                    Main.logger.LogInfo("[ExplosiveTorpedoInitializer] TorpedoType initialized successfully");
                }
                catch (Exception ex)
                {
                    Main.logger.LogError($"An error has occured while initializing torpedo prefab.\n{ex}");
                }
            }
        }
    }
}
