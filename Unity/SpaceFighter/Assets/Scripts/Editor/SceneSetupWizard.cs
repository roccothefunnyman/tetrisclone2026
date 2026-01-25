using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor wizard that automatically sets up the complete Space Fighter prototype scene.
/// Run from menu: Tools > Space Fighter > Setup Prototype Scene
/// </summary>
public class SceneSetupWizard : EditorWindow
{
    [MenuItem("Tools/Space Fighter/Setup Prototype Scene")]
    public static void ShowWindow()
    {
        GetWindow<SceneSetupWizard>("Space Fighter Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Space Fighter Prototype Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("This will set up the complete prototype scene including:");
        GUILayout.Label("  - Player Ship with flight controls");
        GUILayout.Label("  - Camera system (cockpit + chase views)");
        GUILayout.Label("  - Starfield environment");
        GUILayout.Label("  - HUD with all displays");
        GUILayout.Label("  - Game Manager");
        GUILayout.Space(10);

        if (GUILayout.Button("Setup Complete Scene", GUILayout.Height(40)))
        {
            SetupScene();
        }

        GUILayout.Space(10);
        GUILayout.Label("Individual Setup:", EditorStyles.boldLabel);

        if (GUILayout.Button("1. Setup Skybox Only"))
        {
            SetupSkybox();
        }

        if (GUILayout.Button("2. Setup Player Ship Only"))
        {
            SetupPlayerShip();
        }

        if (GUILayout.Button("3. Setup Starfield Only"))
        {
            SetupStarfield();
        }

        if (GUILayout.Button("4. Setup HUD Only"))
        {
            SetupHUD();
        }

        if (GUILayout.Button("5. Setup Game Manager Only"))
        {
            SetupGameManager();
        }
    }

    /// <summary>
    /// Sets up the complete scene with all components.
    /// </summary>
    public static void SetupScene()
    {
        Debug.Log("Setting up Space Fighter Prototype Scene...");

        SetupSkybox();
        SetupLighting();
        SetupPlayerShip();
        SetupCamera();
        SetupStarfield();
        SetupGameManager();
        SetupHUD();

        Debug.Log("Scene setup complete! Press Play to test.");
        EditorUtility.DisplayDialog("Setup Complete",
            "Space Fighter prototype scene has been set up!\n\n" +
            "Press Play to test the controls:\n" +
            "- WASD: Pitch/Yaw\n" +
            "- Q/E: Roll\n" +
            "- Shift/Ctrl: Throttle\n" +
            "- Space: Boost\n" +
            "- Tab: Switch camera\n" +
            "- V: Toggle dampening\n" +
            "- Esc: Pause",
            "OK");
    }

    private static void SetupSkybox()
    {
        // Create skybox material
        Material skyboxMat = new Material(Shader.Find("Skybox/SpaceSkybox"));

        if (skyboxMat.shader.name == "Hidden/InternalErrorShader")
        {
            // Fallback to procedural skybox if custom shader not found
            skyboxMat = new Material(Shader.Find("Skybox/Procedural"));
            if (skyboxMat != null)
            {
                skyboxMat.SetColor("_SkyTint", new Color(0.02f, 0.02f, 0.05f));
                skyboxMat.SetColor("_GroundColor", new Color(0f, 0f, 0.02f));
                skyboxMat.SetFloat("_Exposure", 0.3f);
            }
            Debug.LogWarning("Custom skybox shader not found, using procedural fallback");
        }
        else
        {
            skyboxMat.SetColor("_TopColor", new Color(0.02f, 0.02f, 0.05f));
            skyboxMat.SetColor("_BottomColor", new Color(0f, 0f, 0.02f));
            skyboxMat.SetFloat("_NebulaIntensity", 0.3f);
            skyboxMat.SetFloat("_StarDensity", 500f);
            skyboxMat.SetFloat("_StarBrightness", 1f);
        }

        // Save material
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
        AssetDatabase.CreateAsset(skyboxMat, "Assets/Materials/SpaceSkyboxMaterial.mat");

        // Apply to scene
        RenderSettings.skybox = skyboxMat;

        Debug.Log("Skybox configured");
    }

    private static void SetupLighting()
    {
        // Find or create directional light
        Light dirLight = Object.FindFirstObjectByType<Light>();
        if (dirLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
        }

        dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        dirLight.intensity = 1f;
        dirLight.color = new Color(1f, 0.988f, 0.96f);

        // Ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.15f);

        Debug.Log("Lighting configured");
    }

    private static void SetupPlayerShip()
    {
        // Check if ship already exists
        ShipController existingShip = Object.FindFirstObjectByType<ShipController>();
        if (existingShip != null)
        {
            Debug.Log("Player ship already exists");
            return;
        }

        // Create player ship
        GameObject ship = new GameObject("PlayerShip");
        ship.transform.position = Vector3.zero;

        // Add Rigidbody
        Rigidbody rb = ship.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Add ShipController
        ship.AddComponent<ShipController>();

        // Add ShipBuilder and build the ship model
        ShipBuilder builder = ship.AddComponent<ShipBuilder>();
        builder.BuildShip();

        // Save as prefab
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // Create prefab
        string prefabPath = "Assets/Prefabs/PlayerShip.prefab";
        PrefabUtility.SaveAsPrefabAsset(ship, prefabPath);

        Debug.Log("Player ship created and saved as prefab");
    }

    private static void SetupCamera()
    {
        // Find main camera
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }

        // Add CameraController if not present
        CameraController camController = mainCam.GetComponent<CameraController>();
        if (camController == null)
        {
            camController = mainCam.gameObject.AddComponent<CameraController>();
        }

        // Configure camera
        mainCam.clearFlags = CameraClearFlags.Skybox;
        mainCam.backgroundColor = Color.black;
        mainCam.fieldOfView = 60f;
        mainCam.nearClipPlane = 0.1f;
        mainCam.farClipPlane = 2000f;

        Debug.Log("Camera configured with CameraController");
    }

    private static void SetupStarfield()
    {
        // Check if starfield exists
        StarfieldGenerator existingStarfield = Object.FindFirstObjectByType<StarfieldGenerator>();
        if (existingStarfield != null)
        {
            Debug.Log("Starfield already exists");
            return;
        }

        GameObject starfield = new GameObject("Starfield");
        starfield.AddComponent<StarfieldGenerator>();

        Debug.Log("Starfield created");
    }

    private static void SetupGameManager()
    {
        // Check if game manager exists
        GameManager existingGM = Object.FindFirstObjectByType<GameManager>();
        if (existingGM != null)
        {
            Debug.Log("Game Manager already exists");
            return;
        }

        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        Debug.Log("Game Manager created");
    }

    private static void SetupHUD()
    {
        // Check if canvas exists
        Canvas existingCanvas = Object.FindFirstObjectByType<Canvas>();
        if (existingCanvas != null && existingCanvas.GetComponent<HUDController>() != null)
        {
            Debug.Log("HUD already exists");
            return;
        }

        // Create Canvas
        GameObject canvasObj = new GameObject("HUDCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Add HUDController
        HUDController hudController = canvasObj.AddComponent<HUDController>();

        // Create HUD panels
        CreateTopLeftPanel(canvasObj.transform, hudController);
        CreateTopRightPanel(canvasObj.transform, hudController);
        CreateBottomLeftPanel(canvasObj.transform, hudController);
        CreateBottomRightPanel(canvasObj.transform, hudController);
        CreateCenterPanel(canvasObj.transform, hudController);
        CreatePausePanel(canvasObj.transform);

        Debug.Log("HUD created with all panels");
    }

    private static void CreateTopLeftPanel(Transform parent, HUDController hud)
    {
        GameObject panel = CreatePanel("TopLeftPanel", parent, TextAnchor.UpperLeft);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -20);
        rt.sizeDelta = new Vector2(300, 150);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.spacing = 5;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Speed text
        TextMeshProUGUI speedText = CreateText("SpeedText", panel.transform, "0 m/s", 28);
        speedText.color = HexColor("33FF66");

        // Max speed text
        TextMeshProUGUI maxSpeedText = CreateText("MaxSpeedText", panel.transform, "/ 200", 18);
        maxSpeedText.color = HexColor("66AA66");

        // Throttle text
        TextMeshProUGUI throttleText = CreateText("ThrottleText", panel.transform, "THR: 0%", 22);
        throttleText.color = HexColor("33FF66");

        // Assign references via serialized fields
        SetPrivateField(hud, "speedText", speedText);
        SetPrivateField(hud, "maxSpeedText", maxSpeedText);
        SetPrivateField(hud, "throttleText", throttleText);
    }

    private static void CreateTopRightPanel(Transform parent, HUDController hud)
    {
        GameObject panel = CreatePanel("TopRightPanel", parent, TextAnchor.UpperRight);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -20);
        rt.sizeDelta = new Vector2(250, 100);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperRight;
        vlg.spacing = 5;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        // Shield text
        TextMeshProUGUI shieldText = CreateText("ShieldText", panel.transform, "SHIELD: 100%", 20);
        shieldText.color = HexColor("3399FF");
        shieldText.alignment = TextAlignmentOptions.Right;

        SetPrivateField(hud, "shieldText", shieldText);
    }

    private static void CreateBottomLeftPanel(Transform parent, HUDController hud)
    {
        GameObject panel = CreatePanel("BottomLeftPanel", parent, TextAnchor.LowerLeft);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = new Vector2(20, 20);
        rt.sizeDelta = new Vector2(200, 100);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.LowerLeft;
        vlg.spacing = 2;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        // Orientation texts
        TextMeshProUGUI headingText = CreateText("HeadingText", panel.transform, "HDG 0", 16);
        headingText.color = HexColor("33FF66");

        TextMeshProUGUI pitchText = CreateText("PitchText", panel.transform, "PIT 0", 16);
        pitchText.color = HexColor("33FF66");

        TextMeshProUGUI rollText = CreateText("RollText", panel.transform, "ROL 0", 16);
        rollText.color = HexColor("33FF66");

        SetPrivateField(hud, "headingText", headingText);
        SetPrivateField(hud, "pitchText", pitchText);
        SetPrivateField(hud, "rollText", rollText);
    }

    private static void CreateBottomRightPanel(Transform parent, HUDController hud)
    {
        GameObject panel = CreatePanel("BottomRightPanel", parent, TextAnchor.LowerRight);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-20, 20);
        rt.sizeDelta = new Vector2(150, 80);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.LowerRight;
        vlg.spacing = 5;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        // Dampening text
        TextMeshProUGUI dampeningText = CreateText("DampeningText", panel.transform, "DAMP ON", 16);
        dampeningText.color = HexColor("33FF66");
        dampeningText.alignment = TextAlignmentOptions.Right;

        // Boost indicator
        GameObject boostObj = new GameObject("BoostIndicator");
        boostObj.transform.SetParent(panel.transform);
        TextMeshProUGUI boostText = boostObj.AddComponent<TextMeshProUGUI>();
        boostText.text = "BOOST";
        boostText.fontSize = 18;
        boostText.color = HexColor("6699FF");
        boostText.alignment = TextAlignmentOptions.Right;
        boostObj.SetActive(false);

        SetPrivateField(hud, "dampeningText", dampeningText);
        SetPrivateField(hud, "boostIndicator", boostObj);
    }

    private static void CreateCenterPanel(Transform parent, HUDController hud)
    {
        // Crosshair
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(parent);
        RectTransform crosshairRT = crosshairObj.AddComponent<RectTransform>();
        crosshairRT.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairRT.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRT.anchoredPosition = Vector2.zero;
        crosshairRT.sizeDelta = new Vector2(40, 40);

        Image crosshairImage = crosshairObj.AddComponent<Image>();
        crosshairImage.color = new Color(0.2f, 1f, 0.4f, 0.8f);

        // Create simple crosshair sprite (or use default)
        crosshairImage.sprite = CreateCrosshairSprite();

        SetPrivateField(hud, "crosshair", crosshairRT);
        SetPrivateField(hud, "crosshairImage", crosshairImage);

        // Velocity marker
        GameObject velocityObj = new GameObject("VelocityMarker");
        velocityObj.transform.SetParent(parent);
        RectTransform velocityRT = velocityObj.AddComponent<RectTransform>();
        velocityRT.anchorMin = new Vector2(0.5f, 0.5f);
        velocityRT.anchorMax = new Vector2(0.5f, 0.5f);
        velocityRT.anchoredPosition = Vector2.zero;
        velocityRT.sizeDelta = new Vector2(20, 20);

        Image velocityImage = velocityObj.AddComponent<Image>();
        velocityImage.color = new Color(0.2f, 1f, 0.4f, 0.6f);

        SetPrivateField(hud, "velocityMarker", velocityRT);
    }

    private static void CreatePausePanel(Transform parent)
    {
        GameObject pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(parent);

        RectTransform rt = pausePanel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Background
        Image bg = pausePanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);

        // Pause text
        GameObject textObj = new GameObject("PauseText");
        textObj.transform.SetParent(pausePanel.transform);
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.5f, 0.5f);
        textRT.anchorMax = new Vector2(0.5f, 0.5f);
        textRT.anchoredPosition = Vector2.zero;
        textRT.sizeDelta = new Vector2(400, 200);

        TextMeshProUGUI pauseText = textObj.AddComponent<TextMeshProUGUI>();
        pauseText.text = "PAUSED\n\nPress ESC to resume\nPress H for controls";
        pauseText.fontSize = 36;
        pauseText.color = HexColor("33FF66");
        pauseText.alignment = TextAlignmentOptions.Center;

        pausePanel.SetActive(false);

        // Assign to GameManager
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            SetPrivateField(gm, "pausePanel", pausePanel);
        }
    }

    private static GameObject CreatePanel(string name, Transform parent, TextAnchor alignment)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.localScale = Vector3.one;

        // Semi-transparent background
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.08f, 0.6f);

        return panel;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string text, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(280, fontSize + 10);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Normal;

        return tmp;
    }

    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }

    private static Sprite CreateCrosshairSprite()
    {
        // Create a simple crosshair texture
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        Color white = Color.white;

        // Fill with transparent
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                tex.SetPixel(x, y, transparent);
            }
        }

        // Draw crosshair
        int center = size / 2;
        int thickness = 2;
        int gap = 6;
        int length = 12;

        // Horizontal lines
        for (int i = gap; i < gap + length; i++)
        {
            for (int t = -thickness/2; t <= thickness/2; t++)
            {
                tex.SetPixel(center + i, center + t, white);
                tex.SetPixel(center - i, center + t, white);
            }
        }

        // Vertical lines
        for (int i = gap; i < gap + length; i++)
        {
            for (int t = -thickness/2; t <= thickness/2; t++)
            {
                tex.SetPixel(center + t, center + i, white);
                tex.SetPixel(center + t, center - i, white);
            }
        }

        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(obj, value);
            EditorUtility.SetDirty(obj as Object);
        }
    }
}
