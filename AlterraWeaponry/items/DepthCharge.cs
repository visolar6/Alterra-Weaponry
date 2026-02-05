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

        customPrefab.SetGameObject(() => DepthChargeBuilder.CreateGameObject(TechType));
        customPrefab.SetUnlock(BlackPowder.TechType);
        customPrefab.SetPdaGroupCategoryBefore(TechGroup.Personal, TechCategory.Equipment, TechType.Seaglide);
        customPrefab.SetRecipe(recipe)
            .WithCraftingTime(2.5f)
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab(CraftTreeHandler.Paths.FabricatorMachines);

        Main.logger.LogDebug("Registering DepthCharge prefab.");
        customPrefab.Register();

        Main.logger.LogDebug("Loaded and registered DepthCharge prefab.");
    }
}
