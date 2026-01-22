namespace VELD.AlterraWeaponry.Utils;

[Menu("Alterra Weaponry")]
public class Options : Nautilus.Json.ConfigFile
{
#if BZ
    [Toggle(LabelLanguageId = "Options.AW_DialogsBool", TooltipLanguageId = "Options.AW_DialogsBool.Tooltip", Order = 0)]
    public bool allowDialogs = true;
#endif

    [Slider(Min = 0.05f, Max = 10.00f, Step = 0.05f, Format = "x{0:F2}",
        LabelLanguageId = "Options.AW_explosionDamageMultiplier", TooltipLanguageId = "Options.AW_explosionDamageMultiplier.Tooltip", Order = 1)]
    public float explosionDamageMultiplier = 1.0f;

    [Slider(Min = 50f, Max = 500f, Step = 10f, Format = "{0:F0}",
        LabelLanguageId = "Options.AW_explosionDamage", TooltipLanguageId = "Options.AW_explosionDamage.Tooltip", Order = 2)]
    public float explosionDamage = 250f;

    [Slider(Min = 1f, Max = 10f, Step = 0.5f, Format = "{0:F1}s",
        LabelLanguageId = "Options.AW_explosionDuration", TooltipLanguageId = "Options.AW_explosionDuration.Tooltip", Order = 3)]
    public float explosionDuration = 3.0f;

    [Slider(Min = 0.5f, Max = 5f, Step = 0.25f, Format = "{0:F2}x",
        LabelLanguageId = "Options.AW_explosionSpeed", TooltipLanguageId = "Options.AW_explosionSpeed.Tooltip", Order = 4)]
    public float explosionSpeed = 2.0f;

    [Slider(Min = 5f, Max = 30f, Step = 1f, Format = "{0:F0}m",
        LabelLanguageId = "Options.AW_explosionRadius", TooltipLanguageId = "Options.AW_explosionRadius.Tooltip", Order = 5)]
    public float explosionRadius = 10f;

    [ColorPicker(LabelLanguageId = "Options.AW_explosionColor", TooltipLanguageId = "Options.AW_explosionColor.Tooltip", Order = 6)]
    public UnityEngine.Color explosionColor = new UnityEngine.Color(1f, 0.2f, 0f, 1f); // Red-orange
}
