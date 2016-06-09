using System;
using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
    //temp
    public Texture2D selectedIcon;

    [SerializeField] private float maxCharge = 100f;
    [SerializeField] private float shrinkSpeed = 10f;
    [SerializeField] private GameObject gadgetModel;

    private Material material;
    private Renderer renderer;
    private SphereCollider sphereCollider;

    public City SelectedCity { get; private set; }
    public float Charge { get; private set; }
    public bool IsScanning { get; private set; }

    public float ChargeFraction
    {
        get { return Charge/maxCharge; }
    }

    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.enabled = false;
        gadgetModel.SetActive(false);
    }
    
    void Start()
    {
        renderer = GameManager.Instance.Planet.scannableMesh.GetComponent<Renderer>();
        material = renderer.material;
    }

    void Update()
    {
        if (!GameManager.Instance.RoundStarted)
            return;

        if (Charge > 0)
            Charge -= Time.deltaTime * shrinkSpeed;

        CheckStartStop();

//        Debug.Log(IsScanning + ", " + Charge);

        HandleScanning();

        material.SetFloat("_Radius", Charge);
    }

    void OnGUI()
    {
        if (SelectedCity)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(SelectedCity.transform.position);

            GUI.Label(GameManager.Instance.Hud.CenteredRect(new Rect(pos.x, pos.y, 80, 80)), selectedIcon);
        }
    }

    public void AddCharge(float amount)
    {
        Charge = Mathf.Min(Charge + amount, maxCharge);
    }

    private void CheckStartStop()
    {
        if (IsScanning)
        {
            if (Charge <= 0f)
            {
                EndScan();
            }
            else if (GameManager.Instance.TouchInput && Input.touchCount == 0)
                EndScan();
            else if (!GameManager.Instance.TouchInput && !Input.GetMouseButton(0))
                EndScan();
        }
        else
        {
            if (GameManager.Instance.TouchInput)
            {

            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        UpdateScannerPosition(hit.point);

                        City city = hit.transform.GetComponent<City>();
                        if (city)
                        {
                            SelectedCity = city;
                            StartScan();
                        }
                    }
                }
            }
        }
    }

    private void StartScan()
    {
        IsScanning = true;
        sphereCollider.enabled = true;
        gadgetModel.SetActive(true);
        Charge = maxCharge;

        GameManager.Instance.Director.SetMode(Director.Modes.Grid, SelectedCity.transform);
    }

    private void EndScan()
    {
        IsScanning = false;
        sphereCollider.enabled = false;
        gadgetModel.SetActive(false);
        Charge = 0f;
        SelectedCity.Reset();

        GameManager.Instance.Director.SetMode(Director.Modes.Orbit, GameManager.Instance.PlanetTransform);
    }

    private void HandleScanning()
    {
        gadgetModel.transform.localScale = new Vector3(Charge, Charge, Charge);
        sphereCollider.radius = Charge;

        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                UpdateScannerPosition(hit.point);
              
                Pylon pylon = hit.transform.GetComponent<Pylon>();
                if (pylon)
                {
                    if (SelectedCity != null)
                    {
                        SelectedCity.TryBuild(pylon);
                    }
                }
            }
        }
    }

    private void UpdateScannerPosition(Vector3 position)
    {
        transform.position = position;
        material.SetVector("_CenterPoint", new Vector4(position.x, position.y, position.z, 0));
    }

    private bool CheckCancelScan()
    {
        if (GameManager.Instance.TouchInput)
        {
            if (Input.touches.Length != 2)
                return true;
            return false;
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
                return true;
            return false;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
//        Debug.Log(other.name);
        Pylon pylon = other.GetComponent<Pylon>();
        
        if (pylon)
            pylon.ShowPreview(true);
    }

    void OnTriggerExit(Collider other)
    {
//        Debug.Log(other.name);
        Pylon pylon = other.GetComponent<Pylon>();

        if (pylon)
            pylon.ShowPreview(false);
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
