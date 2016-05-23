using System;
using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
    public ScanProperties smallScan;
    public ScanProperties globalScan;
     
    [SerializeField] private float cooldownTime = 7f;
    [SerializeField] private LayerMask scanRayMask;

    private float cooldownLeft;
    private Vector3 centerPoint;
    private Material material;
    private Renderer renderer;
    private float radius;
    private int smallScanId;
    private Ray ray;
    private RaycastHit hit;
    private float emptyY;
    private float fullY;

    public bool IsScanning { get; private set; }
//    public float ScanProgress { get { return 1f - durationLeft/durationTime; } }
    public float Cooldown { get { return cooldownLeft; } }
    
    void Awake()
    {
        cooldownLeft = cooldownTime;
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
            HandleScanner();    
        }

        material.SetVector("_CenterPoint", new Vector4(centerPoint.x, centerPoint.y, centerPoint.z, 0));

        cooldownLeft -= Time.deltaTime;

        if (cooldownLeft <= 0)
        {
            cooldownLeft = cooldownTime;
//            StartScan(globalScan);    //todo: global scans removed for testing local
        }

        material.SetFloat("_Radius", radius);
    }

    public void StartScan(ScanProperties scanProperties)
    {
        IsScanning = true;

        Vector3 dir = (GameManager.Instance.PlanetTransform.position - Camera.main.transform.position).normalized;

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, dir, out hit))
        {
            centerPoint = hit.point;
        }

        LeanTween.value(gameObject, radiusTweenCallback, 5f, scanProperties.range, scanProperties.duration)
            .setEase(scanProperties.tweenType)
            .setOnComplete(EndScan);
    }

    public void StartScan(Vector3 point)
    {
        IsScanning = true;

        centerPoint = point;

        smallScanId = LeanTween.value(gameObject, radiusTweenCallback, 5f, smallScan.range, smallScan.duration)
            .setEase(smallScan.tweenType)
            .setOnComplete(SmallScanCompleted)
            .id;
    }

    public void EndScan()
    {
        IsScanning = false;

        //todo: if tweening, cancel tween, call EndScan() when drilling game preloader cancels
        if (LeanTween.isTweening(smallScanId))
        {
            LeanTween.cancel(smallScanId);
        }

        radius = 0;
    }

    private void HandleScanner()
    {
        if (Input.GetMouseButton(0))
        {
            if (Input.GetMouseButtonDown(0))
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                
                if (Physics.Raycast(ray, out hit, scanRayMask))
                {
//                    Debug.Log(hit.transform.name + " , " + Time.time);
//                    Debug.DrawLine(Camera.main.transform.position, hit.point);

                    GameManager.Instance.Director.OrbitPaused = true;
                    GameManager.Instance.DrillingGame.PressureIcon.rectTransform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 140, Input.mousePosition.z);

                    StartScan(hit.point);
                    activateImages(true);
//                    moveUImages();

//                    drillToastTimer -= Time.deltaTime;
//                    if (drillToastTimer <= 0 && !drilled) toastMessageShown = true;
//                    if (toastMessageShown)
//                    {
//                        if (Physics.Raycast(ray, out hit, drillRayMask))
//                        {
//                            Color sample = GameManager.Instance.SampleHeatmap(hit.textureCoord);
//                            Drill(hit.point, hit.normal, 1f - sample.r);
//                            drilled = true;
//                            activateImages(false);
//                        }
//                        toastMessageShown = false;
//                    }
                }
            }
        }
        else
        {
            if (GameManager.Instance.Director.OrbitPaused) GameManager.Instance.Director.OrbitPaused = false;
//            toastMessageShown = false;
//            drillToastTimer = drillToastTime;
//            drilled = false;
            activateImages(false);
//            GameManager.Instance.DrillingGame.GlobeDrillPipeIcon.rectTransform.anchoredPosition = initPressureImagePos;
//            GameManager.Instance.DrillingGame.BgActive.rectTransform.anchoredPosition = initBgImagePos;

            EndScan();
        }
    }

    private void SmallScanCompleted()
    {
//        Debug.Log("hello : " + Time.time);

        EndScan();

        if (Physics.Raycast(ray, out hit, scanRayMask))
        {
            Color sample = GameManager.Instance.SampleHeatmap(hit.textureCoord);
            GameManager.Instance.Player.Drill(hit.point, hit.normal, 1f - sample.r);
//            drilled = true;
            activateImages(false);
        }
    }

    void radiusTweenCallback(float val, float ratio)
    {
        radius = val;

        Vector2 pos = GameManager.Instance.DrillingGame.GlobeDrillPipeIcon.rectTransform.anchoredPosition;

        pos.y = (emptyY - fullY) * ratio;

        GameManager.Instance.DrillingGame.GlobeDrillPipeIcon.rectTransform.anchoredPosition = pos;
    }

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
