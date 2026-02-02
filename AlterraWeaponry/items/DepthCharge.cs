using VELD.AlterraWeaponry.Builders;
using VELD.AlterraWeaponry.Mono.DepthCharge;

namespace VELD.AlterraWeaponry.Items;

/// <summary>
/// The depth charge is a throwable explosive device that detonates underwater after being primed and dropped into the water, and coming into contact with
/// heavy objects. Launching a depth charge from a vehicle using a depth charge system automatically primes it for detonation.<br/><br/>
/// Additionally, it can be detonated manually by taking explosive or fire damage.
/// </summary>
internal class DepthCharge
{
    public static string ClassID = "DepthCharge";
    public static TechType TechType { get; private set; } = 0;


    public PrefabInfo Info { get; private set; }

    public DepthCharge()
    {
        Sprite? icon;
        try
        {
            icon = ResourceHandler.LoadSpriteFromFile(Path.Combine("Assets", "Sprite", "depth-charge.png"));
        }
        catch (Exception)
        {
            Main.logger.LogError("Failed to load depth charge icon sprite.");
            icon = null;
        }

        Info = PrefabInfo
            .WithTechType(classId: ClassID, displayName: null, description: null, unlockAtStart: false, techTypeOwner: Assembly.GetExecutingAssembly())
            .WithIcon(icon)
            .WithSizeInInventory(new(2, 2));
        TechType = Info.TechType;
    }

    public void Patch()
    {
        RecipeData recipe = new()
        {
            craftAmount = 1,
            Ingredients =
            [
                new(TechType.Titanium, 2),
                new(BlackPowder.TechType, 2),
                new(TechType.Copper, 1),
            ]
        };

        CustomPrefab customPrefab = new(Info);

        customPrefab.SetGameObject(CreatePrefab);
        customPrefab.SetUnlock(BlackPowder.TechType);
        customPrefab.SetPdaGroupCategoryBefore(TechGroup.Personal, TechCategory.Equipment, TechType.Seaglide);
        customPrefab.SetRecipe(recipe)
            .WithCraftingTime(2.5f)
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab(CraftTreeHandler.Paths.FabricatorMachines);

        customPrefab.Register();

        Main.logger.LogDebug("Loaded and registered DepthCharge prefab.");
    }

    /// <summary>
    /// Creates the depth charge prefab from a simple sphere
    /// </summary>
    /// <returns>The depth charge game object</returns>
    public static GameObject CreatePrefab()
    {
        // Create main body with combined mesh
        var go = new GameObject("DepthCharge");

        // Add mesh components with cached combined mesh
        var meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = DepthChargeMesh.GetBody();

        var renderer = go.AddComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Standard"))
        {
            color = new Color(0.8f, 0.8f, 0.8f) // Light gray
        };
        mat.SetFloat("_Metallic", 0.8f); // High metallic
        mat.SetFloat("_Glossiness", 0.7f); // Smooth/reflective
        var metalTexture = ResourceHandler.LoadTexture2DFromFile(Path.Combine("Assets", "Texture2D", "metal.png"));
        if (metalTexture != null)
        {
            mat.mainTexture = metalTexture;
            Main.logger.LogInfo("metal.png texture loaded and applied to depth charge material.");
        }
        else
        {
            Main.logger.LogWarning("metal.png texture NOT found or failed to load.");
        }
        renderer.material = mat;

        // Add rigidbody for physics
        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 10f;
        rb.drag = 0.7f; // Lower drag for smoother underwater motion
        rb.angularDrag = 0.6f; // Lower angular drag for more natural spin
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooths visual movement

        // Add LargeWorldEntity and other required components to make it a valid world object
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        go.EnsureComponent<PrefabIdentifier>();
        go.EnsureComponent<TechTag>().type = TechType;

        // Allow pickup
        var pickupable = go.EnsureComponent<Pickupable>();
        pickupable.isPickupable = true;
        pickupable.randomizeRotationWhenDropped = false;

        // Add world forces for underwater behavior
        var worldForces = go.EnsureComponent<WorldForces>();
        worldForces.underwaterGravity = 0f;
        worldForces.underwaterDrag = 1f;

        // Add entity tag for inventory/equipment handling
        go.EnsureComponent<EntityTag>().slotType = EntitySlot.Type.Small;

        // Add a sphere collider for pickup interaction (reduced to not overlap button)
        var sphereCollider = go.AddComponent<SphereCollider>();
        sphereCollider.radius = DepthChargeMesh.SphereRadius;
        sphereCollider.isTrigger = false; // Solid collider for physics interactions

        // Add fabrication VFX
        go.EnsureComponent<VFXFabricating>();

        // Add behavior
        go.EnsureComponent<DepthChargeBehavior>();

        // Add VFX
        go.EnsureComponent<DepthChargeVFX>();

        // Add audio
        go.EnsureComponent<DepthChargeAudio>();

        // Add live mixin
        var liveMixin = go.EnsureComponent<LiveMixin>();
        liveMixin.data = ScriptableObject.CreateInstance<LiveMixinData>();
        liveMixin.data.maxHealth = 10f;
        liveMixin.data.invincibleInCreative = false;
        liveMixin.data.destroyOnDeath = false;

        // Add damage handling
        go.EnsureComponent<DepthChargeDamager>().liveMixin = liveMixin;

        // Create primer button as child object
        var primerButton = new GameObject("PrimerButton");
        primerButton.transform.SetParent(go.transform, false);

        // Add button mesh
        var buttonMeshFilter = primerButton.AddComponent<MeshFilter>();
        buttonMeshFilter.mesh = DepthChargeMesh.GetButton();

        // Add button renderer with distinct material
        var buttonRenderer = primerButton.AddComponent<MeshRenderer>();
        var buttonMat = new Material(Shader.Find("Standard"))
        {
            color = new Color(0.8f, 0.2f, 0.1f) // Red/orange
        };
        buttonMat.SetFloat("_Metallic", 1f); // Metallic button
        buttonMat.SetFloat("_Glossiness", 1f); // Slightly less smooth
        buttonRenderer.material = buttonMat;
        buttonRenderer.material.mainTexture = metalTexture;

        // Position on top and scale down
        primerButton.transform.localPosition = new Vector3(0f, DepthChargeMesh.SphereRadius * 0.95f, 0f); // Top of sphere (radius 0.4)
        primerButton.transform.localScale = new Vector3(DepthChargeMesh.SpikeLength, DepthChargeMesh.SpikeBaseRadius, DepthChargeMesh.SpikeLength); // Smaller and flatter

        // Add box collider for easier interaction
        var buttonCollider = primerButton.AddComponent<BoxCollider>();
        buttonCollider.size = new Vector3(1f, 1f, 1f);

        // Add HandTarget for priming to the button
        primerButton.EnsureComponent<DepthChargeHandTarget>();

        // Apply shaders
        MaterialUtils.ApplySNShaders(go);

        go.tag = "Untagged";
        go.layer = 0;

        return go;
    }
}
