namespace VELD.AlterraWeaponry.Utilities;

/// <summary>
/// Represents a single drop entry for breakable resources (outcrops).
/// </summary>
internal class OutcropDropData
{
    public TechType TechType { get; set; }
    public float chance { get; set; }

    public override string ToString()
    {
        return $"OutcropDropData {{ TechType: {TechType}, chance: {chance} }}";
    }

    public string ToString(string prefix)
    {
        return $"{prefix}OutcropDropData {{ TechType: {TechType}, chance: {chance} }}";
    }
}
