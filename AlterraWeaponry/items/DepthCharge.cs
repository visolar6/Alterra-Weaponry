using VELD.AlterraWeaponry.Builders;

namespace VELD.AlterraWeaponry.Items;

internal class DepthCharge
{
    public static string ClassID = "DepthCharge";
    public static TechType TechType { get; private set; } = 0;


    public PrefabInfo Info { get; private set; }

    public DepthCharge()
    {
        if (!Main.AssetsCache.TryGetAsset("DepthCharge", out Sprite icon))
            Main.logger.LogError("Unable to load DepthCharge sprite from cache.");

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
        customPrefab.SetPdaGroupCategoryBefore(TechGroup.Personal, TechCategory.Equipment, TechType.Seaglide);
        // Do not call SetEquipment for non-equippable items
        customPrefab.SetRecipe(recipe)
            .WithCraftingTime(2.5f)
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Equipment");

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
        var mat = new Material(Shader.Find("MarmosetUBER"))
        {
            color = new Color(0.3f, 0.3f, 0.3f) // Dark gray
        };
        mat.SetFloat("_Metallic", 0.8f); // High metallic
        mat.SetFloat("_Glossiness", 0.7f); // Smooth/reflective
        renderer.material = mat;

        // Add required Subnautica components
        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 25f; // Increased mass for heavier object
        rb.drag = 1f;
        rb.angularDrag = 1f;
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        go.EnsureComponent<PrefabIdentifier>();
        go.EnsureComponent<TechTag>().type = TechType;

        var pickupable = go.EnsureComponent<Pickupable>();
        pickupable.isPickupable = true;
        pickupable.randomizeRotationWhenDropped = true;

        var worldForces = go.EnsureComponent<WorldForces>();
        worldForces.underwaterGravity = 0.1f;
        worldForces.underwaterDrag = 1f;

        go.EnsureComponent<EntityTag>().slotType = EntitySlot.Type.Small;

        // Add a sphere collider for pickup interaction (reduced to not overlap button)
        var sphereCollider = go.AddComponent<SphereCollider>();
        sphereCollider.radius = DepthChargeMesh.SphereRadius * 0.9f; // Slightly smaller than sphere radius to avoid button overlap

        // Add fabrication VFX
        go.EnsureComponent<VFXFabricating>();

        // Add depth charge behavior
        go.EnsureComponent<DepthChargeBehavior>();

        // Add live mixin
        var liveMixin = go.EnsureComponent<LiveMixin>();
        liveMixin.data = ScriptableObject.CreateInstance<LiveMixinData>();
        liveMixin.data.maxHealth = 10f;
        liveMixin.data.invincibleInCreative = false;

        // Add damage handling
        go.EnsureComponent<DepthChargeDamager>().liveMixin = liveMixin;

        // Create primer button as child object
        var primerButton = new GameObject("PrimerButton");
        primerButton.transform.SetParent(go.transform, false);

        // Add button mesh
        var buttonMeshFilter = primerButton.AddComponent<MeshFilter>();
        buttonMeshFilter.mesh = DepthChargeMesh.GetButton();

        var buttonRenderer = primerButton.AddComponent<MeshRenderer>();
        var buttonMat = new Material(Shader.Find("MarmosetUBER"));
        buttonMat.color = new Color(0.8f, 0.2f, 0.1f); // Red/orange
        buttonMat.SetFloat("_Metallic", 0.7f); // Metallic button
        buttonMat.SetFloat("_Glossiness", 0.6f); // Slightly less smooth
        buttonRenderer.material = buttonMat;

        // Position on top and scale down
        primerButton.transform.localPosition = new Vector3(0f, DepthChargeMesh.SphereRadius, 0f); // Top of sphere (radius 0.4)
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
