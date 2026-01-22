// using Nautilus.Assets;
// using Nautilus.Assets.Gadgets;
// using Nautilus.Crafting;
// using Nautilus.Handlers;
// using Nautilus.Utility;
// using System.Reflection;

// namespace VELD.AlterraWeaponry.Items;

// public class PrawnLaserArm
// {
//     public const float maxCharge = 20f;
//     public const float energyCost = 3f;
//     public const float cooldown = 0f; // No cooldown - continuous firing

//     public static string ClassID = "PrawnLaserArm";
//     public static TechType TechType { get; private set; } = 0;

//     public static GameObject prefab;
//     public PrefabInfo Info { get; private set; }

//     public PrawnLaserArm()
//     {
//         if (!Main.AssetsCache.TryGetAsset("PrawnLaserArm", out Sprite icon))
//             Main.logger.LogError("Unable to load PrawnLaserArm sprite from cache.");

//         Info = PrefabInfo
//             .WithTechType(classId: ClassID, displayName: null, description: null, unlockAtStart: false techTypeOwner: Assembly.GetExecutingAssembly())
//             .WithSizeInInventory(new(1, 1))
//             .WithIcon(icon);

//         TechType = this.Info.TechType;
//     }

//     public void Patch()
//     {
//         RecipeData recipe = new()
//         {
//             craftAmount = 1,
//             Ingredients =
//             [
//                 new(TechType.ComputerChip, 2),
//                 new(TechType.Magnetite, 3),
//                 new(TechType.Nickel, 2),
//                 new(TechType.Diamond, 1)
//             ]
//         };

//         CustomPrefab customPrefab = new(Info);

//         CloneTemplate clone = new(Info, TechType.ExosuitDrillArmModule);

//         customPrefab.SetGameObject(clone);

//         // var scanningGadget = customPrefab.SetUnlock(BlackPowder.TechType);
//         // scanningGadget.WithPdaGroupCategoryAfter(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades, TechType.GasTorpedo);
//         // scanningGadget.WithCompoundTechsForUnlock([Coal.TechType]);
//         // if (!Main.AssetsCache.TryGetAsset("UpgradePopup", out Sprite popupSprite))
//         // {
//         //     Main.logger.LogError("Unable to load UpgradePopup sprite from cache.");
//         // }
//         // else
//         // {
//         //     scanningGadget.WithEncyclopediaEntry("Tech/Weaponry", popupSprite);
//         //     scanningGadget.WithAnalysisTech(popupSprite, PDAHandler.UnlockImportant);
//         // }

//         // customPrefab.SetVehicleUpgradeModule(EquipmentType.ExosuitArm)
//         //     .WithEnergyCost(energyCost)
//         //     .WithMaxCharge(maxCharge)
//         //     .WithCooldown(cooldown)
//         //     .WithOnModuleAdded((Vehicle instance, int slotID) =>
//         //     {
//         //         if (!instance.gameObject.TryGetComponent(out LaserArmBehaviour laserMono))
//         //             instance.gameObject.EnsureComponent<LaserArmBehaviour>();
//         //         else
//         //             Main.logger.LogWarning("Laser arm behavior already exists on PRAWN.");
//         //     })
//         //     .WithOnModuleRemoved((Vehicle instance, int slotID) =>
//         //     {
//         //         if (instance.gameObject.TryGetComponent(out LaserArmBehaviour laserMono))
//         //             UnityEngine.Object.Destroy(laserMono);
//         //         else
//         //             Main.logger.LogWarning("Laser arm behavior not found on PRAWN during removal.");
//         //     })
//         //     .WithOnModuleUsed((Vehicle instance, int slotID, float charge, float chargeScalar) =>
//         //     {
//         //         // Continuous firing handled in Update
//         //         if (!instance.gameObject.TryGetComponent(out LaserArmBehaviour laserMono))
//         //             laserMono = instance.gameObject.EnsureComponent<LaserArmBehaviour>();

//         //         if (instance is Exosuit exosuit)
//         //         {
//         //             laserMono.StartFiring(exosuit, slotID);
//         //             Main.logger.LogInfo("Laser arm firing started.");
//         //         }
//         //     });

//         CraftDataHandler.SetEquipmentType(Info.TechType, EquipmentType.ExosuitArm);
//         CraftDataHandler.SetQuickSlotType(Info.TechType, QuickSlotType.Selectable);

//         CustomExosuitArmUtils.RegisterCustomExosuitArm(
//             new CustomExosuitArmUtils.CustomArm(Info.TechType, GetPrawnSuitArmPrefab));

//         // customPrefab.SetBackgroundType(CraftData.BackgroundType.ExosuitArm);

//         customPrefab.SetRecipe(recipe)
//             .WithCraftingTime(3f)
//             .WithFabricatorType(CraftTree.Type.Workbench);

//         customPrefab.Register();
//     }

//     private static IEnumerator GetPrawnSuitArmPrefab(IOut<GameObject> result)
//     {
//         // Load prawn suit
//         var exosuitTask = CraftData.GetPrefabForTechTypeAsync(TechType.Exosuit);
//         yield return exosuitTask;
//         var exosuit = exosuitTask.GetResult();

//         // Create prefab
//         var exosuitComponent = exosuit.GetComponent<Exosuit>();
//         var getArmPrefabMethod = typeof(Exosuit).GetMethod("GetArmPrefab", BindingFlags.Instance | BindingFlags.NonPublic);
//         var armPrefab = (GameObject)getArmPrefabMethod.Invoke(exosuitComponent, new object[] { TechType.ExosuitDrillArmModule });
//         var prefab = UWE.Utils.InstantiateDeactivated(armPrefab);
//         prefab.name = ClassID;
//         // var oldDrillArmComponent = prefab.GetComponent<ExosuitDrillArm>();
//         // Object.DestroyImmediate(oldDrillArmComponent);
//         // var armRigParent = prefab.transform.Find("exosuit_01_armRight/ArmRig");
//         // armRigParent.Find("exosuit_hand_geo").gameObject.SetActive(false);
//         // var grapplingArm = armRigParent.Find("exosuit_grapplingHook_geo").gameObject;
//         // grapplingArm.SetActive(true);

//         // Fix arm being culled on the screen
//         // var armRenderer = grapplingArm.GetComponent<SkinnedMeshRenderer>();
//         // armRenderer.localBounds = new Bounds(armRenderer.localBounds.center, armRenderer.localBounds.size * 1.5f);

//         // Spawn blade model
//         // var bladeModel = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("PrawnBladeArmPrefab"),
//         //     armRigParent.Find("clavicle/shoulder/bicepPivot/elbow"));
//         // bladeModel.transform.localPosition = new Vector3(-0.630f, -0.050f, 0);
//         // bladeModel.transform.localEulerAngles = new Vector3(270, 90, 0);
//         // bladeModel.transform.localScale = Vector3.one * 0.9f;
//         // var eyeLook = bladeModel.transform.Find("PrawnSuitBladeArmModule/PrawnArmBladeArmature/Eye").gameObject
//         //     .AddComponent<GenericEyeLook>();
//         // eyeLook.degreesPerSecond = 300;
//         // eyeLook.useLimits = true;
//         // eyeLook.dotLimit = 0.4f;
//         // MaterialUtils.ApplySNShaders(bladeModel);

//         // Add core functionality
//         // var arm = prefab.AddComponent<ObsidianBladeArm>();
//         // arm.animator = prefab.GetComponentInChildren<Animator>();
//         // arm.front = bladeModel.transform.Find("Front");
//         // arm.fxControl = arm.GetComponent<VFXController>();

//         // Connect everything back together
//         // prefab.GetComponent<SkyApplier>().renderers = prefab.GetComponentsInChildren<Renderer>();

//         // Prevent the prefab from being destroyed
//         UnityEngine.Object.DontDestroyOnLoad(prefab);
//         prefab.AddComponent<SceneCleanerPreserve>();

//         result.Set(prefab);
//     }
// }
