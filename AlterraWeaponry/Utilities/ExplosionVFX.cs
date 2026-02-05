using UnityEngine;
using System.Collections;

namespace VELD.AlterraWeaponry.Utilities;

/// <summary>
/// Utility class for creating explosion visual effects.
/// Requires a MonoBehaviour context to run coroutines.
/// </summary>
public static class ExplosionVFX
{
    /// <summary>
    /// Spawns a multi-pulse explosion effect. Requires a MonoBehaviour to run coroutines.
    /// </summary>
    public static void SpawnMultiPulseExplosion(float explosionRadius, Vector3 position, MonoBehaviour coroutineRunner)
    {
        coroutineRunner.StartCoroutine(SpawnMultiPulseExplosionCoroutine(explosionRadius, position, coroutineRunner));
    }

    public static IEnumerator SpawnMultiPulseExplosionCoroutine(float explosionRadius, Vector3 position, MonoBehaviour coroutineRunner)
    {
        // Initial main blast (flash, light, big VFX)
        yield return coroutineRunner.StartCoroutine(SpawnCustomExplosionCoroutine(explosionRadius, position, coroutineRunner));

        // Parameters for pulses
        int pulseCount = 3;
        float pulseInterval = 0.35f;
        float pulseScale = 0.5f; // Start at half size
        float scaleDecay = 0.5f; // Each pulse is half the previous

        for (int i = 0; i < pulseCount; i++)
        {
            yield return new WaitForSeconds(pulseInterval);
            coroutineRunner.StartCoroutine(SpawnPulseExplosionCoroutine(position, pulseScale, coroutineRunner));
            pulseScale *= scaleDecay;
        }
    }

    /// <summary>
    /// Spawns a smaller crashfish explosion for a pressure pulse (no light).
    /// </summary>
    public static IEnumerator SpawnPulseExplosionCoroutine(Vector3 position, float scale, MonoBehaviour coroutineRunner)
    {
        // Use Crashfish explosion particles
        var crashfishTask = CraftData.GetPrefabForTechTypeAsync(TechType.Crash);
        yield return crashfishTask;

        var crashfishPrefab = crashfishTask.GetResult();
        if (crashfishPrefab != null)
        {
            var crash = crashfishPrefab.GetComponent<Crash>();
            if (crash != null && crash.detonateParticlePrefab != null)
            {
                var explosion = UnityEngine.Object.Instantiate(crash.detonateParticlePrefab, position, Quaternion.identity);
                explosion.transform.localScale = Vector3.one * scale;

                var particleSystems = explosion.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    var main = ps.main;
                    main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                    main.startSizeMultiplier *= scale * 1.5f;
                    main.startSpeedMultiplier *= scale;
                    ps.Play();
                }
            }
        }
    }

    public static IEnumerator SpawnCustomExplosionCoroutine(float explosionRadius, Vector3 position, MonoBehaviour coroutineRunner)
    {
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
        coroutineRunner.StartCoroutine(AnimateExplosionLightCoroutine(explosionRadius, light, flashObj));

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
                float scale = explosionRadius / 4f;
                var explosion = UnityEngine.Object.Instantiate(crash.detonateParticlePrefab, position, Quaternion.identity);
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
            }
        }

        // 3. Create visible shockwave effect
        coroutineRunner.StartCoroutine(CreateShockwaveCoroutine(explosionRadius, position, coroutineRunner));
    }

    public static IEnumerator AnimateExplosionLightCoroutine(float explosionRadius, Light light, GameObject lightObj)
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

    public static IEnumerator CreateShockwaveCoroutine(float explosionRadius, Vector3 position, MonoBehaviour coroutineRunner)
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
            coroutineRunner.StartCoroutine(AnimateBubbleCoroutine(bubbleObj, direction, speed));
        }

        yield return null;
    }

    public static IEnumerator AnimateBubbleCoroutine(GameObject bubble, Vector3 direction, float speed)
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