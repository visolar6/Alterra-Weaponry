// namespace VELD.AlterraWeaponry.Mono.DepthCharge;

// public class DepthChargeBehavior : MonoBehaviour
// {
//     // Explosion parameters
//     private const float explosionRadius = 25f;
//     private const float explosionDamage = 2000f;

//     // State timing constants
//     private const int primingCountdown = 3; // Seconds until collision detonation allowed

//     // Audio/visual timing constants - single source of truth
//     public const int PrimedBeepCount = 3;
//     public const float PrimedBeepInterval = 0.25f;
//     public const int CollisionBeepCount = 5;
//     public const float CollisionBeepInterval = 0.1f;

//     // Collision detection parameters
//     private const float collisionMassThreshold = 50f;
//     private const float collisionVelocityThreshold = 1f;

//     // State tracking
//     private float primingStartedAt = 0f;
//     private float armedAt = 0f; // Time when mine became armed (for primed light sequence)
//     private float collisionDetectedAt = 0f; // Time when collision was detected
//     private DepthChargeState currentState = DepthChargeState.Inactive;
//     private bool wasPickedUp = false; // Flag to detect if mine was picked up and re-dropped
//     private bool hasBeenInitialized = false; // Flag to ensure state initialization happens only once

//     private PrefabIdentifier? prefabIdentifier;
//     private DepthChargeVisuals? vfx;
//     private DepthChargeAudio? audio;

//     public float ExplosionRadius => explosionRadius;
//     public int PrimingCountdown => primingCountdown;
//     public float PrimingStartedAt => primingStartedAt;
//     public float ArmedAt => armedAt;
//     public float CollisionDetectedAt => collisionDetectedAt;
//     public DepthChargeState CurrentState => currentState;

//     // Legacy properties for backward compatibility
//     public bool IsPriming => currentState == DepthChargeState.Priming;
//     public bool IsPrimed => currentState == DepthChargeState.Armed || currentState == DepthChargeState.CollisionDetonating;
//     public bool IsDetonated => currentState == DepthChargeState.Detonated;

//     private void Awake()
//     {
//         prefabIdentifier = GetComponent<PrefabIdentifier>();
//         vfx = gameObject.GetComponent<DepthChargeVisuals>();
//         audio = gameObject.GetComponent<DepthChargeAudio>();

//         // Start in Inactive state - will be activated when placed in the world
//         currentState = DepthChargeState.Inactive;
//     }

//     private void OnEnable()
//     {
//         // When the mine is enabled (placed in world or loaded from save), check its state
//         // Only initialize if it hasn't been initialized yet or if it was picked up and re-placed
//         if (currentState == DepthChargeState.Inactive && (!hasBeenInitialized || wasPickedUp))
//         {
//             // Don't initialize while in inventory - check if Rigidbody is kinematic (inventory items are kinematic)
//             var rb = GetComponent<Rigidbody>();
//             if (rb != null && rb.isKinematic)
//             {
//                 Main.logger.LogInfo("Depth Charge in inventory - skipping initialization.");
//                 return;
//             }

//             // Check if this is a loaded object (from a save) or newly instantiated
//             bool isLoadedFromSave = Time.timeSinceLevelLoad < 2f;

//             if (isLoadedFromSave && !wasPickedUp)
//             {
//                 // This is an existing object loaded from a save - start it already armed
//                 currentState = DepthChargeState.Armed;
//                 primingStartedAt = Time.time - primingCountdown; // Set so that it's already primed
//                 armedAt = Time.time - 1f; // Set so primed sequence is already over
//                 Main.logger.LogInfo("Depth Charge loaded from save - starting armed.");
//             }
//             else
//             {
//                 // This is a newly instantiated object (dropped or spawned) or re-dropped after pickup - begin auto-priming
//                 currentState = DepthChargeState.Priming;
//                 primingStartedAt = Time.time;
//                 wasPickedUp = false; // Reset the flag

//                 // Reinitialize audio system if this is a re-placed mine
//                 audio?.Initialize();

//                 Main.logger.LogInfo("Depth Charge newly placed - auto-priming started.");
//             }

//             hasBeenInitialized = true;
//         }
//     }

//     private void Start()
//     {
//         audio?.Initialize();

//         // Play priming sound for newly placed mines (not loaded ones)
//         bool isLoadedFromSave = Time.timeSinceLevelLoad < 2f;

//         if (isLoadedFromSave)
//         {
//             // This is a loaded mine, don't play sound
//         }
//         else
//         {
//             // This is a newly placed/spawned mine, play the priming sound
//             audio?.PlayArmingSound();
//         }
//     }

//     private void FixedUpdate()
//     {
//         // Handle state transitions
//         if (currentState == DepthChargeState.Priming && Time.time >= primingStartedAt + primingCountdown)
//         {
//             currentState = DepthChargeState.Armed;
//             armedAt = Time.time;
//         }

//         // Apply self-righting torque to keep button on top
//         if (currentState != DepthChargeState.Detonated && currentState != DepthChargeState.Inactive)
//         {
//             var rb = GetComponent<Rigidbody>();
//             if (rb != null && !rb.isKinematic)
//             {
//                 // Ensure rotation is not frozen
//                 rb.constraints = RigidbodyConstraints.None;

//                 // Calculate how much we need to rotate to point up
//                 Vector3 currentUp = transform.up;
//                 Vector3 targetUp = Vector3.up;

//                 // Calculate the torque needed to rotate toward upright
//                 Vector3 torqueAxis = Vector3.Cross(currentUp, targetUp);
//                 float angleFromUpright = Vector3.Angle(currentUp, targetUp);

//                 // Apply stabilizing torque with damping
//                 if (angleFromUpright > 2f) // Only apply if tilted more than 2 degrees
//                 {
//                     // Reduce torque as we get closer to upright (prevents oscillation)
//                     float normalizedAngle = Mathf.Clamp01(angleFromUpright / 180f);
//                     float torqueStrength = normalizedAngle * normalizedAngle * 50f; // Quadratic falloff

//                     // Dampen if angular velocity is high (prevents overshooting)
//                     float angularSpeed = rb.angularVelocity.magnitude;
//                     if (angularSpeed > 0.5f)
//                     {
//                         torqueStrength *= 0.5f; // Reduce torque when spinning
//                     }

//                     rb.AddTorque(torqueAxis.normalized * torqueStrength, ForceMode.Force);
//                 }
//             }
//         }
//     }

//     private void OnCollisionEnter(Collision collision)
//     {
//         // Detonate on heavy impacts, while allowing slow velocity
//         if (IsPrimed && collision.rigidbody != null && collision.rigidbody.mass > collisionMassThreshold && collision.relativeVelocity.magnitude > collisionVelocityThreshold)
//         {
//             // Transition to collision detonating state
//             currentState = DepthChargeState.CollisionDetonating;
//             collisionDetectedAt = Time.time;

//             audio?.PlayCollisionSound();
//             Detonate(0.5f);
//         }
//     }

//     public void Detonate(float delay = 0f, bool force = false)
//     {
//         if (currentState == DepthChargeState.Priming && !force)
//         {
//             Main.logger.LogWarning("Attempted to detonate Depth Charge that is not primed!");
//             return;
//         }

//         // Only allow detonation if primeDelay has elapsed since priming
//         if (currentState == DepthChargeState.Priming && Time.time < primingStartedAt + primingCountdown && !force)
//         {
//             Main.logger.LogWarning("Attempted to detonate Depth Charge too soon after priming!");
//             return;
//         }

//         if (currentState == DepthChargeState.Detonated)
//         {
//             Main.logger.LogWarning("Attempted to detonate Depth Charge that is already detonated!");
//             return;
//         }

//         // Don't set to Detonated here - let the state stay as CollisionDetonating during the delay
//         // State will be set to Detonated in HideAndExplode()
//         StartCoroutine(HideAndExplodeAfterDelay(delay));
//     }

//     public void OnPickedUp()
//     {
//         hasBeenInitialized = false; // Allow re-initialization when placed again
//                                     // Called when the mine is picked up by the player
//         currentState = DepthChargeState.Inactive;
//         wasPickedUp = true;
//         Main.logger.LogInfo("Depth Charge picked up - state set to inactive.");
//     }

//     public void Disarm()
//     {
//         // Disarming is no longer possible - once primed, mines cannot be disarmed
//     }

//     private IEnumerator HideAndExplodeAfterDelay(float delay)
//     {
//         yield return new WaitForSeconds(delay);
//         currentState = DepthChargeState.Detonated;
//         HideAndExplode();
//     }

//     private void HideAndExplode()
//     {
//         // Hide all mesh renderers (including child objects)
//         var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
//         foreach (var meshRenderer in meshRenderers)
//         {
//             meshRenderer.enabled = false;
//         }

//         // Also disable SkinnedMeshRenderers if any
//         var skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
//         foreach (var skinnedRenderer in skinnedMeshRenderers)
//         {
//             skinnedRenderer.enabled = false;
//         }

//         vfx?.PlayExplosionEffect();
//         audio?.PlayExplosionSound();
//         DamageSystem.RadiusDamage(explosionDamage * Main.Options.explosionDamageMultiplier, gameObject.transform.position, explosionRadius, DamageType.Explosive, gameObject);
//         Destroy(gameObject, 5.0f);  // Delay to let damage system / VFX / audio finish
//     }
// }