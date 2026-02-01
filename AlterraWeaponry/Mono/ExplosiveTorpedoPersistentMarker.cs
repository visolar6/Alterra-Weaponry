using UnityEngine;

namespace VELD.AlterraWeaponry.Mono;

/// <summary>
/// Persists through cloning to mark a torpedo as explosive
/// Attached during prefab load
/// </summary>
public class ExplosiveTorpedoPersistentMarker : MonoBehaviour
{
    // This is just a marker - the presence of this component indicates it's an ExplosiveTorpedo
}
