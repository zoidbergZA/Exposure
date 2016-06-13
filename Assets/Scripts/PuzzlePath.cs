using UnityEngine;
using System.Collections;
using System.Linq;

public class PuzzlePath : MonoBehaviour
{
    [SerializeField] private Connectable[] connectablePath;
    
    public Connectable[] ConnectablePath { get { return connectablePath; } }
    public GeoThermalPlant GeoPlant { get { return connectablePath[0] as GeoThermalPlant; } }       //GeoPlant is first element
    public City ParentCity { get { return connectablePath[connectablePath.Length-1] as City; } }    //City is last element

    void Awake()
    {
        GeoPlant.SetMyPuzzlePath(this);
    }

    public void Reset()
    {
        for (int i = 0; i < connectablePath.Length; i++)
        {
            connectablePath[i].Reset();
        }
    }
}
