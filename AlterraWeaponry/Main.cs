namespace VELD.AlterraWeaponry;

[BepInPlugin(modGUID, modName, modVers)]
public class Main : BaseUnityPlugin
{
    // MOD INFO
    internal const string modName = "Alterra Weaponry";
    internal const string modGUID = "com.VELD.AlterraWeaponry";
    internal const string modVers = "1.1.1";
    internal const string modLongVers = "1.1.1.0";

    // BepInEx/Harmony/Unity
    private static readonly Harmony harmony = new(modGUID);
    public static ManualLogSource logger;

    // STORY GOALS
#if BZ
    internal static StoryGoal AWPresentationGoal;
#endif

    public static ResourcesCacheManager AssetsCache { get; private set; }
    public static ResourcesCacheManager BannerAssetsCache { get; private set; }

    internal static Options Options { get; } = OptionsPanelHandler.RegisterModOptions<Options>();

    private void Awake()
    {
        logger = Logger;
        try
        {
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            AssetsCache = ResourcesCacheManager.LoadResources(Path.Combine(basePath, "sn.alterraweaponry.assets"));
            BannerAssetsCache = ResourcesCacheManager.LoadResources(Path.Combine(basePath, "sn.alterraweaponry_banners.assets"));

            var soundSource = new ModFolderSoundSource(Path.Combine(basePath, "Assets", "Audio"));
            var builder = new FModSoundBuilder(soundSource);
            ExplosionAudio.RegisterEvents(builder);
        }
        catch (Exception ex)
        {
            logger.LogFatal($"Fatal error occured: Unable to load resources to cache.\n{ex}");
        }
    }

    private void Start()
    {
        logger.LogInfo($"{modName} {modVers} started patching.");
        harmony.PatchAll();
        logger.LogInfo($"{modName} {modVers} harmony patched.");

#if SN1
        BreakableResourcePatcher.Patch(harmony);
        logger.LogInfo($"{modName} {modVers} breakable resource patched.");

        // ModDatabankHandler entry disabled - was causing duplicate PDA entries on each load
        // ModDatabankHandler.RegisterMod(new ModDatabankHandler.ModData()
        // {
        //     guid = modGUID,
        //     version = modVers,
        //     image = AssetsCache.GetAsset<Texture2D>("ModLogo"),
        //     name = "Alterra Weaponry",
        //     desc = "Ever wanted to bust some asses in Subnautica? You got the right mod!\n\nThis mod adds weapons and defensive modules to Subnautica."
        // });

        Coal coal = new();
        coal.Patch();

        BlackPowder blackPowder = new();
        blackPowder.Patch();

        ExplosiveTorpedo explosiveTorpedo = new();
        explosiveTorpedo.Patch();

        PrawnSelfDefenseModule prawnSelfDefenseModule = new();
        prawnSelfDefenseModule.Patch();

        DepthCharge depthCharge = new();
        depthCharge.Patch();

        // PrawnLaserArm prawnLaserArm = new();
        // prawnLaserArm.Patch();

        // PrawnLaserArmFragment prawnLaserArmFragment = new();
        // prawnLaserArmFragment.Patch();

        logger.LogInfo($"{modName} {modVers} items registered.");

        // Patch localization before registering PDA entries
        LanguagesHandler.GlobalPatch();

        // Initialize PDA entries and goals
        GlobalInitializer.PatchGoals();
        GlobalInitializer.PatchPDALogs();
        GlobalInitializer.PatchPDAEncyEntries();

        logger.LogInfo($"{modName} {modVers} initialization complete.");
#endif
    }
}
