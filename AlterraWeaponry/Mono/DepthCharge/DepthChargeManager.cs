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
            Main.logger.LogInfo($"DepthChargeManager - Restored state {cachedState} from cache for ID {prefabIdentifier.Id}");
            CurrentState = cachedState;
        }
    }

    private void Start()
    {
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
                    Main.logger.LogInfo("DepthChargeManager - Auto-priming spawned depth charge (not in inventory)");
                    ChangeState(DepthChargeState.Priming);
                    Invoke(nameof(Arm), 0.5f);
                }
            }
        }
    }

    private void Update()
    {
        if (!IsStateArmed && armedIndicatorCycleCoroutine != null)
        {
            Main.logger.LogInfo($"DepthChargeManager - Stopping armed indicator cycle coroutine (state changed to {CurrentState})");
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
            Main.logger.LogInfo($"DepthChargeManager - Removed cached state for ID {prefabIdentifier.Id}");
        }
    }



    /* Public Methods */

    public void Deactivate()
    {
        Main.logger.LogInfo("DepthChargeManager Deactivate called");
        if (IsStateInactive)
        {
            Main.logger.LogInfo("DepthChargeManager - Depth Charge is already inactive.");
            return;
        }


        if (HasDisallowedState([DepthChargeState.Collision, DepthChargeState.Detonated]))
            return;

        ChangeState(DepthChargeState.Inactive);

        Main.logger.LogInfo("DepthChargeManager - Depth Charge is now deactivated.");
    }

    public void Prime()
    {
        IEnumerator ArmAfterwards()
        {
            float totalPrimingTime = DepthChargeConstants.primingIndicatorCount * DepthChargeConstants.primingIndicatorInterval;
            yield return new WaitForSeconds(totalPrimingTime);
            Arm();
        }

        Main.logger.LogInfo("DepthChargeManager Prime called");
        if (HasDisallowedState([DepthChargeState.Priming, DepthChargeState.Armed, DepthChargeState.Collision, DepthChargeState.Detonated]))
            return;

        if (CurrentState != DepthChargeState.Inactive)
        {
            Main.logger.LogWarning("DepthChargeManager Prime called, but Depth Charge is not in Inactive state.");
            return;
        }

        Main.logger.LogInfo("DepthChargeManager - Transitioning to Priming state");
        ChangeState(DepthChargeState.Priming);

        av?.PlayPriming();
        StartCoroutine(ArmAfterwards());
    }

    public void Arm()
    {
        Main.logger.LogInfo("DepthChargeManager Arm called");
        if (HasDisallowedState([DepthChargeState.Inactive, DepthChargeState.Armed, DepthChargeState.Collision, DepthChargeState.Detonated]))
            return;

        if (CurrentState != DepthChargeState.Priming)
        {
            Main.logger.LogWarning("DepthChargeManager Arm called, but Depth Charge is not in Priming state.");
            return;
        }

        Main.logger.LogInfo("DepthChargeManager - Transitioning to Armed state");
        ChangeState(DepthChargeState.Armed);

        av?.PlayArmedInitial();
        Main.logger.LogInfo("DepthChargeManager - Starting armed indicator cycle coroutine");
        armedIndicatorCycleCoroutine = StartCoroutine(PlayArmedCycleLoop());
    }

    public void Collide()
    {
        IEnumerator DetonateAfterwards()
        {
            float totalCollisionTime = DepthChargeConstants.collisionIndicatorCount * DepthChargeConstants.collisionIndicatorInterval;
            yield return new WaitForSeconds(7f);
            Detonate();
        }

        Main.logger.LogInfo("DepthChargeManager Collide called");
        if (HasDisallowedState([DepthChargeState.Inactive, DepthChargeState.Priming, DepthChargeState.Detonated]))
            return;

        if (CurrentState != DepthChargeState.Armed)
        {
            Main.logger.LogWarning("DepthChargeManager Collide called, but Depth Charge is not in Armed state.");
            return;
        }

        Main.logger.LogInfo("DepthChargeManager - Transitioning to Collision state");
        ChangeState(DepthChargeState.Collision);

        av?.PlayCollision();
        StartCoroutine(DetonateAfterwards());
    }

    public void Detonate(float? overrideDelay = null)
    {
        float delay = overrideDelay ?? DepthChargeConstants.collisionIndicatorDuration;

        Main.logger.LogInfo("DepthChargeManager Detonate called");
        if (HasDisallowedState([DepthChargeState.Inactive, DepthChargeState.Detonated]))
            return;

        Main.logger.LogInfo("DepthChargeManager - Transitioning to Detonated state");
        ChangeState(DepthChargeState.Detonated);

        StartCoroutine(HideAndExplodeThenDestroy(delay));
    }



    /* Private Methods */

    private void ChangeState(DepthChargeState newState)
    {
        Main.logger.LogInfo($"DepthChargeManager Change - State change from {CurrentState} to {newState} at time {Time.time}");
        CurrentState = newState;
        LastChangeTime = Time.time;

        // Cache the state by ID for persistence across unload/reload
        if (prefabIdentifier != null)
        {
            stateCache[prefabIdentifier.Id] = newState;
            Main.logger.LogInfo($"DepthChargeManager - Cached state {newState} for ID {prefabIdentifier.Id}");
        }
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
        Main.logger.LogInfo("DepthChargeManager - PlayArmedCycleLoop started");
        while (true)
        {
            Main.logger.LogInfo("DepthChargeManager - Playing armed cycle");
            yield return StartCoroutine(av!.PlayArmedCycle());
            Main.logger.LogInfo($"DepthChargeManager - Armed cycle complete, waiting {DepthChargeConstants.armedIndicatorCycleInterval}s before next cycle");
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