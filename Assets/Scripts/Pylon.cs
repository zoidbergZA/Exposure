using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pylon : MonoBehaviour
{
    private List<Pylon> Connections;

    void Awake()
    {
        Connections = new List<Pylon>();
    }
}
