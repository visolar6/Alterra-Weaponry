using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD;
using Nautilus.Extensions;
using Nautilus.FMod;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace VELD.AlterraWeaponry.Utilities;

/// <summary>
/// Utility class for handling explosion audio effects.
/// </summary>
public static class ExplosionAudio
{
    private static readonly string beepSoundId = "Beep";
    private static readonly string underwaterExplosionSoundId = "UnderwaterExplosion";

    public static void RegisterEvents(FModSoundBuilder builder)
    {
        builder.CreateNewEvent(beepSoundId, AudioUtils.BusPaths.SFX)
            .SetMode3D(1, 15)
            .SetSound("beep")
            .Register();

        builder.CreateNewEvent(underwaterExplosionSoundId, AudioUtils.BusPaths.SFX)
            .SetMode3D(25, 300)
            .SetSound("underwater-explosion")
            .Register();
    }

    public static void PlayBeep(Vector3 position)
    {
        Utils.PlayFMODAsset(AudioUtils.GetFmodAsset(beepSoundId), position);
    }

    public static void PlayUnderwaterExplosion(Vector3 position)
    {
        Utils.PlayFMODAsset(AudioUtils.GetFmodAsset(underwaterExplosionSoundId), position);
    }
}