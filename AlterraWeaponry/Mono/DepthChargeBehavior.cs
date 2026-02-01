namespace VELD.AlterraWeaponry.Mono
{
    public class DepthChargeBehavior : MonoBehaviour
    {
        private const int explosionCountdown = 5;
        private const float explosionRadius = 20f;
        private const float explosionDamage = 200f;

        private bool isPrimed = false;
        private float primedAt = 0f;
        private bool isDetonated = false;
        private DepthChargeVFX vfx;

        public bool IsPrimed => isPrimed;
        public bool IsDetonated => isDetonated;

        private void Awake()
        {
            vfx = gameObject.GetComponent<DepthChargeVFX>();
            if (vfx == null)
            {
                vfx = gameObject.AddComponent<DepthChargeVFX>();
            }

            // HandTarget is already added during prefab setup
        }

        private void Start()
        {

        }

        private void Update()
        {
            if (isPrimed && !isDetonated && Time.time - primedAt >= explosionCountdown)
            {
                Detonate();
            }
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

        private void OnDelete()
        {
        }

        public void Prime()
        {
            isPrimed = true;
            primedAt = Time.time;

            // Disable pickup so player can't pick up armed charge
            var pickupable = gameObject.GetComponent<Pickupable>();
            if (pickupable != null)
            {
                pickupable.isPickupable = false;
            }

            ErrorMessage.AddMessage("Depth charge primed");
        }

        public void Detonate(float delay = 0f)
        {
            if (!isDetonated)
            {
                isDetonated = true;
                Main.logger.LogInfo("Detonating Depth Charge!" + (delay > 0f ? $" (delayed {delay}s)" : ""));

                if (delay > 0f)
                {
                    StartCoroutine(DetonateAfterDelay(delay));
                    return;
                }

                HideAndExplode();
            }
            else
            {
                Main.logger.LogWarning("Attempted to detonate Depth Charge that is already detonated!");
            }
        }

        private IEnumerator DetonateAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideAndExplode();
        }

        private void HideAndExplode()
        {
            // Hide all mesh renderers (including child objects)
            var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
            Main.logger.LogInfo($"Found {meshRenderers.Length} MeshRenderers to disable");
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.enabled = false;
                Main.logger.LogInfo($"Disabled MeshRenderer on {meshRenderer.gameObject.name}");
            }

            // Also disable SkinnedMeshRenderers if any
            var skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Main.logger.LogInfo($"Found {skinnedMeshRenderers.Length} SkinnedMeshRenderers to disable");
            foreach (var skinnedRenderer in skinnedMeshRenderers)
            {
                skinnedRenderer.enabled = false;
                Main.logger.LogInfo($"Disabled SkinnedMeshRenderer on {skinnedRenderer.gameObject.name}");
            }

            vfx?.PlayExplosionEffect();
            DamageSystem.RadiusDamage(explosionDamage * Main.Options.explosionDamageMultiplier, gameObject.transform.position, explosionRadius, DamageType.Explosive, gameObject);

            Destroy(gameObject, 5.0f);  // Delay to let damage system finish
        }
    }
}