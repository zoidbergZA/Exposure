using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject radar;
    [SerializeField] private float scanTime;
    [SerializeField] private LayerMask rayMask;
    [SerializeField] private Powerplant PowerplantPrefab;

    public bool Scanning { get; private set; }
    public float LastScan { get; private set; }

    void Update()
    {
        if (Scanning && Time.time > LastScan + scanTime)
            Scanning = false;

        if (Input.GetButtonDown("Fire1") && !Scanning)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayMask))
            {
                Scan(hit.point);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayMask))
            {
                BuildPowerplant(hit.point, hit.normal);
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

    private void BuildPowerplant(Vector3 location, Vector3 normal)
    {
        Powerplant powerplant = Instantiate(PowerplantPrefab, location, Quaternion.identity) as Powerplant;
        powerplant.Orientate(normal);
    }

    private void Scan(Vector3 location)
    {
        LastScan = Time.time;
        Scanning = true;
        radar.transform.position = location;
    }
}
