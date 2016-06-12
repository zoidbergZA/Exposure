using UnityEngine;
using System.Collections;
using System.Linq;

public class PuzzlePath : MonoBehaviour
{
    [SerializeField] private GeoThermalPlant geoPlant;
    [SerializeField] private Pylon[] pathPylons;

    public City ParentCity { get; private set; }
    public Pylon[] PathPylons { get { return pathPylons; } }
    public GeoThermalPlant GeoPlant { get { return geoPlant; } }
//    public int CurrentTarget { get; private set; }
//    public bool IsCompleted { get; private set; }

    public void SetParentCity(City parentCity)
    {
        ParentCity = parentCity;
        geoPlant.SetMyPuzzlePath(this);
    }

    public void Reset()
    {
//        CurrentTarget = 0;

        for (int i = 0; i < pathPylons.Length; i++)
        {
            pathPylons[i].Reset();
        }
    }

//    public void ConnectPylon(Pylon pylon)
//    {
//        if (IsCompleted)
//            return;
//
//        if (pathPylons.Contains(pylon) && pathPylons[CurrentTarget] == pylon)
//        {
//            pylon.Build();
//            GameManager.Instance.Director.SetMode(Director.Modes.Grid, pylon.transform);
//
//            CurrentTarget++;
//
//            if (CurrentTarget >= pathPylons.Length)
//            {
//                IsCompleted = true;
//                ParentCity.CleanUp();
//
////                Debug.Log("puzzle completed! " + Time.time);
//            }
//        }
//    }
}
