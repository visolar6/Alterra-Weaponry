namespace VELD.AlterraWeaponry.Mono.DepthCharge;

public class DepthChargeAudioVisual : MonoBehaviour
{
    /* Private Fields */

    // Indicator light and material for visual effects
    private Light? indicatorLight;
    private MeshRenderer? meshRenderer;
    private Material? material;



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



    /* Private Methods */

    private void Initialize()
    {
        // If already initialized, skip
        if (indicatorLight != null && meshRenderer != null && meshRenderer.material != null)
            return;

        indicatorLight = GetComponentInChildren<Light>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        if (indicatorLight != null && meshRenderer != null && meshRenderer.material != null)
        {
            material = meshRenderer.material;
            indicatorLight.intensity = 0f; // Start off
            material.SetColor("_EmissionColor", Color.black);
        }
    }

    private IEnumerator PlayPrimingSequence()
    {
        Initialize();

        float nextEventTime = Time.time - DepthChargeConstants.primingIndicatorInterval;
        for (int i = 0; i < DepthChargeConstants.primingIndicatorCount; i++)
        {
            nextEventTime += DepthChargeConstants.primingIndicatorInterval;
            yield return StartCoroutine(TimingUtility.WaitUntilTime(nextEventTime, this));
            ExplosionAudio.PlayBeep(transform.position);
            StartCoroutine(FlashIndicatorLight(DepthChargeConstants.primingIndicatorIntensity, DepthChargeConstants.primingIndicatorEmission, DepthChargeConstants.primingIndicatorDuration));
        }
    }

    private IEnumerator PlayArmedInitialSequence()
    {
        Initialize();

        float nextEventTime = Time.time - DepthChargeConstants.armedIndicatorInitialInterval;
        for (int i = 0; i < DepthChargeConstants.armedIndicatorInitialCount; i++)
        {
            nextEventTime += DepthChargeConstants.armedIndicatorInitialInterval;
            yield return StartCoroutine(TimingUtility.WaitUntilTime(nextEventTime, this));
            ExplosionAudio.PlayBeep(transform.position);
            Main.logger.LogInfo($"[Armed Initial Beep {i + 1}] Played beep at position: {transform.position}, (player position: {Player.main?.gameObject.transform.position})");
            StartCoroutine(FlashIndicatorLight(DepthChargeConstants.armedIndicatorInitialIntensity, DepthChargeConstants.armedIndicatorInitialEmission, DepthChargeConstants.armedIndicatorInitialDuration));
        }
    }

    private IEnumerator PlayArmedCycleSequence()
    {
        Initialize();

        float nextEventTime = Time.time;
        yield return StartCoroutine(TimingUtility.WaitUntilTime(nextEventTime, this));
        ExplosionAudio.PlayBeep(transform.position);
        StartCoroutine(FlashIndicatorLight(DepthChargeConstants.armedIndicatorCycleIntensity, DepthChargeConstants.armedIndicatorCycleEmission, DepthChargeConstants.armedIndicatorCycleDuration));
    }

    private IEnumerator PlayCollisionSequence()
    {
        Initialize();

        float nextEventTime = Time.time - DepthChargeConstants.collisionIndicatorInterval;
        for (int i = 0; i < DepthChargeConstants.collisionIndicatorCount; i++)
        {
            nextEventTime += DepthChargeConstants.collisionIndicatorInterval;
            yield return StartCoroutine(TimingUtility.WaitUntilTime(nextEventTime, this));
            ExplosionAudio.PlayBeep(transform.position);
            StartCoroutine(FlashIndicatorLight(DepthChargeConstants.collisionIndicatorIntensity, DepthChargeConstants.collisionIndicatorEmission, DepthChargeConstants.collisionIndicatorDuration));
        }
    }

    private IEnumerator PlayDetonationSequence()
    {
        Initialize();

        ExplosionAudio.PlayUnderwaterExplosion(transform.position);
        ExplosionVFX.SpawnMultiPulseExplosion(DepthChargeConstants.explosionRadius / 2, transform.position, this);
        yield return null;
    }

    private IEnumerator FlashIndicatorLight(float intensity, Color emission, float duration)
    {
        Initialize();

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