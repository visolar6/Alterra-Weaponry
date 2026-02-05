namespace VELD.AlterraWeaponry.Items.DepthCharge;

using VELD.AlterraWeaponry.Utilities;

public class DepthChargeAudioVisual : MonoBehaviour
{
    /* Private Fields */

    // Audio clips and sources for audio effects
    private AudioClip? beepClip;
    private AudioClip? underwaterExplosionClip;
    private AudioSource? primingAudioSource;
    private AudioSource? primedAudioSource;
    private AudioSource? collisionAudioSource;
    private AudioSource? explosionAudioSource;
    private AudioSource? disarmingAudioSource;

    // Indicator light and material for visual effects
    private Light? indicatorLight;
    private MeshRenderer? meshRenderer;
    private Material? material;



    /* Unity Methods */

    /// <summary>
    /// Initializes the audio-visual components of the depth charge.
    /// </summary>
    private void Start()
    {
        // Load the primed AudioClip
        string beepClipPath = Path.Combine("Assets", "Audio", "beep.wav");
        beepClip = ResourceHandler.LoadAudioClipFromFile(beepClipPath);
        if (beepClip == null)
            Main.logger.LogError($"Failed to load primed AudioClip from {beepClipPath}");

        string underwaterExplosionClipPath = Path.Combine("Assets", "Audio", "underwater-explosion.wav");
        underwaterExplosionClip = ResourceHandler.LoadAudioClipFromFile(underwaterExplosionClipPath);
        if (underwaterExplosionClip == null)
            Main.logger.LogError($"Failed to load explosion AudioClip from {underwaterExplosionClipPath}");

        // Setup AudioSource for priming sound
        primingAudioSource = gameObject.AddComponent<AudioSource>();
        primingAudioSource.playOnAwake = false;
        primingAudioSource.loop = false;
        primingAudioSource.volume = 0.2f;
        primingAudioSource.pitch = 0.8f;
        primingAudioSource.spatialize = true;
        primingAudioSource.spatialBlend = 1f; // 3D sound
        primingAudioSource.minDistance = 1f;
        primingAudioSource.maxDistance = 15f;
        primingAudioSource.rolloffMode = AudioRolloffMode.Linear;

        // Setup AudioSource for primed sound
        primedAudioSource = gameObject.AddComponent<AudioSource>();
        primedAudioSource.playOnAwake = false;
        primedAudioSource.loop = false;
        primedAudioSource.volume = 0.2f;
        primedAudioSource.pitch = 1.0f;
        primedAudioSource.spatialize = true;
        primedAudioSource.spatialBlend = 1f;
        primedAudioSource.minDistance = 1f;
        primedAudioSource.maxDistance = 15f;
        primedAudioSource.rolloffMode = AudioRolloffMode.Linear;

        // Setup AudioSource for collision sound
        collisionAudioSource = gameObject.AddComponent<AudioSource>();
        collisionAudioSource.playOnAwake = false;
        collisionAudioSource.loop = false;
        collisionAudioSource.volume = 0.4f;
        collisionAudioSource.pitch = 1.2f;
        collisionAudioSource.spatialize = true;
        collisionAudioSource.spatialBlend = 1f;
        collisionAudioSource.minDistance = 1f;
        collisionAudioSource.maxDistance = 15f;
        collisionAudioSource.rolloffMode = AudioRolloffMode.Linear;

        // Setup explosion AudioSource and load clip
        explosionAudioSource = gameObject.AddComponent<AudioSource>();
        explosionAudioSource.playOnAwake = false;
        explosionAudioSource.loop = false;
        explosionAudioSource.volume = 1f;
        explosionAudioSource.pitch = 1.0f;
        explosionAudioSource.spatialize = true;
        explosionAudioSource.spatialBlend = 1f;
        explosionAudioSource.minDistance = 1f;
        explosionAudioSource.maxDistance = 100f;
        explosionAudioSource.rolloffMode = AudioRolloffMode.Linear;

        // Setup disarming AudioSource
        disarmingAudioSource = gameObject.AddComponent<AudioSource>();
        disarmingAudioSource.playOnAwake = false;
        disarmingAudioSource.loop = false;
        disarmingAudioSource.volume = 0.2f;
        disarmingAudioSource.pitch = 0.7f;
        disarmingAudioSource.spatialize = true;
        disarmingAudioSource.spatialBlend = 1f;
        disarmingAudioSource.minDistance = 1f;
        disarmingAudioSource.maxDistance = 15f;
        disarmingAudioSource.rolloffMode = AudioRolloffMode.Linear;

        indicatorLight = GetComponentInChildren<Light>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        if (indicatorLight != null && meshRenderer != null && meshRenderer.material != null)
        {
            material = meshRenderer.material;
            indicatorLight.intensity = 0f; // Start off
            material.SetColor("_EmissionColor", Color.black);
        }
    }



    /* Public Methods */

    /// <summary>
    /// Plays the priming sequence (audio and visual effects)
    /// Beeps a set amount of times with indicator light flashes
    /// </summary>
    public void PlayPriming()
    {
        StartCoroutine(PlayPrimingSequence());
    }

    /// <summary>
    /// Plays the armed sequence (audio and visual effects)
    /// Beeps semi-rapidly with indicator light flashes
    /// </summary>
    public void PlayArmedInitial()
    {
        StartCoroutine(PlayArmedInitialSequence());
    }

    /// <summary>
    /// Plays the armed cycle sequence (audio and visual effects)
    /// Beeps once with indicator light flash
    /// </summary>
    public IEnumerator PlayArmedCycle()
    {
        return PlayArmedCycleSequence();
    }

    /// <summary>
    /// Plays the collision sequence (audio and visual effects)
    /// Beeps rapidly with indicator light flashes
    /// </summary>
    public void PlayCollision()
    {
        StartCoroutine(PlayCollisionSequence());
    }

    /// <summary>
    /// Plays the detonation sequence (audio and visual effects)
    /// Explodes with explosion sound and VFX
    /// </summary>
    public void PlayDetonation()
    {
        StartCoroutine(PlayDetonationSequence());
    }



    /* Internal Methods */

    private IEnumerator PlayPrimingSequence()
    {
        float nextEventTime = Time.time - DepthChargeConstants.primingIndicatorInterval;
        for (int i = 0; i < DepthChargeConstants.primingIndicatorCount; i++)
        {
            nextEventTime += DepthChargeConstants.primingIndicatorInterval;
            yield return StartCoroutine(TimingUtility.WaitUntilTime(nextEventTime, this));
            primingAudioSource!.PlayOneShot(beepClip);
            StartCoroutine(FlashIndicatorLight(DepthChargeConstants.primingIndicatorIntensity, DepthChargeConstants.primingIndicatorEmission, DepthChargeConstants.primingIndicatorDuration));
        }
    }

    private IEnumerator PlayArmedInitialSequence()
    {
        float nextEventTime = Time.time - DepthChargeConstants.armedIndicatorInitialInterval;
        for (int i = 0; i < DepthChargeConstants.armedIndicatorInitialCount; i++)
        {
            nextEventTime += DepthChargeConstants.armedIndicatorInitialInterval;
            yield return StartCoroutine(TimingUtility.WaitUntilTime(nextEventTime, this));
            primedAudioSource!.PlayOneShot(beepClip);
            StartCoroutine(FlashIndicatorLight(DepthChargeConstants.armedIndicatorInitialIntensity, DepthChargeConstants.armedIndicatorInitialEmission, DepthChargeConstants.armedIndicatorInitialDuration));
        }
    }

    private IEnumerator PlayArmedCycleSequence()
    {
        float nextEventTime = Time.time;
        yield return StartCoroutine(TimingUtility.WaitUntilTime(nextEventTime, this));
        primedAudioSource!.PlayOneShot(beepClip);
        StartCoroutine(FlashIndicatorLight(DepthChargeConstants.armedIndicatorCycleIntensity, DepthChargeConstants.armedIndicatorCycleEmission, DepthChargeConstants.armedIndicatorCycleDuration));
    }

    private IEnumerator PlayCollisionSequence()
    {
        float nextEventTime = Time.time - DepthChargeConstants.collisionIndicatorInterval;
        for (int i = 0; i < DepthChargeConstants.collisionIndicatorCount; i++)
        {
            nextEventTime += DepthChargeConstants.collisionIndicatorInterval;
            yield return StartCoroutine(TimingUtility.WaitUntilTime(nextEventTime, this));
            collisionAudioSource!.PlayOneShot(beepClip);
            StartCoroutine(FlashIndicatorLight(DepthChargeConstants.collisionIndicatorIntensity, DepthChargeConstants.collisionIndicatorEmission, DepthChargeConstants.collisionIndicatorDuration));
        }
    }

    private IEnumerator PlayDetonationSequence()
    {
        explosionAudioSource!.PlayOneShot(underwaterExplosionClip);
        ExplosionVFX.SpawnMultiPulseExplosion(DepthChargeConstants.explosionRadius, transform.position, this);
        yield return null;
    }

    private IEnumerator FlashIndicatorLight(float intensity, Color emission, float duration)
    {
        float startTime = Time.time;
        SetIndicatorLight(intensity, emission);
        yield return StartCoroutine(TimingUtility.WaitUntilTime(startTime + duration, this));
        SetIndicatorLight(0f, Color.black);
    }

    private void SetIndicatorLight(float intensity, Color emission)
    {
        indicatorLight!.intensity = intensity;
        material!.SetColor("_EmissionColor", emission);
    }
}