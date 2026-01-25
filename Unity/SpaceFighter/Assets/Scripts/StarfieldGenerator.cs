using UnityEngine;

/// <summary>
/// Generates and manages a procedural starfield using Unity's particle system.
/// Stars follow the player to create an infinite space effect.
/// </summary>
public class StarfieldGenerator : MonoBehaviour
{
    [Header("Starfield Settings")]
    [SerializeField] private int starCount = 5000;
    [SerializeField] private float starfieldRadius = 500f;
    [SerializeField] private float minStarSize = 0.5f;
    [SerializeField] private float maxStarSize = 2f;

    [Header("Star Colors")]
    [SerializeField] private Color[] starColors = new Color[]
    {
        Color.white,
        new Color(0.9f, 0.9f, 1f),      // Blue-white
        new Color(1f, 0.95f, 0.8f),     // Yellow-white
        new Color(1f, 0.8f, 0.6f),      // Orange
        new Color(0.8f, 0.85f, 1f)      // Blue
    };

    [Header("Nebula Background")]
    [SerializeField] private bool enableNebula = true;
    [SerializeField] private Color nebulaColor1 = new Color(0.1f, 0.05f, 0.2f, 0.3f);
    [SerializeField] private Color nebulaColor2 = new Color(0.05f, 0.1f, 0.15f, 0.3f);

    [Header("Follow Settings")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private bool followPlayer = true;

    private ParticleSystem starParticles;
    private ParticleSystem.Particle[] particles;

    private void Awake()
    {
        CreateStarfield();
    }

    private void Start()
    {
        if (followTarget == null && followPlayer)
        {
            ShipController ship = FindFirstObjectByType<ShipController>();
            if (ship != null)
            {
                followTarget = ship.transform;
            }
        }
    }

    private void LateUpdate()
    {
        // Keep starfield centered on player for infinite space effect
        if (followPlayer && followTarget != null)
        {
            transform.position = followTarget.position;
        }
    }

    /// <summary>
    /// Creates the particle system for the starfield.
    /// </summary>
    private void CreateStarfield()
    {
        // Create or get particle system
        starParticles = GetComponent<ParticleSystem>();
        if (starParticles == null)
        {
            starParticles = gameObject.AddComponent<ParticleSystem>();
        }

        // Configure main module
        var main = starParticles.main;
        main.maxParticles = starCount;
        main.startLifetime = Mathf.Infinity;
        main.startSpeed = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.playOnAwake = true;
        main.loop = false;

        // Configure emission - emit all at once
        var emission = starParticles.emission;
        emission.enabled = false; // We'll set particles manually

        // Configure shape - not needed for manual positioning
        var shape = starParticles.shape;
        shape.enabled = false;

        // Configure renderer
        var renderer = GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            // Create simple star material if none exists
            if (renderer.material == null)
            {
                renderer.material = CreateStarMaterial();
            }
        }

        // Generate the stars
        GenerateStars();
    }

    /// <summary>
    /// Creates a simple material for star particles.
    /// </summary>
    private Material CreateStarMaterial()
    {
        // Use additive particle shader
        Shader shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Legacy Shaders/Particles/Additive");
        }
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material mat = new Material(shader);
        mat.SetFloat("_Mode", 2); // Fade mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);

        return mat;
    }

    /// <summary>
    /// Generates star particles in a sphere around the origin.
    /// </summary>
    private void GenerateStars()
    {
        particles = new ParticleSystem.Particle[starCount];

        for (int i = 0; i < starCount; i++)
        {
            // Random position on sphere surface
            Vector3 position = Random.onUnitSphere * starfieldRadius;

            // Add some depth variation
            position *= Random.Range(0.5f, 1f);

            particles[i].position = position;
            particles[i].startSize = Random.Range(minStarSize, maxStarSize);

            // Random color from palette with varying brightness
            Color starColor = starColors[Random.Range(0, starColors.Length)];
            float brightness = Random.Range(0.3f, 1f);
            starColor *= brightness;
            starColor.a = brightness;
            particles[i].startColor = starColor;

            particles[i].remainingLifetime = Mathf.Infinity;
        }

        starParticles.SetParticles(particles, starCount);
    }

    /// <summary>
    /// Regenerates the starfield with new random positions.
    /// </summary>
    public void RegenerateStarfield()
    {
        GenerateStars();
    }

    /// <summary>
    /// Sets the follow target for the starfield.
    /// </summary>
    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
    }

    /// <summary>
    /// Adjusts starfield density at runtime.
    /// </summary>
    public void SetStarCount(int count)
    {
        starCount = count;
        var main = starParticles.main;
        main.maxParticles = count;
        RegenerateStarfield();
    }
}
