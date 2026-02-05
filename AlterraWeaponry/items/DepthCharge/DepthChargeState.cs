namespace VELD.AlterraWeaponry.Items.DepthCharge;

public enum DepthChargeState
{
    /// <summary>
    /// The depth charge is inactive (fabricating, or in inventory)
    /// </summary>
    Inactive,
    /// <summary>
    /// The depth charge is priming
    /// </summary>
    Priming,
    /// <summary>
    /// The depth charge is armed
    /// </summary>
    Armed,
    /// <summary>
    /// The depth charge made contact with something (will detonate shortly)
    /// </summary>
    Collision,
    /// <summary>
    /// The depth charge has detonated (will be destroyed soon)
    /// </summary>
    Detonated
}