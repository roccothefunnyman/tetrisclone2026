using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using SpaceFighter.Core;
using SpaceFighter.Systems;
using SpaceFighter.UI;

/// <summary>
/// Editor wizard that automatically sets up the complete Space Fighter prototype scene.
/// Updated for multi-station architecture (Phase 1).
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
        GUILayout.Label("Multi-Station Architecture - Phase 1", EditorStyles.miniLabel);
        GUILayout.Space(10);

        GUILayout.Label("This will set up the complete prototype scene including:");
        GUILayout.Label("  - Ship with modular systems (Power, Navigation, Hull)");
        GUILayout.Label("  - Camera system (cockpit + chase views)");
        GUILayout.Label("  - Starfield environment");
        GUILayout.Label("  - Unified Fighter HUD");
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
    }

    /// <summary>
    /// Sets up the complete scene with all components.
    /// </summary>
    public static void SetupScene()
    {
        Debug.Log("Setting up Space Fighter Prototype Scene (Multi-Station Architecture)...");

        SetupSkybox();
        SetupLighting();
        SetupPlayerShip();
        SetupCamera();
        SetupStarfield();
        SetupHUD();

        Debug.Log("Scene setup complete! Press Play to test.");
        EditorUtility.DisplayDialog("Setup Complete",
            "Space Fighter prototype scene has been set up!\n\n" +
            "Press Play to test the controls:\n\n" +
            "FLIGHT:\n" +
            "- WASD: Pitch/Yaw\n" +
            "- Q/E: Roll\n" +
            "- Shift/Ctrl: Throttle\n" +
            "- Space: Boost\n" +
            "- X: Toggle dampening\n" +
            "- Arrow Keys: Strafe\n\n" +
            "POWER:\n" +
            "- 1: Balanced\n" +
            "- 2: Combat\n" +
            "- 3: Speed\n" +
            "- 4: Defensive\n\n" +
            "CAMERA:\n" +
            "- Tab: Switch view\n" +
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
        Ship existingShip = Object.FindFirstObjectByType<Ship>();
        if (existingShip != null)
        {
            Debug.Log("Player ship already exists");
            return;
        }

        // Create player ship
        GameObject shipObj = new GameObject("PlayerShip");
        shipObj.transform.position = Vector3.zero;

        // Add Ship component (this will auto-create systems)
        Ship ship = shipObj.AddComponent<Ship>();

        // Add ShipBuilder for visual model
        ShipBuilder builder = shipObj.AddComponent<ShipBuilder>();
        builder.BuildShip();

        // Create default ShipConfig
        ShipConfig config = ScriptableObject.CreateInstance<ShipConfig>();
        config.shipName = "Fighter-01";
        config.shipClass = "Fighter";

        // Save config asset
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Configs"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Configs");
        }
        AssetDatabase.CreateAsset(config, "Assets/Resources/Configs/DefaultFighterConfig.asset");

        // Assign config to ship
        SetPrivateField(ship, "shipConfig", config);

        // Save as prefab
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Ships"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Ships");
        }

        string prefabPath = "Assets/Prefabs/Ships/PlayerShip.prefab";
        PrefabUtility.SaveAsPrefabAsset(shipObj, prefabPath);

        Debug.Log("Player ship created with modular systems");
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

    private static void SetupHUD()
    {
        // Check if HUD exists
        UnifiedFighterHUD existingHUD = Object.FindFirstObjectByType<UnifiedFighterHUD>();
        if (existingHUD != null)
        {
            Debug.Log("HUD already exists");
            return;
        }

        // Create Canvas
        GameObject canvasObj = new GameObject("FighterHUD");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Add UnifiedFighterHUD
        UnifiedFighterHUD hud = canvasObj.AddComponent<UnifiedFighterHUD>();

        // Create HUD panels
        CreateTopPanel(canvasObj.transform, hud);
        CreateBottomLeftPanel(canvasObj.transform, hud);
        CreateBottomCenterPanel(canvasObj.transform, hud);
        CreateBottomRightPanel(canvasObj.transform, hud);
        CreateCenterCrosshair(canvasObj.transform, hud);

        Debug.Log("Unified Fighter HUD created");
    }

    private static void CreateTopPanel(Transform parent, UnifiedFighterHUD hud)
    {
        GameObject panel = CreatePanel("TopPanel", parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -10);
        rt.sizeDelta = new Vector2(-40, 80);

        HorizontalLayoutGroup hlg = panel.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 30;
        hlg.padding = new RectOffset(20, 20, 10, 10);
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;

        // Hull section
        GameObject hullSection = CreateSection("HullSection", panel.transform, 200);
        TextMeshProUGUI hullText = CreateText("HullText", hullSection.transform, "HULL: 100%", 22);
        hullText.color = HexColor("33FF66");
        SetPrivateField(hud, "hullText", hullText);

        // Ship name section
        GameObject nameSection = CreateSection("NameSection", panel.transform, 200);
        TextMeshProUGUI nameText = CreateText("ShipName", nameSection.transform, "FIGHTER-01", 24);
        nameText.color = HexColor("FFFFFF");
        nameText.alignment = TextAlignmentOptions.Center;

        // Power section
        GameObject powerSection = CreateSection("PowerSection", panel.transform, 250);
        TextMeshProUGUI powerText = CreateText("PowerText", powerSection.transform, "PWR: [BALANCED]", 20);
        powerText.color = HexColor("33FF66");
        powerText.alignment = TextAlignmentOptions.Right;
        SetPrivateField(hud, "powerPresetText", powerText);
    }

    private static void CreateBottomLeftPanel(Transform parent, UnifiedFighterHUD hud)
    {
        GameObject panel = CreatePanel("BottomLeftPanel", parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = new Vector2(20, 20);
        rt.sizeDelta = new Vector2(280, 150);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.LowerLeft;
        vlg.spacing = 5;
        vlg.padding = new RectOffset(15, 15, 10, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        // Orientation
        TextMeshProUGUI headingText = CreateText("HeadingText", panel.transform, "HDG 0°", 18);
        headingText.color = HexColor("33FF66");
        SetPrivateField(hud, "headingText", headingText);

        TextMeshProUGUI pitchText = CreateText("PitchText", panel.transform, "PIT 0°", 18);
        pitchText.color = HexColor("33FF66");
        SetPrivateField(hud, "pitchText", pitchText);

        TextMeshProUGUI rollText = CreateText("RollText", panel.transform, "ROL 0°", 18);
        rollText.color = HexColor("33FF66");
        SetPrivateField(hud, "rollText", rollText);

        // Dampening
        TextMeshProUGUI dampText = CreateText("DampeningText", panel.transform, "DAMP: ON", 18);
        dampText.color = HexColor("33FF66");
        SetPrivateField(hud, "dampeningText", dampText);
    }

    private static void CreateBottomCenterPanel(Transform parent, UnifiedFighterHUD hud)
    {
        GameObject panel = CreatePanel("BottomCenterPanel", parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 20);
        rt.sizeDelta = new Vector2(300, 100);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 5;
        vlg.padding = new RectOffset(15, 15, 10, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        // Speed
        TextMeshProUGUI speedText = CreateText("SpeedText", panel.transform, "0 m/s", 28);
        speedText.color = HexColor("33FF66");
        speedText.alignment = TextAlignmentOptions.Center;
        SetPrivateField(hud, "speedText", speedText);

        // Throttle
        TextMeshProUGUI throttleText = CreateText("ThrottleText", panel.transform, "THR: 0%", 20);
        throttleText.color = HexColor("33FF66");
        throttleText.alignment = TextAlignmentOptions.Center;
        SetPrivateField(hud, "throttleText", throttleText);
    }

    private static void CreateBottomRightPanel(Transform parent, UnifiedFighterHUD hud)
    {
        GameObject panel = CreatePanel("BottomRightPanel", parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-20, 20);
        rt.sizeDelta = new Vector2(200, 120);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.LowerRight;
        vlg.spacing = 3;
        vlg.padding = new RectOffset(15, 15, 10, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        // Power distribution
        TextMeshProUGUI engineText = CreateText("EngineText", panel.transform, "E: 33%", 16);
        engineText.color = HexColor("33FF66");
        engineText.alignment = TextAlignmentOptions.Right;
        SetPrivateField(hud, "enginePowerText", engineText);

        TextMeshProUGUI weaponText = CreateText("WeaponText", panel.transform, "W: 33%", 16);
        weaponText.color = HexColor("FF6633");
        weaponText.alignment = TextAlignmentOptions.Right;
        SetPrivateField(hud, "weaponPowerText", weaponText);

        TextMeshProUGUI shieldText = CreateText("ShieldText", panel.transform, "S: 33%", 16);
        shieldText.color = HexColor("3399FF");
        shieldText.alignment = TextAlignmentOptions.Right;
        SetPrivateField(hud, "shieldPowerText", shieldText);

        // Boost indicator
        GameObject boostObj = new GameObject("BoostIndicator");
        boostObj.transform.SetParent(panel.transform);
        RectTransform boostRT = boostObj.AddComponent<RectTransform>();
        boostRT.sizeDelta = new Vector2(100, 25);
        TextMeshProUGUI boostText = boostObj.AddComponent<TextMeshProUGUI>();
        boostText.text = "BOOST";
        boostText.fontSize = 20;
        boostText.color = HexColor("6699FF");
        boostText.alignment = TextAlignmentOptions.Right;
        boostObj.SetActive(false);
        SetPrivateField(hud, "boostIndicator", boostObj);
    }

    private static void CreateCenterCrosshair(Transform parent, UnifiedFighterHUD hud)
    {
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(parent);
        RectTransform rt = crosshairObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(40, 40);

        Image crosshairImage = crosshairObj.AddComponent<Image>();
        crosshairImage.color = new Color(0.2f, 1f, 0.4f, 0.8f);
        crosshairImage.sprite = CreateCrosshairSprite();

        SetPrivateField(hud, "crosshair", rt);
        SetPrivateField(hud, "crosshairImage", crosshairImage);

        // Critical warning (hidden by default)
        GameObject criticalObj = new GameObject("CriticalWarning");
        criticalObj.transform.SetParent(parent);
        RectTransform critRT = criticalObj.AddComponent<RectTransform>();
        critRT.anchorMin = new Vector2(0.5f, 0.5f);
        critRT.anchorMax = new Vector2(0.5f, 0.5f);
        critRT.anchoredPosition = new Vector2(0, 100);
        critRT.sizeDelta = new Vector2(300, 50);

        TextMeshProUGUI critText = criticalObj.AddComponent<TextMeshProUGUI>();
        critText.text = "!! HULL CRITICAL !!";
        critText.fontSize = 28;
        critText.color = HexColor("FF3333");
        critText.alignment = TextAlignmentOptions.Center;
        criticalObj.SetActive(false);

        SetPrivateField(hud, "criticalWarning", criticalObj);
    }

    private static GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.localScale = Vector3.one;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.08f, 0.7f);

        return panel;
    }

    private static GameObject CreateSection(string name, Transform parent, float width)
    {
        GameObject section = new GameObject(name);
        section.transform.SetParent(parent);

        RectTransform rt = section.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, 60);

        LayoutElement le = section.AddComponent<LayoutElement>();
        le.preferredWidth = width;

        return section;
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
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        Color white = Color.white;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                tex.SetPixel(x, y, transparent);
            }
        }

        int center = size / 2;
        int thickness = 2;
        int gap = 6;
        int length = 12;

        for (int i = gap; i < gap + length; i++)
        {
            for (int t = -thickness / 2; t <= thickness / 2; t++)
            {
                tex.SetPixel(center + i, center + t, white);
                tex.SetPixel(center - i, center + t, white);
            }
        }

        for (int i = gap; i < gap + length; i++)
        {
            for (int t = -thickness / 2; t <= thickness / 2; t++)
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
