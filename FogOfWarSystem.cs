using UnityEngine;
using System.Collections.Generic;

public class FogOfWarSystem : MonoBehaviour
{
    public static FogOfWarSystem Instance;

    [Header("Map Settings")]
    public float mapSize = 100f; // Size of the map (100x100)
    public int textureResolution = 128; // Resolution of the Fog Texture (higher = smoother but heavier)
    public Color fogColor = new Color(0, 0, 0, 1f); // Black Opaque
    
    [Header("Visuals")]
    public Material fogMaterial; // Assign a Transparent Material here
    public float updateInterval = 0.1f; // Update fog every 0.1s

    private Texture2D fogTexture;
    private Color[] fogColors;
    private float timer;
    private GameObject fogPlane;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializeFogTexture();
        CreateFogPlane();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            UpdateFog();
            timer = 0f;
        }
    }

    void InitializeFogTexture()
    {
        fogTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, false);
        fogTexture.wrapMode = TextureWrapMode.Clamp;
        fogTexture.filterMode = FilterMode.Bilinear; // Smooth edges

        fogColors = new Color[textureResolution * textureResolution];
        ResetFog(); // Fill with black
    }

    void ResetFog()
    {
        for (int i = 0; i < fogColors.Length; i++)
        {
            fogColors[i] = fogColor;
        }
        fogTexture.SetPixels(fogColors);
        fogTexture.Apply();
    }

    void CreateFogPlane()
    {
        // Auto-create the visual plane if not assigned
        fogPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fogPlane.name = "FogOfWar_Plane";
        fogPlane.transform.SetParent(transform);
        
        // Rotate to face up (Quads face -Z by default, need to be flat on Y)
        fogPlane.transform.eulerAngles = new Vector3(90, 0, 0);
        
        // Scale: MapSize 100 means Scale 100 for a 1x1 Quad
        fogPlane.transform.localScale = new Vector3(mapSize, mapSize, 1);
        
        // Position: Center (0, Y, 0) assuming map is centered at 0,0
        // Lift slightly above ground (Y=5 or similar) to cover everything
        fogPlane.transform.position = new Vector3(0, 8f, 0);

        // 🛡️ IMPORTANT: Destroy the Collider so it doesn't block mouse clicks!
        if (fogPlane.TryGetComponent(out Collider c)) Destroy(c);

        // Assign Material
        Renderer r = fogPlane.GetComponent<Renderer>();
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;

        if (fogMaterial == null)
        {
            // 🎨 Fix: Use "Sprites/Default" which works in URP and Built-in Universal for transparency
            Shader shader = Shader.Find("Sprites/Default"); 
            if (shader == null) shader = Shader.Find("Transparent/Diffuse"); // Fallback

            fogMaterial = new Material(shader);
            fogMaterial.color = fogColor; // Sprites/Default uses simple color tint
        }

        r.material = fogMaterial;
        r.material.mainTexture = fogTexture;
    }

    void UpdateFog()
    {
        // 1. Reset Fog to Opaque (Simple 'Unseen' method)
        // If we want "Visited" state (Grey), we wouldn't reset fully here.
        // For MVP, we redraw the fog every frame to handle moving units clearing "active" vision.
        // Actually, re-filling the array every frame is costly.
        // Optimization: Don't clear. Just draw black over everything? No, that's worse.
        // Let's Reset to Black for now (Classic "Darkness returns" style).
        for (int i = 0; i < fogColors.Length; i++) fogColors[i] = fogColor;

        // 2. Find all Player Units
        // Optimization: Use a cached list managed by a UnitManager if possible. But FindObjects is okay for limited units (<100).
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        
        // 3. Carve Holes
        foreach (Unit unit in allUnits)
        {
            if (unit.team == Unit.Team.Player && unit.IsAlive())
            {
                ClearFogAround(unit.transform.position, unit.visionRange);
            }
        }
        
        // Also Buildings!
        BuildingBase[] buildings = FindObjectsByType<BuildingBase>(FindObjectsSortMode.None);
        foreach (var b in buildings)
        {
            if (b.team == Unit.Team.Player && b.IsAlive())
            {
                // Buildings have fixed vision, say 15f
                ClearFogAround(b.transform.position, 15f); 
            }
        }

        // 4. Apply
        fogTexture.SetPixels(fogColors);
        fogTexture.Apply();
    }

    void ClearFogAround(Vector3 position, float radius)
    {
        // Convert World Position to Texture Coords
        // Map: (-50, -50) to (50, 50) -> uv (0,0) to (1,1)
        
        float worldToUV = 1f / mapSize;
        float centerUV_X = (position.x + mapSize * 0.5f) * worldToUV;
        float centerUV_Y = (position.z + mapSize * 0.5f) * worldToUV;

        int centerX = Mathf.RoundToInt(centerUV_X * textureResolution);
        int centerY = Mathf.RoundToInt(centerUV_Y * textureResolution);
        
        // Radius in pixels
        int radiusPixels = Mathf.RoundToInt((radius * worldToUV) * textureResolution);
        int rSquared = radiusPixels * radiusPixels;

        for (int y = -radiusPixels; y <= radiusPixels; y++)
        {
            for (int x = -radiusPixels; x <= radiusPixels; x++)
            {
                if (x*x + y*y <= rSquared)
                {
                    int px = centerX + x;
                    int py = centerY + y;

                    // Bounds Check
                    if (px >= 0 && px < textureResolution && py >= 0 && py < textureResolution)
                    {
                        // Set Alpha to 0 (Transparent)
                        // Use Soft Edges?
                        float distSq = x*x + y*y;
                        float alpha = 0f;
                        
                        // Simple Soft Edge
                        // if (distSq > rSquared * 0.8f) alpha = 0.5f;

                        int index = py * textureResolution + px;
                        fogColors[index] = new Color(0, 0, 0, alpha); 
                    }
                }
            }
        }
    }
}
