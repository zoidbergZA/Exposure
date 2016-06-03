using System;
using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
    public delegate void ScanHandler();
    
    public static event ScanHandler ScanStarted;
    public static event ScanHandler HotspotFound;

    public float maxRadius = 100f;
    public float limiter = 100f;

    [SerializeField] private float maxScanDistance = 250f;
    [SerializeField] private float minScanDistance = 150f;
    [SerializeField] private float focusTime = 2f;
    [SerializeField] private Texture2D touchIcon;
    [SerializeField] private Texture2D goodIcon;
    [SerializeField] private Texture2D progresIcon;
    [SerializeField] private Texture2D centerIcon;

    private Material material;
    private Renderer renderer;
    private float radius;
    private bool isOnHotspot;

    private Vector3 startPoint;
    private Vector3 endPoint;

    private Vector3 center
    {
        get
        {
            Vector3 scanpoint = startPoint + ((endPoint - startPoint)/2f);
            Vector3 center = new Vector3(Screen.width/2f, Screen.height/2f);
            Vector3 output = Vector3.ClampMagnitude(scanpoint - center, limiter) + center;

            return output;
        }
    }
    private Vector3 forwardDirection;
    private float lastStartScanAt;
    private float focusTimer;

    public bool IsScanning { get; private set; }

    public float Progress
    {
        get { return 1f - focusTimer/focusTime; }
    }
    
    void Start()
    {
        renderer = GameManager.Instance.Planet.scannableMesh.GetComponent<Renderer>();
        material = renderer.material;
    }

    void Update()
    {
        if (GameManager.Instance.Player.PlayerState == Player.PlayerStates.Normal && !IsScanning)
        {
            CheckStartScan();
        }

        if (IsScanning)
            HandleScanning();

        material.SetFloat("_Radius", radius);
    }

    void OnGUI()
    {
        if (IsScanning)
        {
            //draw scanning debug
            GUI.Label(new Rect(startPoint.x - 25f, Screen.height - startPoint.y - 25f, 50f, 50f), touchIcon);
            GUI.Label(new Rect(endPoint.x - 25f, Screen.height - endPoint.y - 25f, 50f, 50f), touchIcon);
            GUI.Label(GameManager.Instance.Hud.CenteredRect(new Rect(center.x, center.y, 270f, 270f)), centerIcon);
            
            string progress = "";

            if (isOnHotspot)
            {
                GUI.Label(GameManager.Instance.Hud.CenteredRect(new Rect(center.x, center.y, 270f, 270f)), goodIcon);

//                progress = ((Progress*100f)).ToString("F0");

                float size = (1f -Progress) * 200f;
                GUI.Label(GameManager.Instance.Hud.CenteredRect(new Rect(center.x, center.y, size, size)), progresIcon);
            }
//            GUI.Label(new Rect(center.x, Screen.height - center.y + 80f, 50f, 50f), progress + "%");
            }
    }

    public void StartScan()
    {
        if (IsScanning)
            return;

        IsScanning = true;
        lastStartScanAt = Time.time;
        focusTimer = focusTime;

        if (ScanStarted != null)
        {
            ScanStarted();
        }
    }

    public void EndScan()
    {
        IsScanning = false;
        radius = 0;
        isOnHotspot = false;
    }

    private void DrawTouchInfo(Touch touch)
    {
        GUI.BeginGroup(new Rect(touch.position.x, Screen.height - touch.position.y, 200, 125), "", "box");

        GUI.Label(new Rect(5, 0, 200, 25), "finger id: " + touch.fingerId);
        GUI.Label(new Rect(5, 25, 200, 25), "phase: " + touch.phase);
        GUI.Label(new Rect(5, 50, 200, 25), "tap count: " + touch.tapCount);
        GUI.Label(new Rect(5, 75, 200, 25), "position: " + touch.position);
        GUI.Label(new Rect(5, 100, 200, 25), "delta: " + touch.deltaPosition);

        GUI.EndGroup();
    }

    private void CheckStartScan()
    {
        if (GameManager.Instance.TouchInput)
        {
            if (Input.touches.Length == 2)
            {
                startPoint = Input.touches[0].position;
                endPoint = Input.touches[1].position;

                StartScan();
            }
        }
        else
        {
            // right click updates start point
            if (Input.GetMouseButtonDown(1))
            {
                startPoint = Input.mousePosition;
            }

            // check if start scanning conditions are met
            if (!IsScanning && Input.GetMouseButtonDown(0))
            {
                endPoint = Input.mousePosition;
                StartScan();
            }
        }
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

    private void HandleScanning()
    {
        if (CheckCancelScan())
        {
            EndScan();
            return;
        }
        
        //update scan points
        {
            if (GameManager.Instance.TouchInput)
            {
                if (Input.touches.Length == 2)
                {
                    startPoint = Input.touches[0].position;
                    endPoint = Input.touches[1].position;
                }
            }
            else
            {
                endPoint = Input.mousePosition;
            }
        }
        
        Ray ray = Camera.main.ScreenPointToRay(center);
        RaycastHit hit;

        float sample = 0f;

        if (Physics.Raycast(ray, out hit))
        {
            float scanDelta = Vector3.Distance(hit.point, Camera.main.transform.position);
            radius = Mathf.Clamp(scanDelta * 0.161f, 0f, maxRadius);

            sample = GameManager.Instance.SampleHeatmap(hit.textureCoord).r;

            if (sample >= 0.2f)
                isOnHotspot = true;
            else
                isOnHotspot = false;

            material.SetVector("_CenterPoint", new Vector4(hit.point.x, hit.point.y, hit.point.z, 0));

            if (isOnHotspot && Progress >= 0.99f && Time.time >= lastStartScanAt + 2f)
            {
                if (HotspotFound != null)
                    HotspotFound();

                ScanSucceeded(sample, hit.point, hit.normal);
            }
        }

        if (sample >= 0.2f)
            focusTimer -= Time.deltaTime;
        else
            focusTimer = focusTime;
    }

    private void ScanSucceeded(float sample, Vector3 location, Vector3 normal)
    {
        radius = 0;
        IsScanning = false;
        GameManager.Instance.Player.Drill(location, normal, 1f - sample);

        //        Ray ray = Camera.main.ScreenPointToRay(center);
        //        RaycastHit hit;
        //
        //        if (Physics.Raycast(ray, out hit))
        //        {
        ////            Color sample = GameManager.Instance.SampleHeatmap(hit.textureCoord);
        //            GameManager.Instance.Player.Drill(location, normal, 1f - sample);
        //        }
        //        else
        //        {
        //            Debug.Log("oops, couldn't drill :/");   
        //        }
    }

//    void radiusTweenCallback(float val, float ratio)
//    {
//        radius = val;
//
//        Vector2 pos = GameManager.Instance.DrillingGame.GlobeDrillPipeIcon.rectTransform.anchoredPosition;
//
//        pos.y = (emptyY - fullY) * ratio;
//
//        GameManager.Instance.DrillingGame.GlobeDrillPipeIcon.rectTransform.anchoredPosition = pos;
//    }
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
