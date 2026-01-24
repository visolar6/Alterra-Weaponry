// Based on TheRedPlague (https://github.com/ThePlagueSpreads/TheRedPlague-PublicMirror)
// Original source: TheRedPlague/Utilities/Gadgets/BackgroundTypeGadget.cs

using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;

namespace VELD.AlterraWeaponry.Utils;

public class BackgroundTypeGadget : Gadget
{
    private readonly CraftData.BackgroundType _backgroundType;

    public BackgroundTypeGadget(ICustomPrefab prefab, CraftData.BackgroundType backgroundType) : base(prefab)
    {
        _backgroundType = backgroundType;
    }

    protected override void Build()
    {
        CraftDataHandler.SetBackgroundType(prefab.Info.TechType, _backgroundType);
    }
}