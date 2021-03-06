﻿using System;
using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
    [SerializeField] float radius = 30f;
    [SerializeField] private GameObject scannerModel;
    [SerializeField] private ParticleSystem scannerParticleSystem;

    private ScannerGadget scannerGadget;
    private Material material;
    private Renderer renderer;
    private SphereCollider sphereCollider;

    void Awake()
    {
        scannerGadget = FindObjectOfType<ScannerGadget>();
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = radius;
        sphereCollider.enabled = false;
        if(scannerGadget) transform.position = scannerGadget.transform.position;

    }
    
    void Start()
    {
        renderer = GameManager.Instance.Planet.scannableMesh.GetComponent<Renderer>();
        material = renderer.material;

        //Debug.Log(material.name);

        ShowTerrainScanner(true);
        scannerParticleSystem.startSize = radius*2;
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.RoundStarted || GameManager.Instance.Player.PlayerState != Player.PlayerStates.Normal)
            return;

        if (scannerGadget && scannerGadget.IsGrabbed)
            sphereCollider.enabled = true;
        else
            sphereCollider.enabled = false;
        
        HandleScanning();
    }

    public void ShowTerrainScanner(bool show)
    {
        if (show)
            material.SetFloat("_Radius", radius);
        else
            material.SetFloat("_Radius", 0);
    }

    private void HandleScanning()
    {
        Vector2 rayPos;

        if (GameManager.Instance.TouchInput)
        {
            if (Input.touchCount == 0)
                return;

            rayPos = Input.touches[0].position;
        }
        else
            rayPos = Input.mousePosition;
        
        Ray ray = Camera.main.ScreenPointToRay(rayPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (scannerGadget.IsGrabbed)
            {
                UpdateScannerPosition(hit.point);
                
                GeoThermalPlant plant = hit.transform.GetComponent<GeoThermalPlant>();
                if (plant && scannerGadget.IsGrabbed)
                {
                    if (plant.State == GeoThermalPlant.States.Ready)
                        ScanSucceeded(plant);
                }
            }
            else
            {
                //if clicked on screen, jump scanner there
                CheckScannerJump(GameManager.Instance.TouchInput, hit.point);

                UpdateScannerPosition(scannerGadget.transform.position);

                City city = hit.transform.GetComponent<City>();
                if (city)
                {
                    if (city.CityState == CityStates.DIRTY && Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began))
                        GameManager.Instance.TapTips.ShowRandomTip(city.transform);
                }
            }
        }
    }

    private void CheckScannerJump(bool touchInput, Vector3 point)
    {
        if (GameManager.Instance.Planet.IsSpinning)
            return;

        if (touchInput)
        {
            if (Input.touchCount != 1)
                return;
            if (Input.touches[0].phase != TouchPhase.Ended)
                return;

            UpdateScannerPosition(point);
            scannerGadget.transform.position = point;
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                UpdateScannerPosition(point);
//                scannerGadget.transform.position = point;
            }
        }
    }

    private void ScanSucceeded(GeoThermalPlant geoPlant)
    {
        scannerGadget.Release();
        geoPlant.Build();

        GameManager.Instance.Player.ScorePoints(5, geoPlant.transform);
        GameManager.Instance.Player.StartDrillMinigame(geoPlant, 1f);
    }

    private void UpdateScannerPosition(Vector3 position)
    {
        transform.position = position;

        Vector3 lookDir = position - GameManager.Instance.PlanetTransform.position;
        scannerModel.transform.LookAt(position + lookDir);
        
        material.SetVector("_CenterPoint", new Vector4(position.x, position.y, position.z, 0));
    }
    
    void OnTriggerEnter(Collider other)
    {
        GeoThermalPlant geoPlant = other.GetComponent<GeoThermalPlant>();

        if (geoPlant)
            geoPlant.ShowPreview(true);
    }

    void OnTriggerExit(Collider other)
    {
        GeoThermalPlant geoPlant = other.GetComponent<GeoThermalPlant>();

        if (geoPlant)
            geoPlant.ShowPreview(false);
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
