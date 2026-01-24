using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using System.Reflection;

namespace VELD.AlterraWeaponry.Items;

public class PrawnLaserArm
{
    public const float maxCharge = 20f;
    public const float energyCost = 3f;
    public const float cooldown = 0f; // No cooldown - continuous firing

    public static string ClassID = "PrawnLaserArm";
    public static TechType TechType { get; private set; } = 0;

    public static GameObject prefab;
    public PrefabInfo Info { get; private set; }

    public PrawnLaserArm()
    {
        if (!Main.AssetsCache.TryGetAsset("PrawnLaserArm", out Sprite icon))
            Main.logger.LogError("Unable to load PrawnLaserArm sprite from cache.");

        Info = PrefabInfo
            .WithTechType(classId: ClassID, displayName: null, description: null, unlockAtStart: false, techTypeOwner: Assembly.GetExecutingAssembly())
            .WithSizeInInventory(new(1, 1))
            .WithIcon(icon);

        TechType = this.Info.TechType;
    }

    public void Patch()
    {
        RecipeData recipe = new()
        {
            craftAmount = 1,
            Ingredients =
            [
                new(TechType.ComputerChip, 2),
                new(TechType.Magnetite, 3),
                new(TechType.Nickel, 2),
                new(TechType.Diamond, 1)
            ]
        };

        CustomPrefab customPrefab = new(Info);

        CloneTemplate clone = new(Info, TechType.ExosuitDrillArmModule);

        customPrefab.SetGameObject(clone);

        var scanningGadget = customPrefab.SetUnlock(BlackPowder.TechType);
        scanningGadget.WithPdaGroupCategoryAfter(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades, TechType.GasTorpedo);
        scanningGadget.WithCompoundTechsForUnlock([Coal.TechType]);

#if BZ
        // Sets this only on BZ if it can find it.
        if (!Main.AssetsCache.TryGetAsset("UpgradePopup", out Sprite popupSprite))
        {
            Main.logger.LogError("Unable to load UpgradePopup sprite from cache.");
        }
        else
        {
            scanningGadget.WithEncyclopediaEntry("Tech/Weaponry", popupSprite);
            scanningGadget.WithAnalysisTech(popupSprite, PDAHandler.UnlockImportant);
        }
#endif

        // customPrefab.SetVehicleUpgradeModule(EquipmentType.ExosuitArm)
        //     .WithEnergyCost(energyCost)
        //     .WithMaxCharge(maxCharge)
        //     .WithCooldown(cooldown)
        //     .WithOnModuleAdded((Vehicle instance, int slotID) =>
        //     {
        //         if (!instance.gameObject.TryGetComponent(out LaserArmBehaviour laserMono))
        //             instance.gameObject.EnsureComponent<LaserArmBehaviour>();
        //         else
        //             Main.logger.LogWarning("Laser arm behavior already exists on PRAWN.");
        //     })
        //     .WithOnModuleRemoved((Vehicle instance, int slotID) =>
        //     {
        //         if (instance.gameObject.TryGetComponent(out LaserArmBehaviour laserMono))
        //             UnityEngine.Object.Destroy(laserMono);
        //         else
        //             Main.logger.LogWarning("Laser arm behavior not found on PRAWN during removal.");
        //     })
        //     .WithOnModuleUsed((Vehicle instance, int slotID, float charge, float chargeScalar) =>
        //     {
        //         // Continuous firing handled in Update
        //         if (!instance.gameObject.TryGetComponent(out LaserArmBehaviour laserMono))
        //             laserMono = instance.gameObject.EnsureComponent<LaserArmBehaviour>();

        //         if (instance is Exosuit exosuit)
        //         {
        //             laserMono.StartFiring(exosuit, slotID);
        //             Main.logger.LogInfo("Laser arm firing started.");
        //         }
        //     });

        CraftDataHandler.SetEquipmentType(Info.TechType, EquipmentType.ExosuitArm);
        CraftDataHandler.SetQuickSlotType(Info.TechType, QuickSlotType.Selectable);

        CustomExosuitArmUtils.RegisterCustomExosuitArm(
            new CustomExosuitArmUtils.CustomArm(Info.TechType, GetPrawnSuitArmPrefab));

        // customPrefab.SetBackgroundType(CraftData.BackgroundType.ExosuitArm);

        customPrefab.SetRecipe(recipe)
            .WithCraftingTime(3f)
            .WithFabricatorType(CraftTree.Type.SeamothUpgrades)
            .WithStepsToFabricatorTab("ExosuitModules");

        customPrefab.AddGadget(new BackgroundTypeGadget(customPrefab, CraftData.BackgroundType.ExosuitArm));

        customPrefab.Register();
    }

    private static IEnumerator GetPrawnSuitArmPrefab(IOut<GameObject> result)
    {
        // Load prawn suit
        var exosuitTask = CraftData.GetPrefabForTechTypeAsync(TechType.Exosuit);
        yield return exosuitTask;
        var exosuit = exosuitTask.GetResult();

        // Create prefab
        var exosuitComponent = exosuit.GetComponent<Exosuit>();
        var getArmPrefabMethod = typeof(Exosuit).GetMethod("GetArmPrefab", BindingFlags.Instance | BindingFlags.NonPublic);
        var armPrefab = (GameObject)getArmPrefabMethod.Invoke(exosuitComponent, [TechType.ExosuitDrillArmModule]);
        var prefab = UWE.Utils.InstantiateDeactivated(armPrefab);
        prefab.name = ClassID;

        // Prevent the prefab from being destroyed
        UnityEngine.Object.DontDestroyOnLoad(prefab);
        prefab.AddComponent<SceneCleanerPreserve>();

        result.Set(prefab);
    }
}
