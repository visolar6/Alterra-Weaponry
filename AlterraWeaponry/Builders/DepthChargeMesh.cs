namespace VELD.AlterraWeaponry.Builders;

internal static class DepthChargeMesh
{
    internal static readonly float SphereRadius = 0.4f;
    internal static readonly float SpikeLength = 0.2f;
    internal static readonly float SpikeBaseRadius = 0.05f;
    internal static readonly float SpikeTipRadius = 0.01f;
    internal static readonly int SpikeCount = 24;
    internal static readonly int SpikeRings = 4;

    private static Mesh _cachedBody;
    private static Mesh _cachedButton;

    /// <summary>
    /// Caches the combined body mesh
    /// </summary>
    private static void CacheBody()
    {
        // Create main sphere mesh
        var sphereMesh = CreateSphereMesh();

        // Create spike meshes
        var spikeMeshes = CreateSpikeMeshes();

        // Combine body meshes
        _cachedBody = CombineBodyMeshes(sphereMesh, spikeMeshes);
        _cachedBody.name = "DepthChargeBody";
    }

    /// <summary>
    /// Caches the button mesh
    /// </summary>
    private static void CacheButton()
    {
        // Create button mesh
        _cachedButton = CreateCylinderMesh();
        _cachedButton.name = "DepthChargeButton";
    }

    /// <summary>
    /// Gets the cached combined body mesh
    /// </summary>
    public static Mesh GetBody()
    {
        if (_cachedBody == null)
        {
            CacheBody();
        }

        return _cachedBody;
    }

    /// <summary>
    /// Gets the cached button mesh
    /// </summary>
    public static Mesh GetButton()
    {
        if (_cachedButton == null)
        {
            CacheButton();
        }

        return _cachedButton;
    }

    /// <summary>
    /// Creates a sphere mesh
    /// </summary>
    /// <returns></returns>
    private static Mesh CreateSphereMesh()
    {
        // Use Unity's built-in sphere but extract the mesh
        var tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var mesh = UnityEngine.Object.Instantiate(tempSphere.GetComponent<MeshFilter>().sharedMesh);
        UnityEngine.Object.Destroy(tempSphere);
        return mesh;
    }

    /// <summary>
    /// Creates spike meshes arranged around a sphere
    /// </summary>
    /// <returns></returns>
    private static CombineInstance[] CreateSpikeMeshes()
    {
        int spikesPerRing = SpikeCount / SpikeRings;

        var coneMesh = CreateConeMesh();
        var combineInstances = new CombineInstance[SpikeCount];
        int spikeIndex = 0;

        float sphereScale = SphereRadius / 0.5f; // Match sphere scaling

        for (int ring = 0; ring < SpikeRings && spikeIndex < SpikeCount; ring++)
        {
            // Distribute rings evenly between -50 and +50 degrees
            float latitudeNormalized = (float)ring / (SpikeRings - 1);
            float latitude = Mathf.Lerp(-Mathf.PI * 0.278f, Mathf.PI * 0.278f, latitudeNormalized);

            float y = Mathf.Sin(latitude);
            float ringRadius = Mathf.Cos(latitude);

            for (int i = 0; i < spikesPerRing && spikeIndex < SpikeCount; i++, spikeIndex++)
            {
                float longitude = 2f * Mathf.PI * i / spikesPerRing;

                float x = ringRadius * Mathf.Cos(longitude);
                float z = ringRadius * Mathf.Sin(longitude);

                Vector3 direction = new Vector3(x, y, z).normalized;

                // Position spike base slightly inside sphere surface for blending
                Vector3 position = direction * SphereRadius * 0.98f;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
                Vector3 scale = new(1.0f * sphereScale, SpikeLength * sphereScale, 1.0f * sphereScale); // Drastically thicker spikes, scaled

                combineInstances[spikeIndex].mesh = coneMesh;
                combineInstances[spikeIndex].transform = Matrix4x4.TRS(position, rotation, scale);
            }
        }

        return combineInstances;
    }

    /// <summary>
    /// Combines the sphere and spike meshes into a single mesh
    /// </summary>
    /// <param name="sphereMesh"></param>
    /// <param name="spikeMeshes"></param>
    /// <returns></returns>
    private static Mesh CombineBodyMeshes(Mesh sphereMesh, CombineInstance[] spikeMeshes)
    {
        // Create combine instance for sphere (scaled to SphereRadius)
        var allCombines = new CombineInstance[spikeMeshes.Length + 1];

        float sphereScale = SphereRadius / 0.5f; // Unity sphere mesh has radius 0.5
        allCombines[0].mesh = sphereMesh;
        allCombines[0].transform = Matrix4x4.Scale(Vector3.one * sphereScale);

        for (int i = 0; i < spikeMeshes.Length; i++)
        {
            allCombines[i + 1] = spikeMeshes[i];
        }

        Mesh combinedMesh = new();
        combinedMesh.CombineMeshes(allCombines, true, true);
        combinedMesh.RecalculateNormals();
        combinedMesh.RecalculateBounds();

        return combinedMesh;
    }

    /// <summary>
    /// Creates a cone mesh
    /// </summary>
    /// <returns></returns>
    private static Mesh CreateConeMesh()
    {
        Mesh mesh = new();
        int segments = 12;

        int vertexCount = segments * 2 + 2; // Top ring + bottom ring + 2 centers
        Vector3[] vertices = new Vector3[vertexCount];

        // Top center
        vertices[0] = new Vector3(0, 1, 0);

        // Top ring vertices
        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            float x = Mathf.Cos(angle) * SpikeTipRadius;
            float z = Mathf.Sin(angle) * SpikeTipRadius;
            vertices[i + 1] = new Vector3(x, 1, z);
        }

        // Bottom ring vertices
        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            float x = Mathf.Cos(angle) * SpikeBaseRadius;
            float z = Mathf.Sin(angle) * SpikeBaseRadius;
            vertices[segments + 1 + i] = new Vector3(x, 0, z);
        }

        // Bottom center
        vertices[segments * 2 + 1] = new Vector3(0, 0, 0);

        // Calculate triangle count: top cap + sides (2 tris per segment) + bottom cap
        int[] triangles = new int[segments * 3 + segments * 6 + segments * 3];
        int triIndex = 0;

        // Top cap (from center to top ring) - counter-clockwise when viewed from above
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            triangles[triIndex++] = 0; // Top center
            triangles[triIndex++] = next + 1; // Next top ring vertex (swapped order)
            triangles[triIndex++] = i + 1; // Current top ring vertex
        }

        // Side quads (top ring to bottom ring)
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            int topCurrent = i + 1;
            int topNext = next + 1;
            int bottomCurrent = segments + 1 + i;
            int bottomNext = segments + 1 + next;

            // First triangle of quad
            triangles[triIndex++] = topCurrent;
            triangles[triIndex++] = bottomNext;
            triangles[triIndex++] = bottomCurrent;

            // Second triangle of quad
            triangles[triIndex++] = topCurrent;
            triangles[triIndex++] = topNext;
            triangles[triIndex++] = bottomNext;
        }

        // Bottom cap (from center to bottom ring)
        int bottomCenter = segments * 2 + 1;
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            triangles[triIndex++] = bottomCenter; // Bottom center
            triangles[triIndex++] = segments + 1 + next; // Next bottom ring vertex
            triangles[triIndex++] = segments + 1 + i; // Current bottom ring vertex
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// Creates a cylinder mesh
    /// </summary>
    /// <returns></returns>
    private static Mesh CreateCylinderMesh()
    {
        // Use Unity's built-in cylinder but extract the mesh
        var tempCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        var mesh = UnityEngine.Object.Instantiate(tempCylinder.GetComponent<MeshFilter>().sharedMesh);
        UnityEngine.Object.Destroy(tempCylinder);
        return mesh;
    }
}