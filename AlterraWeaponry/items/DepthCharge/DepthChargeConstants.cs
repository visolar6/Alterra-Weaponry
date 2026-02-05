namespace VELD.AlterraWeaponry.Items.DepthCharge;

public static class DepthChargeConstants
{
    // Mesh values
    internal static readonly float meshSphereRadius = 0.4f;
    internal static readonly float meshSpikeLength = 0.2f;
    internal static readonly float meshSpikeBaseRadius = 0.05f;
    internal static readonly float meshSpikeTipRadius = 0.01f;
    internal static readonly int meshSpikeCount = 24;
    internal static readonly int meshSpikeRings = 4;

    // Arming timing
    public static readonly int primingIndicatorCount = 3;
    public static readonly float primingIndicatorInterval = 1f;
    public static readonly float primingIndicatorDuration = 0.2f;
    public static readonly float primingIndicatorIntensity = 0.75f;
    public static readonly Color primingIndicatorEmission = Color.yellow * 3f;

    // Armed initial timing
    public static readonly int armedIndicatorInitialCount = 3;
    public static readonly float armedIndicatorInitialInterval = 0.25f;
    public static readonly float armedIndicatorInitialDuration = 0.1f;
    public static readonly float armedIndicatorInitialIntensity = 1f;
    public static readonly Color armedIndicatorInitialEmission = Color.red * 3f;

    // Armed cycle timing
    public static readonly float armedIndicatorCycleInterval = 3f;
    public static readonly float armedIndicatorCycleDuration = 0.5f;
    public static readonly float armedIndicatorCycleIntensity = 0.5f;
    public static readonly Color armedIndicatorCycleEmission = Color.red * 3f;

    // Collision timing
    public static readonly int collisionIndicatorCount = 5;
    public static readonly float collisionIndicatorInterval = 0.1f;
    public static readonly float collisionIndicatorDuration = 0.05f;
    public static readonly float collisionIndicatorIntensity = 1f;
    public static readonly Color collisionIndicatorEmission = Color.red * 5f;

    // Collision detection
    public static readonly float collisionMassThreshold = 50f; // Player character's mass is 70f
    public static readonly float collisionVelocityThreshold = 1f;

    // Explosion intensity
    public static readonly float explosionRadius = 50f;
    public static readonly float explosionDamage = 10000f;
    public static readonly float explosionDamageFalloff = 0.5f;
}