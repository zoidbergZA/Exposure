using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public enum PlayerStates
    {
        Drill,
        BuildGrid
    }

    [SerializeField] private float gridTime = 7f;
    [SerializeField] private GameObject radar;
    [SerializeField] private float scanTime;
    [SerializeField] private LayerMask rayMask;
    [SerializeField] private Powerplant PowerplantPrefab;
    [SerializeField] private Drillspot DrillspotPrefab;

    public PlayerStates PlayerState { get; private set; }
    public float Score { get; private set; }
    public bool Scanning { get; private set; }
    public float LastScan { get; private set; }

    private float gridTimeLeft;

    void Update()
    {
        if (Scanning && Time.time > LastScan + scanTime)
            Scanning = false;

        switch (PlayerState)
        {
            case PlayerStates.Drill:
                HandleDrillState();
                break;
            case PlayerStates.BuildGrid:
                HandleBuildGridState();
                break;
        }
    }

    public void GoToState(PlayerStates state, Transform targetTransform)
    {
        PlayerState = state;

        switch (PlayerState)
        {
            case PlayerStates.BuildGrid:
                gridTimeLeft = gridTime;
                GameManager.Instance.Director.SetMode(Director.Modes.Grid, targetTransform);
                break;
            case PlayerStates.Drill:
                GameManager.Instance.Director.SetMode(Director.Modes.Orbit, targetTransform);
                break;
        }
    }

    public void ScorePoints(float amount)
    {
        Score += amount;
    }

    private void HandleDrillState()
    {
        if (Input.GetMouseButtonDown(1) && !Scanning)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayMask))
            {
                Scan(hit.point);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayMask))
            {
                Drill(hit.point, hit.normal);
            }
        }

        if (Scanning)
        {
            radar.SetActive(true);
        }
        else
        {
            radar.SetActive(false);
        }
    }

    private void HandleBuildGridState()
    {
        if (gridTimeLeft <= 0)
        {
            GoToState(PlayerStates.Drill, GameManager.Instance.PlanetTransform);
            return;
        }

        gridTimeLeft -= Time.deltaTime;
    }

    private void Drill(Vector3 location, Vector3 normal)
    {
        Drillspot drillspot = Instantiate(DrillspotPrefab, location, Quaternion.identity) as Drillspot;
        drillspot.Orientate(normal);
    }

    private void Scan(Vector3 location)
    {
        LastScan = Time.time;
        Scanning = true;
        radar.transform.position = location;
    }
}
