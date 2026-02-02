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
        Main.logger.LogInfo("[ExplosiveTorpedoTagger] Awake() called on " + gameObject.name);
        gameObject.tag = "Projectile";  // Use an existing tag instead
        Main.logger.LogInfo("[ExplosiveTorpedoTagger] Tagged as 'Projectile'");
    }
}
