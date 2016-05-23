using System;
using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
    public ScanProperties smallScan;
    public ScanProperties globalScan;

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

<<<<<<< HEAD
        emptyY = GameManager.Instance.DrillingGame.StartInnerToast.rectTransform.anchoredPosition.y;
        fullY = GameManager.Instance.DrillingGame.StartInnerToast.rectTransform.anchoredPosition.y +
            GameManager.Instance.DrillingGame.StartInnerToast.rectTransform.rect.height;

        activateImages(false);
=======
        emptyY = GameManager.Instance.DrillingGame.GlobeDrillPipeIcon.rectTransform.anchoredPosition.y;
        fullY = GameManager.Instance.DrillingGame.GlobeDrillPipeIcon.rectTransform.anchoredPosition.y +
            GameManager.Instance.DrillingGame.GlobeDrillPipeIcon.rectTransform.rect.height; 
>>>>>>> refs/remotes/origin/master
    }

    void Update()
    {
        if (GameManager.Instance.Player.PlayerState == Player.PlayerStates.Normal)
        {
            CheckStartScan();
        }

        if (IsScanning)
            HandleScanning();

        //set center point to raycayt through center
//        material.SetVector("_CenterPoint", new Vector4(center.x, center.y, center.z, 0));

//        cooldownLeft -= Time.deltaTime;

//        if (cooldownLeft <= 0)
//        {
//            cooldownLeft = cooldownTime;
////            StartScan(globalScan);    //todo: global scans removed for testing local
//        }

        material.SetFloat("_Radius", radius);
    }

    void OnGUI()
    {
        if (IsScanning)
        {
            //draw scanning debug
            GUI.Label(new Rect(startPoint.x, Screen.height - startPoint.y, 30f, 30f), touchIcon);
            GUI.Label(new Rect(endPoint.x, Screen.height - endPoint.y, 30f, 30f), touchIcon);
            GUI.Label(new Rect(center.x, Screen.height - center.y, 30f, 30f), centerIcon);

            //max distance etension icon
            Vector3 scanlineDirection = (endPoint - startPoint).normalized;
            Vector3 maxPoint = startPoint + scanlineDirection * maxScanDistance;
            GUI.Label(new Rect(maxPoint.x, Screen.height - maxPoint.y, 30f, 30f), maxDistanceIcon);
        }

        if (GameManager.Instance.TouchInput && Input.touches.Length > 0)
        {
            foreach (Touch t in Input.touches)
            {
                DrawTouchInfo(t);
            }
        }
    }

//    public void StartScan(ScanProperties scanProperties)
//    {
//        IsScanning = true;
//
//        //direction to center point
//        Vector3 dir = (GameManager.Instance.PlanetTransform.position - Camera.main.transform.position).normalized;
//
//        RaycastHit hit;
//
//        if (Physics.Raycast(Camera.main.transform.position, dir, out hit))
//        {
//            centerPoint = hit.point;
//        }
//
//        LeanTween.value(gameObject, radiusTweenCallback, 5f, scanProperties.range, scanProperties.duration)
//            .setEase(scanProperties.tweenType)
//            .setOnComplete(EndScan);
//    }

    public void StartScan()
    {
        if (IsScanning)
            return;

        IsScanning = true;
        lastStartScanAt = Time.time;

//        smallScanId = LeanTween.value(gameObject, radiusTweenCallback, 9f, smallScan.range, smallScan.duration)
//            .setEase(smallScan.tweenType)
//            .setOnComplete(SmallScanCompleted)
//            .id;
    }

    public void EndScan()
    {
        IsScanning = false;
        
//        if (LeanTween.isTweening(smallScanId))
//        {
//            LeanTween.cancel(smallScanId);
//        }

        radius = 0;
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

//        if (Input.GetMouseButton(0))
//        {
//            if (Input.GetMouseButtonDown(0))
//            {
//                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                
//                if (Physics.Raycast(ray, out hit, scanRayMask))
//                {
////                    Debug.Log(hit.transform.name + " , " + Time.time);
////                    Debug.DrawLine(Camera.main.transform.position, hit.point);
//
//                    GameManager.Instance.Director.OrbitPaused = true;
//                    GameManager.Instance.DrillingGame.PressureIcon.rectTransform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 140, Input.mousePosition.z);
//
//                    StartScan(hit.point);
//                    activateImages(true);
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
//                }
//            }
//        }
//        else
//        {
//            if (GameManager.Instance.Director.OrbitPaused) GameManager.Instance.Director.OrbitPaused = false;
////            toastMessageShown = false;
////            drillToastTimer = drillToastTime;
////            drilled = false;
//            activateImages(false);
////            GameManager.Instance.DrillingGame.StartInnerToast.rectTransform.anchoredPosition = initPressureImagePos;
////            GameManager.Instance.DrillingGame.BgActive.rectTransform.anchoredPosition = initBgImagePos;
//
//            EndScan();
//        }
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
<<<<<<< HEAD
            if (Input.GetMouseButtonUp(0))
                return true;
            return false;
=======
            if (GameManager.Instance.Director.OrbitPaused) GameManager.Instance.Director.OrbitPaused = false;
//            toastMessageShown = false;
//            drillToastTimer = drillToastTime;
//            drilled = false;
            activateImages(false);
//            GameManager.Instance.DrillingGame.GlobeDrillPipeIcon.rectTransform.anchoredPosition = initPressureImagePos;
//            GameManager.Instance.DrillingGame.BgActive.rectTransform.anchoredPosition = initBgImagePos;

            EndScan();
>>>>>>> refs/remotes/origin/master
        }
    }

    private void HandleScanning()
    {
        if (CheckCancelScan())
        {
            Debug.Log("scan cancelled");
            IsScanning = false;
            radius = 0f;
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
        if (scanDelta <= 40f && Time.time >= lastStartScanAt + 1.0f)
        {
            Debug.Log("scan succeeded");
            ScanSucceeded();
            return;   
        }

//        radius = scanDelta * 0.15f;
//        radius = Mathf.Max(radius, 21f);
        radius = Mathf.Clamp(scanDelta*0.15f, 21f, 40f);

        Ray ray = Camera.main.ScreenPointToRay(center);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, scanRayMask))
        {
            material.SetVector("_CenterPoint", new Vector4(hit.point.x, hit.point.y, hit.point.z, 0));
        }

        
        //        Debug.Log("scanning " + startPoint + " , " + endPoint + " , " + Vector3.Distance(startPoint, endPoint));
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

    private void SmallScanCompleted()
    {
//        Debug.Log("hello : " + Time.time);

//        EndScan();
//
//        if (Physics.Raycast(ray, out hit, scanRayMask))
//        {
//            Color sample = GameManager.Instance.SampleHeatmap(hit.textureCoord);
//            GameManager.Instance.Player.Drill(hit.point, hit.normal, 1f - sample.r);
////            drilled = true;
//            activateImages(false);
//        }
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
