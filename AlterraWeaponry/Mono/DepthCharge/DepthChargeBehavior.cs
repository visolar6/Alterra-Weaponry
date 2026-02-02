namespace VELD.AlterraWeaponry.Mono.DepthCharge
{
    public class DepthChargeBehavior : MonoBehaviour
    {
        private const float explosionRadius = 25f;
        private const float explosionDamage = 2000f;
        private const int primingCountdown = 3; // Seconds until collision detonation allowed
        private const float collisionMassThreshold = 50f;
        private const float collisionVelocityThreshold = 1f;
        private float primingStartedAt = 0f;
        private bool isPriming = false;
        private bool isDetonated = false;

        private PrefabIdentifier? prefabIdentifier;
        private DepthChargeVFX? vfx;
        private DepthChargeAudio? audio;

        public float ExplosionRadius => explosionRadius;
        public int PrimingCountdown => primingCountdown;
        public bool IsPriming => isPriming;
        public bool IsPrimed => isPriming && Time.time >= primingStartedAt + primingCountdown;
        public bool IsDetonated => isDetonated;

        private void Awake()
        {
            prefabIdentifier = GetComponent<PrefabIdentifier>();
            // if (prefabIdentifier != null)
            // {
            //     DepthChargeStateManager.TryGetValue(prefabIdentifier, out bool savedPrimedState);
            //     if (savedPrimedState)
            //     {
            //         isPriming = true;
            //         primingStartedAt = Time.time - primingCountdown * 2; // Set so that it's already primed (with some buffer)
            //     }
            //     else
            //     {
            //         Main.logger.LogInfo($"Depth Charge {prefabIdentifier.Id} loaded with unprimed state.");
            //     }
            // }
            // else
            // {
            //     Main.logger.LogWarning("Depth Charge missing PrefabIdentifier component; cannot load saved state.");
            // }

            vfx = gameObject.GetComponent<DepthChargeVFX>();
            audio = gameObject.GetComponent<DepthChargeAudio>();
        }

        private void Start()
        {
            audio?.Initialize();
        }

        private void FixedUpdate()
        {
            // Apply self-righting torque to keep button on top
            if (!isDetonated)
            {
                var rb = GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    // Ensure rotation is not frozen
                    rb.constraints = RigidbodyConstraints.None;

                    // Calculate how much we need to rotate to point up
                    Vector3 currentUp = transform.up;
                    Vector3 targetUp = Vector3.up;

                    // Calculate the torque needed to rotate toward upright
                    Vector3 torqueAxis = Vector3.Cross(currentUp, targetUp);
                    float angleFromUpright = Vector3.Angle(currentUp, targetUp);

                    // Apply stabilizing torque with damping
                    if (angleFromUpright > 2f) // Only apply if tilted more than 2 degrees
                    {
                        // Reduce torque as we get closer to upright (prevents oscillation)
                        float normalizedAngle = Mathf.Clamp01(angleFromUpright / 180f);
                        float torqueStrength = normalizedAngle * normalizedAngle * 50f; // Quadratic falloff

                        // Dampen if angular velocity is high (prevents overshooting)
                        float angularSpeed = rb.angularVelocity.magnitude;
                        if (angularSpeed > 0.5f)
                        {
                            torqueStrength *= 0.5f; // Reduce torque when spinning
                        }

                        rb.AddTorque(torqueAxis.normalized * torqueStrength, ForceMode.Force);
                    }
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Detonate on heavy impacts, while allowing slow velocity
            if (IsPrimed && collision.rigidbody != null && collision.rigidbody.mass > collisionMassThreshold && collision.relativeVelocity.magnitude > collisionVelocityThreshold)
            {
                audio?.PlayCollisionSound();
                Detonate(0.5f);
            }
        }

        public void Prime()
        {
            if (!isPriming)
            {
                isPriming = true;
                // if (prefabIdentifier != null) DepthChargeStateManager.UpdateValue(prefabIdentifier, true);
                primingStartedAt = Time.time;
                audio?.PlayPrimingSound();

                // Disable pickup so player can't pick up armed charge
                var pickupable = gameObject.GetComponent<Pickupable>();
                if (pickupable != null)
                {
                    pickupable.isPickupable = false;
                }
            }
        }

        public void Detonate(float delay = 0f, bool force = false)
        {
            if (!isPriming && !force)
            {
                Main.logger.LogWarning("Attempted to detonate Depth Charge that is not primed!");
                return;
            }

            // Only allow detonation if primeDelay has elapsed since priming
            if (isPriming && Time.time < primingStartedAt + primingCountdown && !force)
            {
                Main.logger.LogWarning("Attempted to detonate Depth Charge too soon after priming!");
                return;
            }

            if (isDetonated)
            {
                Main.logger.LogWarning("Attempted to detonate Depth Charge that is already detonated!");
                return;
            }

            isDetonated = true;
            StartCoroutine(HideAndExplodeAfterDelay(delay));
        }

        public void Disarm()
        {
            if (!IsPrimed || isDetonated)
            {
                Main.logger.LogWarning("Attempted to disarm Depth Charge that is not primed or already detonated!");
                return;
            }

            isPriming = false;
            primingStartedAt = 0f;
            // Re-enable pickup
            var pickupable = gameObject.GetComponent<Pickupable>();
            if (pickupable != null)
            {
                pickupable.isPickupable = true;
            }

            audio?.PlayDisarmSound();
            // if (prefabIdentifier != null) DepthChargeStateManager.UpdateValue(prefabIdentifier, false);
        }

        private IEnumerator HideAndExplodeAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideAndExplode();
        }

        private void HideAndExplode()
        {
            // Hide all mesh renderers (including child objects)
            var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.enabled = false;
            }

            // Also disable SkinnedMeshRenderers if any
            var skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var skinnedRenderer in skinnedMeshRenderers)
            {
                skinnedRenderer.enabled = false;
            }

            vfx?.PlayExplosionEffect();
            audio?.PlayExplosionSound();
            DamageSystem.RadiusDamage(explosionDamage * Main.Options.explosionDamageMultiplier, gameObject.transform.position, explosionRadius, DamageType.Explosive, gameObject);
            Destroy(gameObject, 5.0f);  // Delay to let damage system / VFX / audio finish
        }
    }
}