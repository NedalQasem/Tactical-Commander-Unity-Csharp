using UnityEngine;
using System.Collections.Generic;

public class FogOfWarSystem : MonoBehaviour
{
    public static FogOfWarSystem Instance;

    [Header("Map Settings")]
    public float mapSize = 100f; 
    public int textureResolution = 128; 
    public Color fogColor = new Color(0, 0, 0, 1f); 
    
    [Header("Visuals")]
    public Material fogMaterial; 
    public float updateInterval = 0.1f;

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
        fogTexture.filterMode = FilterMode.Bilinear;

        fogColors = new Color[textureResolution * textureResolution];
        ResetFog();
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
        fogPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fogPlane.name = "FogOfWar_Plane";
        fogPlane.transform.SetParent(transform);
        
        fogPlane.transform.eulerAngles = new Vector3(90, 0, 0);
        
        fogPlane.transform.localScale = new Vector3(mapSize, mapSize, 1);
        
        fogPlane.transform.position = new Vector3(0, 8f, 0);

        if (fogPlane.TryGetComponent(out Collider c)) Destroy(c);

        Renderer r = fogPlane.GetComponent<Renderer>();
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;

        if (fogMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default"); 
            if (shader == null) shader = Shader.Find("Transparent/Diffuse"); // Fallback

            fogMaterial = new Material(shader);
            fogMaterial.color = fogColor; 
        }

        r.material = fogMaterial;
        r.material.mainTexture = fogTexture;
    }

    void UpdateFog()
    {
        for (int i = 0; i < fogColors.Length; i++) fogColors[i] = fogColor;

        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        
        foreach (Unit unit in allUnits)
        {
            if (unit.team == Unit.Team.Player && unit.IsAlive())
            {
                ClearFogAround(unit.transform.position, unit.visionRange);
            }
        }
                BuildingBase[] buildings = FindObjectsByType<BuildingBase>(FindObjectsSortMode.None);
        foreach (var b in buildings)
        {
            if (b.team == Unit.Team.Player && b.IsAlive())
            {
                ClearFogAround(b.transform.position, 15f); 
            }
        }

        // 4. Apply
        fogTexture.SetPixels(fogColors);
        fogTexture.Apply();
    }

    void ClearFogAround(Vector3 position, float radius)
    {        
        float worldToUV = 1f / mapSize;
        float centerUV_X = (position.x + mapSize * 0.5f) * worldToUV;
        float centerUV_Y = (position.z + mapSize * 0.5f) * worldToUV;

        int centerX = Mathf.RoundToInt(centerUV_X * textureResolution);
        int centerY = Mathf.RoundToInt(centerUV_Y * textureResolution);
        
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
                        float distSq = x*x + y*y;
                        float alpha = 0f;
                        

                        int index = py * textureResolution + px;
                        fogColors[index] = new Color(0, 0, 0, alpha); 
                    }
                }
            }
        }
    }
}
