using System;
using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
    //temp
    [SerializeField] private Texture2D scannerIcon;
    [SerializeField] float radius = 30f;
    [SerializeField] private Rect buttonRect;
//    [SerializeField] private float maxCharge = 100f;
//    [SerializeField] private float shrinkSpeed = 10f;
    [SerializeField] private GameObject gadgetModel;
//    [SerializeField] private MeshRenderer gadgetMeshRenderer;
//    [SerializeField] private Color flashColor;

//    private Color normalColor;
    private Material material;
    private Renderer renderer;
    private SphereCollider sphereCollider;
//    private int flashTweenId;
    
//    public float Charge { get; private set; }
    public bool IsScanning { get; private set; }

//    public float ChargeFraction
//    {
//        get { return Charge/maxCharge; }
//    }

    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = radius;
        sphereCollider.enabled = false;
        gadgetModel.SetActive(false);
        
    }
    
    void Start()
    {
        buttonRect = GameManager.Instance.Hud.CenteredRect(new Rect(Screen.width / 2, 200, 200, 200));
        renderer = GameManager.Instance.Planet.scannableMesh.GetComponent<Renderer>();
        material = renderer.material;
    }

    void Update()
    {
        if (!GameManager.Instance.RoundStarted || GameManager.Instance.Mode != GameManager.Modes.Scanning)
            return;
        
        CheckStartStop();

        if (IsScanning)
            HandleScanning();
    }

    void OnGUI()
    {
        if (IsScanning || GameManager.Instance.Mode != GameManager.Modes.Scanning)
            return;

        GUI.Label(new Rect(buttonRect.x, Screen.height - buttonRect.y - buttonRect.height, buttonRect.width, buttonRect.height), scannerIcon);
        
    }

    private void CheckStartStop()
    {
        if (IsScanning)
        {
            if (!Input.GetMouseButton(0))
            {
                EndScan();
            }
        }
        else
        {
            if (GameManager.Instance.TouchInput)
            {
                
            }
            else
            {
                if (Input.GetMouseButton(0) && buttonRect.Contains(Input.mousePosition) && GameManager.Instance.Mode == GameManager.Modes.Scanning)
                {
                    StartScan();
                }
            }
        }
    }

    private void StartScan()
    {
        IsScanning = true;
        sphereCollider.enabled = true;
        gadgetModel.SetActive(true);
        material.SetFloat("_Radius", radius);

        //        GameManager.Instance.Director.SetMode(Director.Modes.Grid, SelectedCity.transform);
    }

    private void EndScan()
    {
        IsScanning = false;
        sphereCollider.enabled = false;
        gadgetModel.SetActive(false);
        material.SetFloat("_Radius", 0);
        
//        GameManager.Instance.Director.SetMode(Director.Modes.Orbit, GameManager.Instance.PlanetTransform);
    }

    private void HandleScanning()
    {
        Vector2 rayPos;

        if (GameManager.Instance.TouchInput)
        {
            rayPos = Input.touches[0].position;
        }
        else
            rayPos = Input.mousePosition;

        if (true)
        {
            Ray ray = Camera.main.ScreenPointToRay(rayPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                UpdateScannerPosition(hit.point);

                GeoThermalPlant plant = hit.transform.GetComponent<GeoThermalPlant>();
                if (plant)
                {
                    EndScan();
                    GameManager.Instance.Mode = GameManager.Modes.DrillingGame;
                    plant.Build();
                }
            }
        }
    }

    private void UpdateScannerPosition(Vector3 position)
    {
        transform.position = position;

        Vector3 lookDir = position - GameManager.Instance.PlanetTransform.position;
        gadgetModel.transform.LookAt(position + lookDir);
        
        material.SetVector("_CenterPoint", new Vector4(position.x, position.y, position.z, 0));
    }
    
    void OnTriggerEnter(Collider other)
    {
        GeoThermalPlant geoPlant = other.GetComponent<GeoThermalPlant>();

        if (geoPlant)
            geoPlant.ShowPreview(true);

//        Debug.Log(other.name);
//        Pylon pylon = other.GetComponent<Pylon>();
//        
//        if (pylon)
//            pylon.ShowPreview(true);
    }

    void OnTriggerExit(Collider other)
    {
        GeoThermalPlant geoPlant = other.GetComponent<GeoThermalPlant>();

        if (geoPlant)
            geoPlant.ShowPreview(false);

        //        Debug.Log(other.name);
        //        Pylon pylon = other.GetComponent<Pylon>();
        //
        //        if (pylon)
        //            pylon.ShowPreview(false);
    }
}

public static class GuiHelper
{
    // The texture used by DrawLine(Color)
    private static Texture2D _coloredLineTexture;

    // The color used by DrawLine(Color)
    private static Color _coloredLineColor;

    /// <summary>
    /// Draw a line between two points with the specified color and a thickness of 1
    /// </summary>
    /// <param name="lineStart">The start of the line</param>
    /// <param name="lineEnd">The end of the line</param>
    /// <param name="color">The color of the line</param>
    public static void DrawLine(Vector2 lineStart, Vector2 lineEnd, Color color)
    {
        DrawLine(lineStart, lineEnd, color, 1);
    }

    /// <summary>
    /// Draw a line between two points with the specified color and thickness
    /// Inspired by code posted by Sylvan
    /// http://forum.unity3d.com/threads/17066-How-to-draw-a-GUI-2D-quot-line-quot?p=407005&viewfull=1#post407005
    /// </summary>
    /// <param name="lineStart">The start of the line</param>
    /// <param name="lineEnd">The end of the line</param>
    /// <param name="color">The color of the line</param>
    /// <param name="thickness">The thickness of the line</param>
    public static void DrawLine(Vector2 lineStart, Vector2 lineEnd, Color color, int thickness)
    {
        if (_coloredLineTexture == null || _coloredLineColor != color)
        {
            _coloredLineColor = color;
            _coloredLineTexture = new Texture2D(1, 1);
            _coloredLineTexture.SetPixel(0, 0, _coloredLineColor);
            _coloredLineTexture.wrapMode = TextureWrapMode.Repeat;
            _coloredLineTexture.Apply();
        }
        DrawLineStretched(lineStart, lineEnd, _coloredLineTexture, thickness);
    }

    /// <summary>
    /// Draw a line between two points with the specified texture and thickness.
    /// The texture will be stretched to fill the drawing rectangle.
    /// Inspired by code posted by Sylvan
    /// http://forum.unity3d.com/threads/17066-How-to-draw-a-GUI-2D-quot-line-quot?p=407005&viewfull=1#post407005
    /// </summary>
    /// <param name="lineStart">The start of the line</param>
    /// <param name="lineEnd">The end of the line</param>
    /// <param name="texture">The texture of the line</param>
    /// <param name="thickness">The thickness of the line</param>
    public static void DrawLineStretched(Vector2 lineStart, Vector2 lineEnd, Texture2D texture, int thickness)
    {
        Vector2 lineVector = lineEnd - lineStart;
        float angle = Mathf.Rad2Deg * Mathf.Atan(lineVector.y / lineVector.x);
        if (lineVector.x < 0)
        {
            angle += 180;
        }

        if (thickness < 1)
        {
            thickness = 1;
        }

        // The center of the line will always be at the center
        // regardless of the thickness.
        int thicknessOffset = (int)Mathf.Ceil(thickness / 2);

        GUIUtility.RotateAroundPivot(angle,
                                     lineStart);
        GUI.DrawTexture(new Rect(lineStart.x,
                                 lineStart.y - thicknessOffset,
                                 lineVector.magnitude,
                                 thickness),
                        texture);
        GUIUtility.RotateAroundPivot(-angle, lineStart);
    }

    /// <summary>
    /// Draw a line between two points with the specified texture and a thickness of 1
    /// The texture will be repeated to fill the drawing rectangle.
    /// </summary>
    /// <param name="lineStart">The start of the line</param>
    /// <param name="lineEnd">The end of the line</param>
    /// <param name="texture">The texture of the line</param>
    public static void DrawLine(Vector2 lineStart, Vector2 lineEnd, Texture2D texture)
    {
        DrawLine(lineStart, lineEnd, texture, 1);
    }

    /// <summary>
    /// Draw a line between two points with the specified texture and thickness.
    /// The texture will be repeated to fill the drawing rectangle.
    /// Inspired by code posted by Sylvan and ArenMook
    /// http://forum.unity3d.com/threads/17066-How-to-draw-a-GUI-2D-quot-line-quot?p=407005&viewfull=1#post407005
    /// http://forum.unity3d.com/threads/28247-Tile-texture-on-a-GUI?p=416986&viewfull=1#post416986
    /// </summary>
    /// <param name="lineStart">The start of the line</param>
    /// <param name="lineEnd">The end of the line</param>
    /// <param name="texture">The texture of the line</param>
    /// <param name="thickness">The thickness of the line</param>
    public static void DrawLine(Vector2 lineStart, Vector2 lineEnd, Texture2D texture, int thickness)
    {
        Vector2 lineVector = lineEnd - lineStart;
        float angle = Mathf.Rad2Deg * Mathf.Atan(lineVector.y / lineVector.x);
        if (lineVector.x < 0)
        {
            angle += 180;
        }

        if (thickness < 1)
        {
            thickness = 1;
        }

        // The center of the line will always be at the center
        // regardless of the thickness.
        int thicknessOffset = (int)Mathf.Ceil(thickness / 2);

        Rect drawingRect = new Rect(lineStart.x,
                                    lineStart.y - thicknessOffset,
                                    Vector2.Distance(lineStart, lineEnd),
                                    (float)thickness);
        GUIUtility.RotateAroundPivot(angle,
                                     lineStart);
        GUI.BeginGroup(drawingRect);
        {
            int drawingRectWidth = Mathf.RoundToInt(drawingRect.width);
            int drawingRectHeight = Mathf.RoundToInt(drawingRect.height);

            for (int y = 0; y < drawingRectHeight; y += texture.height)
            {
                for (int x = 0; x < drawingRectWidth; x += texture.width)
                {
                    GUI.DrawTexture(new Rect(x,
                                             y,
                                             texture.width,
                                             texture.height),
                                    texture);
                }
            }
        }
        GUI.EndGroup();
        GUIUtility.RotateAroundPivot(-angle, lineStart);
    }
}
