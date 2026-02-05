namespace VELD.AlterraWeaponry.Mono.DepthCharge;

/// <summary>
/// Creates a mesh for the depth charge completely from Unity primitives. Get on my level, Blender.
/// </summary>
internal static class DepthChargeBuilder
{
    private static Mesh? cachedBody;

    /// <summary>
    /// Creates the depth charge game object
    /// </summary>
    public static GameObject CreateGameObject(TechType techType)
    {
        try
        {
            var go = new GameObject("DepthCharge");

            SetupMeshAndRenderer(go);
            SetupPhysics(go);
            SetupWorldComponents(go, techType);
            SetupCollider(go);
            SetupVFXAndBehavior(go);
            SetupLiveMixin(go);
            SetupIndicatorLight(go);

            MaterialUtils.ApplySNShaders(go);
            go.tag = "Untagged";
            go.layer = 0;

            return go;
        }
        catch (Exception e)
        {
            Main.logger.LogError($"Failed to create Depth Charge prefab: {e}");
            throw;
        }
    }

    private static void SetupMeshAndRenderer(GameObject go)
    {
        var meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = GetBody();

        var renderer = go.AddComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Standard"))
        {
            color = new Color(0.8f, 0.8f, 0.8f) // Light gray
        };
        mat.SetFloat("_Metallic", 0.8f); // High metallic
        mat.SetFloat("_Glossiness", 0.7f); // Smooth/reflective
        var metalTexture = ResourceHandler.LoadTexture2DFromFile(Path.Combine("Assets", "Texture2D", "metal.png"));
        if (metalTexture != null)
        {
            mat.mainTexture = metalTexture;
            Main.logger.LogInfo("metal.png texture loaded and applied to depth charge material.");
        }
        else
        {
            Main.logger.LogWarning("metal.png texture NOT found or failed to load.");
        }
        renderer.material = mat;
    }

    private static void SetupPhysics(GameObject go)
    {
        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 10f;
        rb.drag = 0.7f; // Lower drag for smoother underwater motion
        rb.angularDrag = 0.6f; // Lower angular drag for more natural spin
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooths visual movement

        var worldForces = go.EnsureComponent<WorldForces>();
        worldForces.underwaterGravity = 0f;
        worldForces.underwaterDrag = 1f;
    }

    private static void SetupWorldComponents(GameObject go, TechType techType)
    {
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        go.EnsureComponent<PrefabIdentifier>();
        go.EnsureComponent<TechTag>().type = techType;

        var pickupable = go.EnsureComponent<Pickupable>();
        pickupable.isPickupable = true;
        pickupable.randomizeRotationWhenDropped = false;

        go.EnsureComponent<EntityTag>().slotType = EntitySlot.Type.Small;
        go.EnsureComponent<HandTarget>();
    }

    private static void SetupCollider(GameObject go)
    {
        var sphereCollider = go.AddComponent<SphereCollider>();
        sphereCollider.radius = DepthChargeConstants.meshSphereRadius;
        sphereCollider.isTrigger = false; // Solid collider for physics interactions
    }

    private static void SetupVFXAndBehavior(GameObject go)
    {
        var vfx = go.EnsureComponent<VFXFabricating>();
        vfx.scaleFactor = 0.25f;
        vfx.posOffset = new Vector3(0f, 0.5f, 0f);
        go.EnsureComponent<DepthChargeManager>();
        go.EnsureComponent<DepthChargeAudioVisual>();
    }

    private static void SetupLiveMixin(GameObject go)
    {
        var liveMixin = go.EnsureComponent<LiveMixin>();
        liveMixin.data = ScriptableObject.CreateInstance<LiveMixinData>();
        liveMixin.data.maxHealth = 10f;
        liveMixin.data.invincibleInCreative = false;
        liveMixin.data.destroyOnDeath = false;

        go.EnsureComponent<DepthChargeDamager>().liveMixin = liveMixin;
    }

    private static void SetupIndicatorLight(GameObject go)
    {
        var indicatorLight = new GameObject("IndicatorLight");
        indicatorLight.transform.SetParent(go.transform, false);
        indicatorLight.transform.localPosition = new Vector3(0f, DepthChargeConstants.meshSphereRadius, 0f);

        CreateIndicatorSphere(indicatorLight);
        CreateLightSource(indicatorLight);
    }

    private static void CreateIndicatorSphere(GameObject parent)
    {
        var sphereVisual = new GameObject("IndicatorSphere");
        sphereVisual.transform.SetParent(parent.transform, false);
        sphereVisual.transform.localPosition = Vector3.zero;

        var indicatorMeshFilter = sphereVisual.AddComponent<MeshFilter>();
        var spherePrimitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicatorMeshFilter.mesh = spherePrimitive.GetComponent<MeshFilter>().sharedMesh;
        UnityEngine.Object.Destroy(spherePrimitive);

        var indicatorMeshRenderer = sphereVisual.AddComponent<MeshRenderer>();
        var indicatorMat = new Material(Shader.Find("Standard"))
        {
            color = Color.red
        };
        indicatorMat.EnableKeyword("_EMISSION");
        indicatorMat.SetColor("_EmissionColor", Color.black);
        indicatorMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        indicatorMeshRenderer.material = indicatorMat;
        sphereVisual.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
    }

    private static void CreateLightSource(GameObject parent)
    {
        var lightSource = new GameObject("LightSource");
        lightSource.transform.SetParent(parent.transform, false);
        lightSource.transform.localPosition = new Vector3(0f, 0.15f, 0f); // Further above to illuminate whole sphere

        var light = lightSource.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0f, 0f); // Pure red
        light.intensity = 0f; // Start off
        light.range = 1f; // Increased range to cover the sphere better
        light.shadows = LightShadows.None;
        light.renderMode = LightRenderMode.ForcePixel;
        light.cullingMask = -1; // Render for all layers
    }

    /// <summary>
    /// Caches the combined body mesh
    /// </summary>
    private static Mesh CacheAndGetBody()
    {
        // Create main sphere mesh
        var sphereMesh = CreateSphereMesh();

        // Create spike meshes
        var spikeMeshes = CreateSpikeMeshes();

        // Combine body meshes
        cachedBody = CombineBodyMeshes(sphereMesh, spikeMeshes);
        cachedBody.name = "DepthChargeBody";
        return cachedBody;
    }

    /// <summary>
    /// Gets the cached combined body mesh
    /// </summary>
    private static Mesh GetBody()
    {
        if (cachedBody == null)
        {
            return CacheAndGetBody();
        }

        return cachedBody;
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
        int spikesPerRing = DepthChargeConstants.meshSpikeCount / DepthChargeConstants.meshSpikeRings;

        var coneMesh = CreateConeMesh();
        var combineInstances = new CombineInstance[DepthChargeConstants.meshSpikeCount];
        int spikeIndex = 0;

        float sphereScale = DepthChargeConstants.meshSphereRadius / 0.5f; // Match sphere scaling

        for (int ring = 0; ring < DepthChargeConstants.meshSpikeRings && spikeIndex < DepthChargeConstants.meshSpikeCount; ring++)
        {
            // Distribute rings evenly between -50 and +50 degrees
            float latitudeNormalized = (float)ring / (DepthChargeConstants.meshSpikeRings - 1);
            float latitude = Mathf.Lerp(-Mathf.PI * 0.278f, Mathf.PI * 0.278f, latitudeNormalized);

            float y = Mathf.Sin(latitude);
            float ringRadius = Mathf.Cos(latitude);

            for (int i = 0; i < spikesPerRing && spikeIndex < DepthChargeConstants.meshSpikeCount; i++, spikeIndex++)
            {
                float longitude = 2f * Mathf.PI * i / spikesPerRing;

                float x = ringRadius * Mathf.Cos(longitude);
                float z = ringRadius * Mathf.Sin(longitude);

                Vector3 direction = new Vector3(x, y, z).normalized;

                // Position spike base slightly inside sphere surface for blending
                Vector3 position = direction * DepthChargeConstants.meshSphereRadius * 0.98f;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
                Vector3 scale = new(1.0f * sphereScale, DepthChargeConstants.meshSpikeLength * sphereScale, 1.0f * sphereScale); // Drastically thicker spikes, scaled

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

        float sphereScale = DepthChargeConstants.meshSphereRadius / 0.5f; // Unity sphere mesh has radius 0.5
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
        Vector2[] uvs = new Vector2[vertexCount];

        // Top center
        vertices[0] = new Vector3(0, 1, 0);
        uvs[0] = new Vector2(0.5f, 1f); // Top center UV

        // Top ring vertices
        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            float x = Mathf.Cos(angle) * DepthChargeConstants.meshSpikeTipRadius;
            float z = Mathf.Sin(angle) * DepthChargeConstants.meshSpikeTipRadius;
            vertices[i + 1] = new Vector3(x, 1, z);
            // Map around the top edge
            uvs[i + 1] = new Vector2((float)i / segments, 1f);
        }

        // Bottom ring vertices
        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            float x = Mathf.Cos(angle) * DepthChargeConstants.meshSpikeBaseRadius;
            float z = Mathf.Sin(angle) * DepthChargeConstants.meshSpikeBaseRadius;
            vertices[segments + 1 + i] = new Vector3(x, 0, z);
            // Map around the bottom edge
            uvs[segments + 1 + i] = new Vector2((float)i / segments, 0f);
        }

        // Bottom center
        vertices[segments * 2 + 1] = new Vector3(0, 0, 0);
        uvs[segments * 2 + 1] = new Vector2(0.5f, 0f); // Bottom center UV

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
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}