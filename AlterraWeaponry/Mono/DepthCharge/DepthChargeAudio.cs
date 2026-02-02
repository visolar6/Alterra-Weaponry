namespace VELD.AlterraWeaponry.Mono.DepthCharge;

/// <summary>
/// Manages audio playback for the Depth Charge.
/// </summary>
public class DepthChargeAudio : MonoBehaviour
{
    private AudioClip? beepClip;
    private AudioClip? underwaterExplosionClip;

    private AudioSource? primingAudioSource;
    private AudioSource? primedAudioSource;
    private AudioSource? collisionAudioSource;
    private AudioSource? explosionAudioSource;
    private AudioSource? disarmingAudioSource;

    private const int primedRepetitions = 3;
    private const float primedDelayBetweenRepetitions = 0.25f;
    private const int collisionRepetitions = 5;
    private const float collisionDelayBetweenRepetitions = 0.1f;
    private const int disarmingRepetitions = 4;
    private const float disarmingDelayBetweenRepetitions = 0.2f;


    private DepthChargeBehavior? _behavior;
    private int PrimingCountdown
    {
        get
        {
            if (_behavior == null)
                _behavior = gameObject.GetComponent<DepthChargeBehavior>();
            return _behavior != null ? _behavior.PrimingCountdown : 1;
        }
    }
    private bool IsDetonated
    {
        get
        {
            if (_behavior == null)
                _behavior = gameObject.GetComponent<DepthChargeBehavior>();
            return _behavior != null && _behavior.IsDetonated;
        }
    }

    public void Initialize()
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
    }

    /// <summary>
    /// Plays the priming sound (when the depth charge is being armed).
    /// </summary>
    public void PlayPrimingSound()
    {
        StartCoroutine(PlayPrimingSoundSequence());
    }

    private IEnumerator PlayPrimingSoundSequence()
    {
        if (primingAudioSource == null || beepClip == null)
        {
            Main.logger.LogWarning("Cannot play priming sound: AudioSource or AudioClip is null.");
            yield break;
        }

        var primingCount = 0;
        while (primingCount++ < PrimingCountdown)
        {
            if (IsDetonated)
            {
                Main.logger.LogWarning("Cannot play priming sound: Depth Charge is already detonated.");
                yield break;
            }

            primingAudioSource.PlayOneShot(beepClip);
            yield return new WaitForSeconds(1f);
        }

        StartCoroutine(PlayPrimedSoundSequence());
    }

    private IEnumerator PlayPrimedSoundSequence()
    {
        if (primedAudioSource == null || beepClip == null)
        {
            Main.logger.LogWarning("Cannot play primed sound: AudioSource or AudioClip is null.");
            yield break;
        }

        if (IsDetonated)
        {
            Main.logger.LogWarning("Cannot play primed sound: Depth Charge is already detonated.");
            yield break;
        }

        for (int i = 0; i < primedRepetitions; i++)
        {
            if (IsDetonated)
            {
                Main.logger.LogWarning($"Cannot play primed sound repeated ({i}): Depth Charge is already detonated.");
                yield break;
            }

            primedAudioSource.PlayOneShot(beepClip);
            yield return new WaitForSeconds(primedDelayBetweenRepetitions);
        }
    }

    /// <summary>
    /// Plays the collision sound.
    /// </summary>
    public void PlayCollisionSound()
    {
        StartCoroutine(PlayCollisionSoundSequence());
    }

    private IEnumerator PlayCollisionSoundSequence()
    {
        if (IsDetonated)
        {
            Main.logger.LogWarning("Cannot play collision sound: Depth Charge is already detonated.");
            yield break;
        }

        for (int i = 0; i < collisionRepetitions; i++)
        {
            collisionAudioSource?.PlayOneShot(beepClip);
            yield return new WaitForSeconds(collisionDelayBetweenRepetitions);
        }
    }

    /// <summary>
    /// Plays the explosion sound.
    /// </summary>
    public void PlayExplosionSound()
    {
        if (explosionAudioSource == null || underwaterExplosionClip == null)
        {
            Main.logger.LogWarning("Cannot play explosion sound: AudioSource or AudioClip is null.");
            return;
        }

        explosionAudioSource.PlayOneShot(underwaterExplosionClip);
    }

    /// <summary>
    /// Plays the disarm sound.
    /// </summary>
    public void PlayDisarmSound()
    {
        if (disarmingAudioSource == null || beepClip == null)
        {
            Main.logger.LogWarning("Cannot play disarm sound: AudioSource or AudioClip is null.");
            return;
        }

        StartCoroutine(PlayDisarmSoundSequence());
    }

    private IEnumerator PlayDisarmSoundSequence()
    {
        for (int i = 0; i < disarmingRepetitions; i++)
        {
            if (IsDetonated)
            {
                Main.logger.LogWarning("Cannot play disarm sound: Depth Charge is already detonated.");
                yield break;
            }

            disarmingAudioSource?.PlayOneShot(beepClip);
            yield return new WaitForSeconds(disarmingDelayBetweenRepetitions);
        }
    }
}
