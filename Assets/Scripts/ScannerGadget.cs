using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ScannerGadget : MonoBehaviour
{
    [SerializeField] private GameObject maleModel;
    [SerializeField] private GameObject femaleModel;
    [SerializeField] private Transform spinningRadar;
    [SerializeField] private float spinRate = 355f;
    [SerializeField] private float tipDelay = 3f;
    [Range(0f, 1f)]
    [SerializeField] private float helperHealthLimit;

    private Scanner scanner;
    private Collider myCollider;
    private GameObject activeModel;
    private Vector2 start;
    private Vector2 end;
    private int arrowTweenId;
    private float arrowTweenValue;

    public bool IsGrabbed { get; private set; }
    public float LastInteractionAt { get; private set; }

    void Awake()
    {
        scanner = FindObjectOfType<Scanner>();
        myCollider = GetComponent<Collider>();
        activeModel = maleModel;
    }

    void Start()
    {
        LastInteractionAt = Time.time;
        FixRotation();
        //un-comment to move with planet
//        transform.SetParent(GameManager.Instance.PlanetTransform);
    }

    void Update()
    {
        if (!IsGrabbed)
        {
            spinningRadar.Rotate(0, spinRate * Time.deltaTime, 0);

//            Debug.Log(Time.time - LastInteractionAt - tipDelay);
            if (Time.time >= LastInteractionAt + tipDelay)
                GameManager.Instance.Hud.ShowScannerTip(true);
            else
                GameManager.Instance.Hud.ShowScannerTip(false);
        }
        else if (GameManager.Instance.Planet.Health < helperHealthLimit)
        {
            Vector2 fixedEnd = new Vector2(end.x, Screen.height - end.y) - new Vector2(start.x, Screen.height - start.y);
            
            GameManager.Instance.Hud.PointBuildArrow(Vector2.Lerp(start, end, arrowTweenValue), fixedEnd);
        }
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.Player.PlayerState != Player.PlayerStates.Normal)
            return;

        if (!IsGrabbed)
            CheckGrab();
        else
        {
            CheckRelease();
        }
    }

    void OnGUI()
    {
        if (IsGrabbed && GameManager.Instance.Planet.Health < helperHealthLimit)
        {
            start = Camera.main.WorldToScreenPoint(scanner.transform.position);
            end = Camera.main.WorldToScreenPoint(FindClosestGeoPlant().transform.position);

            start.y = Screen.height - start.y;
            end.y = Screen.height - end.y;
            
            GuiHelper.DrawLine(end, start, Color.green, 2);
        }
    }

    public void SetGender(bool isMale)
    {
        if (isMale)
        {
            maleModel.SetActive(true);
            femaleModel.SetActive(false);
            activeModel = maleModel;
        }
        else
        {
            maleModel.SetActive(false);
            femaleModel.SetActive(true);
            activeModel = femaleModel;
        }

        spinningRadar.SetParent(activeModel.transform);
    }

    private void CheckGrab()
    {
        if (GameManager.Instance.TouchInput && Input.touchCount == 0)
            return;
        if (!GameManager.Instance.TouchInput && !Input.GetMouseButton(0))
            return;

        Vector2 rayPos;

        if (GameManager.Instance.TouchInput)
        {
            rayPos = Input.touches[0].position;
        }
        else
            rayPos = Input.mousePosition;

        
        Ray ray = Camera.main.ScreenPointToRay(rayPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject == gameObject)
            {
                Grab();
            }
        }
    }

    private void CheckRelease()
    {
        if (GameManager.Instance.TouchInput && Input.touchCount == 0)
            Release();
        if (!GameManager.Instance.TouchInput && !Input.GetMouseButton(0))
            Release();
    }

    private void Grab()
    {
        IsGrabbed = true;
        LastInteractionAt = Time.time;

        //only show in the beginning
        if (GameManager.Instance.Planet.Health < helperHealthLimit)
        {
            FindClosestGeoPlant().ShowPreview(true);
            GameManager.Instance.Hud.ShowBuildArrow(true);

            arrowTweenId = LeanTween.value(gameObject, UpdateArrowCallback, 0f, 1f, 1f).setLoopClamp().setEase(LeanTweenType.easeOutSine).id;
        }
        
        GameManager.Instance.Hud.ShowScannerTip(false);
        myCollider.enabled = false;
        activeModel.SetActive(false);
//        scanner.gameObject.SetActive(false);
    }

    public void Release()
    {
        if (LeanTween.isTweening(arrowTweenId))
            LeanTween.cancel(arrowTweenId);

        IsGrabbed = false;
        LastInteractionAt = Time.time;
        GameManager.Instance.HideAllGeoPlantPreviews();
        GameManager.Instance.Hud.ShowBuildArrow(false);
        myCollider.enabled = true;
        transform.position = scanner.transform.position;
        FixRotation();
        activeModel.SetActive(true);
//        scanner.gameObject.SetActive(true);
    }

    private GeoThermalPlant FindClosestGeoPlant()
    {
        GeoThermalPlant closestPlant = null;
        float closestDistance = 99999f;

        foreach (City city in GameManager.Instance.Cities)
        {
            if (city.CityState == CityStates.DIRTY)
            {
                float dist = Vector3.Distance(city.PuzzlePath.GeoPlant.transform.position, scanner.transform.position);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestPlant = city.PuzzlePath.GeoPlant;
                }
            }
        }

        return closestPlant;
    }

    private void FixRotation()
    {
        transform.LookAt(transform.position + activeModel.transform.position - GameManager.Instance.PlanetTransform.position);
    }

    void UpdateArrowCallback(float val)
    {
        arrowTweenValue = val;
    }
}
