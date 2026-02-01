namespace VELD.AlterraWeaponry.Mono
{
    public class DepthChargeVFX : MonoBehaviour
    {
        private const float explosionRadius = 8f;

        public void PlayExplosionEffect()
        {
            StartCoroutine(SpawnCustomExplosion(transform.position));
        }

        private IEnumerator SpawnCustomExplosion(Vector3 position)
        {
            Main.logger.LogInfo("[DepthCharge] Creating custom explosion VFX...");

            // 1. Create the main explosion flash/burst
            var flashObj = new GameObject("DepthChargeExplosionFlash");
            flashObj.transform.position = position;

            // Add a bright light for the flash
            var light = flashObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.7f, 0.3f); // Orange-yellow
            light.intensity = 10f;
            light.range = explosionRadius * 3f;
            light.shadows = LightShadows.None;

            // Animate the light
            StartCoroutine(AnimateExplosionLight(light, flashObj));

            // 2. Use Crashfish explosion particles (proven to work)
            var crashfishTask = CraftData.GetPrefabForTechTypeAsync(TechType.Crash);
            yield return crashfishTask;

            var crashfishPrefab = crashfishTask.GetResult();
            if (crashfishPrefab != null)
            {
                var crash = crashfishPrefab.GetComponent<Crash>();
                if (crash != null && crash.detonateParticlePrefab != null)
                {
                    // Instantiate the explosion particle and scale it
                    float scale = explosionRadius / 4f; // Slightly smaller scale
                    var explosion = Instantiate(crash.detonateParticlePrefab, position, Quaternion.identity);
                    explosion.transform.localScale = Vector3.one * scale;

                    var particleSystems = explosion.GetComponentsInChildren<ParticleSystem>();
                    foreach (var ps in particleSystems)
                    {
                        var main = ps.main;
                        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                        main.startSizeMultiplier *= scale * 1.5f; // Reduced from 2f
                        main.startSpeedMultiplier *= scale;
                        ps.Play();
                    }

                    Main.logger.LogInfo($"[DepthCharge] Crashfish explosion spawned with scale {scale:F2}x");
                }
            }

            // 3. Create visible shockwave effect
            StartCoroutine(CreateShockwave(position));

            Main.logger.LogInfo("[DepthCharge] Custom explosion VFX complete");
        }

        private IEnumerator AnimateExplosionLight(Light light, GameObject lightObj)
        {
            // Flash bright then fade
            float duration = 3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Quick flash then exponential decay
                if (t < 0.1f)
                {
                    light.intensity = Mathf.Lerp(10f, 15f, t / 0.1f);
                }
                else
                {
                    light.intensity = 15f * Mathf.Exp(-(t - 0.1f) * 5f);
                }

                light.range = explosionRadius * 3f * (1f - t * 0.5f);

                yield return null;
            }

            UnityEngine.Object.Destroy(lightObj);
        }

        private IEnumerator CreateShockwave(Vector3 position)
        {
            // Create expanding bubble shockwave with more visible bubbles
            int bubbleCount = 50;
            float speed = explosionRadius * 2f;

            for (int i = 0; i < bubbleCount; i++)
            {
                Vector3 direction = UnityEngine.Random.onUnitSphere;
                Vector3 bubblePos = position + direction * 0.5f;

                // Spawn bubble VFX
                var bubbleObj = new GameObject($"ShockwaveBubble_{i}");
                bubbleObj.transform.position = bubblePos;

                // Create larger, more visible sphere
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.parent = bubbleObj.transform;
                sphere.transform.localScale = Vector3.one * 0.8f; // Bigger bubbles

                var renderer = sphere.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Make bubbles more opaque and tinted
                    var mat = new Material(Shader.Find("Standard"));
                    mat.color = new Color(1f, 0.9f, 0.7f, 0.8f); // Orange-white, more opaque
                    mat.SetFloat("_Glossiness", 0.8f);
                    mat.SetFloat("_Metallic", 0.2f);
                    renderer.material = mat;
                }

                // Remove collider to avoid physics interactions
                var collider = sphere.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.Destroy(collider);
                }

                // Animate bubble
                StartCoroutine(AnimateBubble(bubbleObj, direction, speed));
            }

            yield return null;
        }

        private IEnumerator AnimateBubble(GameObject bubble, Vector3 direction, float speed)
        {
            float lifetime = 0.8f; // Longer lifetime
            float elapsed = 0f;
            Vector3 startPos = bubble.transform.position;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;

                bubble.transform.position = startPos + direction * speed * t;

                // Scale down more gradually
                float scaleT = Mathf.Pow(1f - t, 2f); // Quadratic falloff
                bubble.transform.localScale = Vector3.one * 0.8f * scaleT;

                yield return null;
            }

            UnityEngine.Object.Destroy(bubble);
        }
    }
}
