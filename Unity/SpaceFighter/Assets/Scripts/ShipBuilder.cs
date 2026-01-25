using UnityEngine;

/// <summary>
/// Utility script for generating a simple ship model from primitive shapes.
/// Attach to an empty GameObject and it will create a basic fighter ship.
/// Can be used as a placeholder until proper 3D models are imported.
/// </summary>
[ExecuteInEditMode]
public class ShipBuilder : MonoBehaviour
{
    [Header("Ship Colors")]
    [SerializeField] private Color hullColor = new Color(0.3f, 0.3f, 0.35f);
    [SerializeField] private Color cockpitColor = new Color(0.2f, 0.4f, 0.6f);
    [SerializeField] private Color engineColor = new Color(0.2f, 0.2f, 0.25f);
    [SerializeField] private Color accentColor = new Color(0.8f, 0.3f, 0.1f);

    [Header("Build Settings")]
    [SerializeField] private bool buildOnStart = true;
    [SerializeField] private bool addCollider = true;

    private void Start()
    {
        if (buildOnStart && Application.isPlaying)
        {
            BuildShip();
        }
    }

    /// <summary>
    /// Builds the ship model from primitive shapes.
    /// Call this from the inspector context menu or at runtime.
    /// </summary>
    [ContextMenu("Build Ship")]
    public void BuildShip()
    {
        // Clear existing children
        ClearChildren();

        // Create materials
        Material hullMat = CreateMaterial(hullColor);
        Material cockpitMat = CreateMaterial(cockpitColor);
        cockpitMat.SetFloat("_Metallic", 0.8f);
        cockpitMat.SetFloat("_Smoothness", 0.9f);
        Material engineMat = CreateMaterial(engineColor);
        Material accentMat = CreateMaterial(accentColor);

        // Main fuselage (elongated capsule)
        GameObject fuselage = CreatePrimitive("Fuselage", PrimitiveType.Capsule, hullMat);
        fuselage.transform.localScale = new Vector3(1f, 2f, 1f);
        fuselage.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        fuselage.transform.localPosition = Vector3.zero;

        // Nose cone
        GameObject nose = CreatePrimitive("Nose", PrimitiveType.Sphere, hullMat);
        nose.transform.localScale = new Vector3(0.8f, 0.6f, 1.2f);
        nose.transform.localPosition = new Vector3(0f, 0f, 2.5f);

        // Cockpit
        GameObject cockpit = CreatePrimitive("Cockpit", PrimitiveType.Sphere, cockpitMat);
        cockpit.transform.localScale = new Vector3(0.6f, 0.4f, 0.8f);
        cockpit.transform.localPosition = new Vector3(0f, 0.4f, 1f);

        // Left wing
        GameObject leftWing = CreatePrimitive("LeftWing", PrimitiveType.Cube, hullMat);
        leftWing.transform.localScale = new Vector3(3f, 0.1f, 1.5f);
        leftWing.transform.localPosition = new Vector3(-1.5f, 0f, -0.5f);
        leftWing.transform.localRotation = Quaternion.Euler(0f, 0f, -10f);

        // Right wing
        GameObject rightWing = CreatePrimitive("RightWing", PrimitiveType.Cube, hullMat);
        rightWing.transform.localScale = new Vector3(3f, 0.1f, 1.5f);
        rightWing.transform.localPosition = new Vector3(1.5f, 0f, -0.5f);
        rightWing.transform.localRotation = Quaternion.Euler(0f, 0f, 10f);

        // Left engine pod
        GameObject leftEngine = CreatePrimitive("LeftEngine", PrimitiveType.Cylinder, engineMat);
        leftEngine.transform.localScale = new Vector3(0.4f, 1f, 0.4f);
        leftEngine.transform.localPosition = new Vector3(-2f, 0f, -1f);
        leftEngine.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // Right engine pod
        GameObject rightEngine = CreatePrimitive("RightEngine", PrimitiveType.Cylinder, engineMat);
        rightEngine.transform.localScale = new Vector3(0.4f, 1f, 0.4f);
        rightEngine.transform.localPosition = new Vector3(2f, 0f, -1f);
        rightEngine.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // Engine glow (left)
        GameObject leftGlow = CreatePrimitive("LeftEngineGlow", PrimitiveType.Sphere, accentMat);
        leftGlow.transform.localScale = new Vector3(0.35f, 0.35f, 0.2f);
        leftGlow.transform.localPosition = new Vector3(-2f, 0f, -2f);

        // Engine glow (right)
        GameObject rightGlow = CreatePrimitive("RightEngineGlow", PrimitiveType.Sphere, accentMat);
        rightGlow.transform.localScale = new Vector3(0.35f, 0.35f, 0.2f);
        rightGlow.transform.localPosition = new Vector3(2f, 0f, -2f);

        // Vertical stabilizer
        GameObject stabilizer = CreatePrimitive("Stabilizer", PrimitiveType.Cube, hullMat);
        stabilizer.transform.localScale = new Vector3(0.1f, 1f, 1f);
        stabilizer.transform.localPosition = new Vector3(0f, 0.5f, -1.5f);

        // Add main collider for physics
        if (addCollider)
        {
            BoxCollider mainCollider = gameObject.AddComponent<BoxCollider>();
            mainCollider.size = new Vector3(6f, 1.5f, 5f);
            mainCollider.center = new Vector3(0f, 0f, -0.5f);
        }

        Debug.Log("Ship built successfully!");
    }

    /// <summary>
    /// Creates a primitive shape as a child object.
    /// </summary>
    private GameObject CreatePrimitive(string name, PrimitiveType type, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        // Apply material
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer != null && mat != null)
        {
            renderer.material = mat;
        }

        // Remove individual colliders (we use one main collider)
        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            DestroyImmediate(col);
        }

        return obj;
    }

    /// <summary>
    /// Creates a simple material with the given color.
    /// </summary>
    private Material CreateMaterial(Color color)
    {
        // Use Standard shader for better lighting
        Shader shader = Shader.Find("Standard");
        if (shader == null)
        {
            shader = Shader.Find("Diffuse");
        }

        Material mat = new Material(shader);
        mat.color = color;
        mat.SetFloat("_Metallic", 0.5f);
        mat.SetFloat("_Smoothness", 0.5f);

        return mat;
    }

    /// <summary>
    /// Removes all child objects.
    /// </summary>
    [ContextMenu("Clear Ship")]
    public void ClearChildren()
    {
        // Destroy in reverse order to avoid index issues
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Remove existing collider
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            DestroyImmediate(col);
        }
    }
}
