using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ScannerGadget : MonoBehaviour
{
    [SerializeField] private GameObject model;
    [SerializeField] private Transform spinningRadar;
    [SerializeField] private float spinRate = 355f;
    [SerializeField] private float tipDelay = 3f;

    private Scanner scanner;
    private Collider myCollider;

    public bool IsGrabbed { get; private set; }
    public float LastInteractionAt { get; private set; }

    void Awake()
    {
        scanner = FindObjectOfType<Scanner>();
        myCollider = GetComponent<Collider>();
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
        GameManager.Instance.Hud.ShowScannerTip(false);
        myCollider.enabled = false;
        model.SetActive(false);
//        GameManager.Instance.Director.SetSunlightBrightness(0.3f);
    }

    private void Release()
    {
        IsGrabbed = false;
        LastInteractionAt = Time.time;
        myCollider.enabled = true;
        transform.position = scanner.transform.position;
        FixRotation();
        model.SetActive(true);
//        GameManager.Instance.Director.SetSunlightBrightness(1f);
    }

    private void FixRotation()
    {
        transform.LookAt(transform.position + model.transform.position - GameManager.Instance.PlanetTransform.position);
    }
}
