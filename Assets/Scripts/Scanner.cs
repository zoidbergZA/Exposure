﻿using System;
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
    [SerializeField] private Texture2D maxDistanceIcon;
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
        if (GameManager.Instance.Player.PlayerState == Player.PlayerStates.Normal)
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

            //max distance etension icon
            Vector3 scanlineDirection = (endPoint - startPoint).normalized;
            Vector3 maxPoint = startPoint + scanlineDirection * maxScanDistance;
            GUI.Label(new Rect(maxPoint.x - 25f, Screen.height - maxPoint.y - 25f, 50f, 50f), maxDistanceIcon);
        }

        if (GameManager.Instance.TouchInput && Input.touches.Length > 0)
        {
            foreach (Touch t in Input.touches)
            {
                DrawTouchInfo(t);
            }
        }
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
            //todo: handle touch input check start
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
            // todo: check end scan for touch input
            return true;
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
                //todo: update touch input points
            }
            else
            {
                endPoint = Input.mousePosition;
            }

            //todo: get forward direction to distort shader circe
        }

        float scanDelta = Vector3.Distance(startPoint, endPoint);
        
        //check scan succeeded
        if (scanDelta <= 40f && Time.time >= lastStartScanAt + 2.0f)
        {
            Debug.Log("scan succeeded");
            ScanSucceeded();
            return;   
        }

        radius = Mathf.Clamp(scanDelta*0.15f, 21f, 40f);

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

[Serializable]
public class ScanProperties
{
    public float duration;
    public float range;
    public float fade;
    public LeanTweenType tweenType;
}
