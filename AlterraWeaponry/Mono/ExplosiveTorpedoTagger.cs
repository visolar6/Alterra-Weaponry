using UnityEngine;

namespace VELD.AlterraWeaponry.Mono;

/// <summary>
/// Behaviour added to ExplosiveTorpedo prefab that tags fired instances
/// </summary>
public class ExplosiveTorpedoTagger : MonoBehaviour
{
    private void Awake()
    {
        // When the torpedo is instantiated, tag it
        gameObject.tag = "Projectile";  // Use an existing tag instead
    }
}
