namespace VELD.AlterraWeaponry.Items;

public class PrawnLaserArmFragment
{
    public static string ClassID = "PrawnLaserArmFragment";
    public static TechType TechType { get; private set; } = 0;

    public static GameObject prefab;
    public PrefabInfo Info { get; private set; }

    public PrawnLaserArmFragment()
    {
        if (!Main.AssetsCache.TryGetAsset("PrawnLaserArm", out Sprite icon))
            Main.logger.LogError("Unable to load PrawnLaserArmFragment sprite from cache.");

        Info = PrefabInfo
            .WithTechType(classId: ClassID, displayName: null, description: null, techTypeOwner: Assembly.GetExecutingAssembly())
            .WithSizeInInventory(new(1, 1))
            .WithIcon(icon);

        TechType = this.Info.TechType;
    }

    public void Patch()
    {
        CustomPrefab customPrefab = new(this.Info);

        // Clone from a generic fragment (using Seamoth Torpedo Arm fragment as base)
        CloneTemplate clone = new(this.Info,
#if BZ
            TechType.SeaTruckUpgradeMiningArmFragment
#elif SN1
            TechType.ExosuitTorpedoArmFragment
#endif
        );

        customPrefab.SetGameObject(clone);

        // Set as a scannable fragment (not craftable)
        var scanningGadget = customPrefab.SetUnlock(PrawnLaserArm.TechType);

        // Don't add to PDA category - fragments are just collectibles
        // They unlock automatically after 3 scans via PDAScanner_Add_Patch

        customPrefab.Register();
    }
}
