namespace VELD.AlterraWeaponry.Behaviours;

internal class LaserArmBehaviour : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private bool isActive = false;
    private float lastDamageTime = 0f;
    private const float damageCooldown = 0.1f; // Damage 10 times per second
    private const float laserRange = 150f;
    private const float damagePerHit = 10f;

    private Exosuit exosuit;
    private int armSlotID = -1;

    private void Awake()
    {
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        // Create a new GameObject for the line renderer
        GameObject laserObj = new GameObject("LaserBeam");
        laserObj.transform.SetParent(transform);
        laserObj.transform.localPosition = Vector3.zero;

        lineRenderer = laserObj.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(1f, 0.2f, 0.2f, 0.8f); // Red laser
        lineRenderer.endColor = new Color(1f, 0.2f, 0.2f, 0.3f); // Fading red
        lineRenderer.startWidth = 0.5f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    public void StartFiring(Exosuit sourceExosuit, int slotID)
    {
        exosuit = sourceExosuit;
        armSlotID = slotID;
        isActive = true;
        if (lineRenderer != null)
            lineRenderer.enabled = true;
        Main.logger.LogDebug("Laser firing started.");
    }

    public void StopFiring()
    {
        isActive = false;
        if (lineRenderer != null)
            lineRenderer.enabled = false;
        Main.logger.LogDebug("Laser firing stopped.");
    }

    private void Update()
    {
        if (!isActive || exosuit == null || lineRenderer == null || armSlotID < 0)
            return;

        // Get laser origin from exosuit center and aim forward
        Transform exosuitTransform = exosuit.transform;
        Vector3 laserOrigin = exosuitTransform.position + exosuitTransform.forward * 5f; // Offset forward from center

        // Direction the PRAWN is facing
        Vector3 laserDirection = exosuitTransform.forward;

        // Update line renderer
        lineRenderer.SetPosition(0, laserOrigin);

        // Raycast to find hits
        if (Physics.Raycast(laserOrigin, laserDirection, out RaycastHit hit, laserRange))
        {
            lineRenderer.SetPosition(1, hit.point);

            // Deal damage to creatures
            if (Time.time - lastDamageTime >= damageCooldown)
            {
                LiveMixin liveMixin = hit.collider.GetComponent<LiveMixin>();
                if (liveMixin != null && liveMixin.IsAlive())
                {
                    liveMixin.TakeDamage(damagePerHit, hit.point, DamageType.Electrical);
                    Main.logger.LogDebug($"Laser hit {liveMixin.gameObject.name} for {damagePerHit} damage.");
                    lastDamageTime = Time.time;
                }
            }
        }
        else
        {
            // Laser extends to max range if nothing hit
            lineRenderer.SetPosition(1, laserOrigin + laserDirection * laserRange);
        }
    }

    public void OnDestroy()
    {
        if (lineRenderer != null && lineRenderer.gameObject != null)
            Destroy(lineRenderer.gameObject);
    }
}
