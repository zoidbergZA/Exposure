using System;
using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
//    public ScanProperties smallScan;
//    public ScanProperties globalScan;

    //    [SerializeField] private float cooldownTime = 7f;
    [SerializeField] private float maxScanDistance = 250f;
    [SerializeField] private Texture2D touchIcon;
    [SerializeField] private Texture2D centerIcon;
//    [SerializeField] private Texture2D maxDistanceIcon;
    [SerializeField] private LayerMask scanRayMask;

//    private float cooldownLeft;
//    private Vector3 centerPoint;
    private Material material;
    private Renderer renderer;
    private float radius;
//    private int smallScanId;
//    private Ray ray;
//    private RaycastHit hit;
    private float emptyY;
    private float fullY;

    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 center { get { return startPoint + ((endPoint - startPoint) / 2f); } }
    private Vector3 forwardDirection;
    private float lastStartScanAt;

    public bool IsScanning { get; private set; }
//    public float ScanProgress { get { return 1f - durationLeft/durationTime; } }
//    public float Cooldown { get { return cooldownLeft; } }
    
    void Awake()
    {
//        cooldownLeft = cooldownTime;
    }

    void Start()
    {
        renderer = GameManager.Instance.Planet.scannableMesh.GetComponent<Renderer>();
        material = renderer.material;

        emptyY = GameManager.Instance.DrillingGame.GlobeDrillPipeIcon.rectTransform.anchoredPosition.y;
        fullY = GameManager.Instance.DrillingGame.GlobeDrillPipeIcon.rectTransform.anchoredPosition.y +
            GameManager.Instance.DrillingGame.GlobeDrillPipeIcon.rectTransform.rect.height;
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
            GUI.Label(new Rect(center.x - 25f, Screen.height - center.y - 25f, 50f, 50f), centerIcon);

            GuiHelper.DrawLine(new Vector2(startPoint.x, Screen.height - startPoint.y), new Vector2(endPoint.x, Screen.height - endPoint.y), Color.grey, 1);

            //            //max distance etension icon
            //            Vector3 scanlineDirection = (endPoint - startPoint).normalized;
            //            Vector3 maxPoint = startPoint + scanlineDirection * maxScanDistance;
            //            GUI.Label(new Rect(maxPoint.x - 25f, Screen.height - maxPoint.y - 25f, 50f, 50f), maxDistanceIcon);
        }

//        if (GameManager.Instance.TouchInput && Input.touches.Length > 0)
//        {
//            foreach (Touch t in Input.touches)
//            {
//                DrawTouchInfo(t);
//            }
//        }
    }

    public void StartScan()
    {
        if (IsScanning)
            return;

        IsScanning = true;
        lastStartScanAt = Time.time;
        GameManager.Instance.Director.OrbitPaused = true;
    }

    public void EndScan()
    {
        IsScanning = false;
        radius = 0;
        GameManager.Instance.Director.OrbitPaused = false;
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
            Debug.Log("scan cancelled");
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

            //todo: get forward direction to distort shader circe
        }

        float scanDelta = Vector3.Distance(startPoint, endPoint);
        
        //check scan succeeded
        if (scanDelta <= 220f && Time.time >= lastStartScanAt + 2.0f)
        {
            Debug.Log("scan succeeded");
            ScanSucceeded();
            return;   
        }
        
        radius = Mathf.Clamp(scanDelta*0.12f, 17f, 26f);

        Ray ray = Camera.main.ScreenPointToRay(center);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, scanRayMask))
        {
            material.SetVector("_CenterPoint", new Vector4(hit.point.x, hit.point.y, hit.point.z, 0));
        }
    }

    private void ScanSucceeded()
    {
        radius = 0;
        IsScanning = false;

        Ray ray = Camera.main.ScreenPointToRay(center);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, scanRayMask))
        {
            Color sample = GameManager.Instance.SampleHeatmap(hit.textureCoord);
            GameManager.Instance.Player.Drill(hit.point, hit.normal, 1f - sample.r);
        }
        else
        {
            Debug.Log("oops, couldn't drill :/");   
        }
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

    private void activateImages(bool activate)
    {
        GameManager.Instance.DrillingGame.GlobeDrillGroundIcon.gameObject.SetActive(activate);
        GameManager.Instance.DrillingGame.GlobeDrillPipeIcon.gameObject.SetActive(activate);
        //GameManager.Instance.DrillingGame.BgActive.gameObject.SetActive(activate);
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
