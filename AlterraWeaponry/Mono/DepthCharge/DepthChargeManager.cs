namespace VELD.AlterraWeaponry.Mono.DepthCharge;

public class DepthChargeManager : MonoBehaviour
{
    // Static dictionary to persist state across entity unload/reload
    private static readonly Dictionary<string, DepthChargeState> stateCache = [];

    public DepthChargeState CurrentState { get; private set; } = DepthChargeState.Inactive;
    public float LastChangeTime { get; private set; } = 0f;

    public bool IsStateInactive => CurrentState == DepthChargeState.Inactive;
    public bool IsStatePriming => CurrentState == DepthChargeState.Priming;
    public bool IsStateArmed => CurrentState == DepthChargeState.Armed;
    public bool IsStateCollision => CurrentState == DepthChargeState.Collision;
    public bool IsStateDetonated => CurrentState == DepthChargeState.Detonated;

    private PrefabIdentifier? prefabIdentifier;
    private Pickupable? pickupable;
    private DepthChargeAudioVisual? av;

    private Coroutine? armedIndicatorCycleCoroutine;



    /* Unity Methods */

    private void Awake()
    {
        prefabIdentifier = GetComponent<PrefabIdentifier>();
        pickupable = GetComponent<Pickupable>();
        av = GetComponent<DepthChargeAudioVisual>();

        // Restore state from cache if it exists
        if (prefabIdentifier != null && stateCache.TryGetValue(prefabIdentifier.Id, out var cachedState))
        {
            CurrentState = cachedState;
        }
    }

    private void Start()
    {
        var fabricator = GetComponentInParent<Fabricator>();
        if (fabricator != null)
        {
            return;
        }

        // Auto-prime if spawned in the world (not in inventory)
        if (pickupable != null && IsStateInactive)
        {
            // Use reflection to check if the item is in inventory
            var inventoryItemField = typeof(Pickupable).GetField("inventoryItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (inventoryItemField != null)
            {
                var inventoryItem = inventoryItemField.GetValue(pickupable);
                if (inventoryItem == null)
                {
                    Arm(true);
                }
            }
        }
    }

    private void Update()
    {
        if (!IsStateArmed && armedIndicatorCycleCoroutine != null)
        {
            StopCoroutine(armedIndicatorCycleCoroutine);
            armedIndicatorCycleCoroutine = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check for collision conditions
        if (IsStateArmed && collision.rigidbody != null && collision.rigidbody.mass > DepthChargeConstants.collisionMassThreshold && collision.relativeVelocity.magnitude > DepthChargeConstants.collisionVelocityThreshold)
        {
            Collide();
        }
    }

    private void OnDestroy()
    {
        // Clean up the cached state when the depth charge is destroyed
        if (prefabIdentifier != null && CurrentState == DepthChargeState.Detonated)
        {
            stateCache.Remove(prefabIdentifier.Id);
        }
    }



    /* Public Methods */

    public void Deactivate()
    {
        if (IsStateInactive) return;


        if (HasDisallowedState([DepthChargeState.Collision, DepthChargeState.Detonated]))
            return;

        ChangeState(DepthChargeState.Inactive);

    }

    public void Prime()
    {
        IEnumerator ArmAfterwards()
        {
            float totalPrimingTime = DepthChargeConstants.primingIndicatorCount * DepthChargeConstants.primingIndicatorInterval;
            yield return new WaitForSeconds(totalPrimingTime);
            Arm();
        }

        if (HasDisallowedState([DepthChargeState.Priming, DepthChargeState.Armed, DepthChargeState.Collision, DepthChargeState.Detonated]))
            return;

        if (CurrentState != DepthChargeState.Inactive)
        {
            Main.logger.LogWarning("DepthChargeManager Prime called, but Depth Charge is not in Inactive state.");
            return;
        }

        ChangeState(DepthChargeState.Priming);

        av?.PlayPriming();
        StartCoroutine(ArmAfterwards());
    }

    public void Arm(bool force = false)
    {
        if (!force)
        {
            if (HasDisallowedState([DepthChargeState.Inactive, DepthChargeState.Armed, DepthChargeState.Collision, DepthChargeState.Detonated]))
                return;

            if (CurrentState != DepthChargeState.Priming)
            {
                Main.logger.LogWarning("DepthChargeManager Arm called, but Depth Charge is not in Priming state.");
                return;
            }
        }

        ChangeState(DepthChargeState.Armed);

        av?.PlayArmedInitial();
        armedIndicatorCycleCoroutine = StartCoroutine(PlayArmedCycleLoop());
    }

    public void Collide()
    {
        IEnumerator DetonateAfterwards()
        {
            float totalCollisionTime = DepthChargeConstants.collisionIndicatorCount * DepthChargeConstants.collisionIndicatorInterval;
            yield return new WaitForSeconds(totalCollisionTime);
            Detonate();
        }

        if (HasDisallowedState([DepthChargeState.Inactive, DepthChargeState.Priming, DepthChargeState.Detonated]))
            return;

        if (CurrentState != DepthChargeState.Armed)
        {
            Main.logger.LogWarning("DepthChargeManager Collide called, but Depth Charge is not in Armed state.");
            return;
        }

        ChangeState(DepthChargeState.Collision);

        av?.PlayCollision();
        StartCoroutine(DetonateAfterwards());
    }

    public void Detonate(float? overrideDelay = null)
    {
        if (HasDisallowedState([DepthChargeState.Inactive, DepthChargeState.Detonated]))
            return;

        // Disable pickupable component
        if (pickupable != null) pickupable.enabled = false;

        float delay = overrideDelay ?? DepthChargeConstants.collisionIndicatorDuration;

        ChangeState(DepthChargeState.Detonated);

        StartCoroutine(HideAndExplodeThenDestroy(delay));
    }



    /* Private Methods */

    private void ChangeState(DepthChargeState newState)
    {
        CurrentState = newState;
        LastChangeTime = Time.time;

        // Cache the state by ID for persistence across unload/reload
        if (prefabIdentifier != null) stateCache[prefabIdentifier.Id] = newState;
    }

    private bool HasDisallowedState(DepthChargeState[] disallowedStates)
    {
        if (disallowedStates.Contains(CurrentState))
        {
            Main.logger.LogWarning($"DepthChargeManager StateCheck failed - Current state {CurrentState} is in disallowed states: {string.Join(", ", disallowedStates)}");
            return true;
        }

        return false;
    }

    private IEnumerator PlayArmedCycleLoop()
    {
        if (av == null)
        {
            Main.logger.LogWarning("DepthChargeManager - DepthChargeAudioVisual (av) is null! Cannot play armed cycle.");
            yield break;
        }
        while (true)
        {
            yield return StartCoroutine(av.PlayArmedCycle());
            yield return new WaitForSeconds(DepthChargeConstants.armedIndicatorCycleInterval);
        }
    }

    private IEnumerator HideAndExplodeThenDestroy(float explosionDelay)
    {
        ChangeState(DepthChargeState.Detonated);

        if (explosionDelay > 0f) yield return new WaitForSeconds(explosionDelay);
        else yield return null;

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

        av?.PlayDetonation();
        DamageSystem.RadiusDamage(DepthChargeConstants.explosionDamage * Main.Options.explosionDamageMultiplier, gameObject.transform.position, DepthChargeConstants.explosionRadius, DamageType.Explosive, gameObject);
        Destroy(gameObject, 5.0f);  // Delay to let damage system / VFX / audio finish
    }
}