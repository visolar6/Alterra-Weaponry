namespace VELD.AlterraWeaponry.Items;

internal class ExplosiveTorpedo
{
    public static string ClassID = "ExplosiveTorpedo";
    public static TechType TechType { get; private set; } = 0;

    public PrefabInfo Info { get; private set; }

    public ExplosiveTorpedo()
    {
        if (!Main.AssetsCache.TryGetAsset("ExplosiveTorpedo", out Sprite icon))
            Main.logger.LogError("Unable to load ExplosiveTorpedo Sprite from cache.");

        Info = PrefabInfo
            .WithTechType(classId: ClassID, displayName: null, description: null, unlockAtStart: false, techTypeOwner: Assembly.GetExecutingAssembly())
            .WithSizeInInventory(new(1, 1))
            .WithIcon(icon);

        TechType = Info.TechType;
    }

    public void Patch()
    {
        RecipeData recipe = new()
        {
            craftAmount = 2,
            Ingredients =
            [
                new(TechType.Titanium, 1),
                new(BlackPowder.TechType, 1)
            ]
        };

        CustomPrefab customPrefab = new(Info);
        CloneTemplate clone = new(Info, TechType.GasTorpedo);

        // Add marker component to the cloned template
        clone.ModifyPrefab += go =>
        {
            Main.logger.LogInfo($"[ExplosiveTorpedo] ModifyPrefab called for {go.name}");

            // Use PersistentMarker instead - more robust
            var marker = go.AddComponent<VELD.AlterraWeaponry.Mono.ExplosiveTorpedoPersistentMarker>();
            Main.logger.LogInfo($"[ExplosiveTorpedo] Added ExplosiveTorpedoPersistentMarker to {go.name}");

            // Also add the regular marker as backup
            go.AddComponent<VELD.AlterraWeaponry.Mono.ExplosiveTorpedoMarker>();
            Main.logger.LogInfo($"[ExplosiveTorpedo] Added ExplosiveTorpedoMarker to {go.name}");
        };

        customPrefab.SetGameObject(clone);
        var scanningGadget = customPrefab.SetUnlock(BlackPowder.TechType);
        scanningGadget.WithPdaGroupCategoryAfter(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades, TechType.GasTorpedo);
        scanningGadget.WithCompoundTechsForUnlock([Coal.TechType]);

#if BZ  // This sets the popup on BZ if it can find it.
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

        customPrefab.SetRecipe(recipe)
            .WithCraftingTime(4f)
            .WithFabricatorType(CraftTree.Type.SeamothUpgrades)
            .WithStepsToFabricatorTab("Torpedoes");

        customPrefab.Register();
    }
}