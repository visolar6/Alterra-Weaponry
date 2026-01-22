namespace VELD.AlterraWeaponry.Utils;

internal class GlobalInitializer
{
    internal static void PatchGoals()
    {
#if BZ
        Main.AWPresentationGoal = new("PWAPresentation", Story.GoalType.PDA, 8f)
        { 
            playInCreative = true,
            playInCinematics = false,
            checkPlayerSafety = true,
            delay = 15f
        };

        Nautilus.Handlers.StoryGoalHandler.RegisterCustomEvent(Main.AWPresentationGoal.key, () =>
        {
            Main.logger.LogInfo("Played PWAPresentation goal.");
        });
#endif
    }

    // This function MUST be an IEnumerator because the SpriteManager is not initialized soon enough for getting the PDA logs icons at time.
    internal static void PatchPDALogs()
    {
        //yield return new WaitUntil(() => SpriteManager.hasInitialized);
#if BZ
        Main.logger.LogInfo($"{Main.modName} {Main.modVers} Registering PDA Logs...");

        // Presentation PDA log "Hello xenoworker 91802..."
        if(Main.AssetsCache.TryGetAsset("PWAPresentation", out AudioClip PWAPresentation))
        {
            Main.logger.LogInfo("PWAPresentation audio message is being registered.");
            if(Main.Options.allowDialogs)
            {
                Sound sound = AudioUtils.CreateSound(PWAPresentation, AudioUtils.StandardSoundModes_Stream);
                CustomSoundHandler.RegisterCustomSound("PWAPresAudio", sound, AudioUtils.BusPaths.PDAVoice);
                FMODAsset fmodAsset = AudioUtils.GetFmodAsset("PWAPresAudio");
                PDAHandler.AddLogEntry(
                    Main.AWPresentationGoal.key,
                    "Subtitles_AWPresentation",
                    sound: fmodAsset,
                    SpriteManagerAwaiter.Get(SpriteManager.Group.Log, "Pda").Result
                );
            }
            else
            {
                PDAHandler.AddLogEntry(
                    Main.AWPresentationGoal.key,
                    "Subtitles_AWPresentation",
                    sound: null,
                    SpriteManager.Get(SpriteManager.Group.Log, "Pda")
                );
            }
        }

        // First lethal weapon PDA log "A lethal weapon have been detected into your inventory..."
        if(Main.AssetsCache.TryGetAsset("FirstLethalMessage", out AudioClip AWFirstLethal))
        {
            Main.logger.LogInfo("AWFirstLethal audio message is being registered.");
            if(Main.Options.allowDialogs)
            {
                Sound sound = AudioUtils.CreateSound(AWFirstLethal, AudioUtils.StandardSoundModes_Stream);
                CustomSoundHandler.RegisterCustomSound("FirstLethalMessage", sound, AudioUtils.BusPaths.PDAVoice);
                FMODAsset fmodAsset = AudioUtils.GetFmodAsset("FirstLethalMessage");
                PDAHandler.AddLogEntry(
                    key: "AWFirstLethal",
                    languageKey: "Subtitles_AWFirstLethal",
                    sound: fmodAsset,
                    SpriteManager.Get(SpriteManager.Group.Log, "Pda")
                );
            }
            else
            {
                PDAHandler.AddLogEntry(
                    key: "AWFirstLethal",
                    languageKey: "Subtitles_AWFirstLethal",
                    sound: null,
                    SpriteManager.Get(SpriteManager.Group.Log, "Pda")
                );
            }
            Main.logger.LogInfo("AWFirstLethal registered successfully.");

        }

        Main.logger.LogInfo($"{Main.modName} {Main.modVers} Registered PDA logs!");
#endif
    }

    internal static void PatchPDAEncyEntries()
    {
        // Coal entry
        Sprite coalIcon = null;
        Texture2D coalBanner = null;
        Main.AssetsCache.TryGetAsset("Coal", out coalIcon);
        Main.BannerAssetsCache.TryGetAsset("Coal_banner", out coalBanner);
        PDAHandler.AddEncyclopediaEntry(
            "Coal",
            "Tech/Weaponry",
            Language.main.Get("Ency_Coal"),
            Language.main.Get("EncyDesc_Coal"),
            coalBanner,
            coalIcon,
            unlockSound: PDAHandler.UnlockBasic
        );

        // BlackPowder entry
        Sprite blackPowderIcon = null;
        Texture2D blackPowderBanner = null;
        Main.AssetsCache.TryGetAsset("BlackPowder", out blackPowderIcon);
        Main.BannerAssetsCache.TryGetAsset("BlackPowder_banner", out blackPowderBanner);
        PDAHandler.AddEncyclopediaEntry(
            "BlackPowder",
            "Tech/Weaponry",
            Language.main.Get("Ency_BlackPowder"),
            Language.main.Get("EncyDesc_BlackPowder"),
            blackPowderBanner,
            blackPowderIcon,
            unlockSound: PDAHandler.UnlockBasic
        );

        // ExplosiveTorpedo entry
        Sprite explosiveTorpedoIcon = null;
        Texture2D explosiveTorpedoBanner = null;
        Main.AssetsCache.TryGetAsset("ExplosiveTorpedo", out explosiveTorpedoIcon);
        Main.BannerAssetsCache.TryGetAsset("ExplosiveTorpedo_banner", out explosiveTorpedoBanner);
        PDAHandler.AddEncyclopediaEntry(
            "ExplosiveTorpedo",
            "Tech/Weaponry",
            Language.main.Get("Ency_ExplosiveTorpedo"),
            Language.main.Get("EncyDesc_ExplosiveTorpedo"),
            explosiveTorpedoBanner,
            explosiveTorpedoIcon,
            unlockSound: PDAHandler.UnlockImportant
        );

        // Prawn laser arm entry
        // Sprite prawnLaserArmIcon = null;
        // Main.AssetsCache.TryGetAsset("PrawnLaserArm", out prawnLaserArmIcon);
        // PDAHandler.AddEncyclopediaEntry(
        //     "PrawnLaserArm",
        //     "Tech/Weaponry",
        //     Language.main.Get("Ency_PrawnLaserArm"),
        //     Language.main.Get("EncyDesc_PrawnLaserArm"),
        //     null,
        //     prawnLaserArmIcon,
        //     unlockSound: PDAHandler.UnlockImportant
        // );

        // PDAHandler.AddEncyclopediaEntry(
        //     "WeaponsAuthorization",
        //     "Tech/Weaponry",
        //     null,
        //     null,
        //     unlockSound: PDAHandler.UnlockImportant
        // );

        // Register AWFirstLethal event handler for both games
        /*Nautilus.Handlers.StoryGoalHandler.RegisterCustomEvent("AWFirstLethal", () =>
        {
            Main.logger.LogInfo("Triggering AWFirstLethal - unlocking WeaponsAuthorization encyclopedia entry.");
            PDAEncyclopedia.Add("WeaponsAuthorization", true);
        });*/
    }
}
