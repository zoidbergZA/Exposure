﻿using UnityEngine;
using System.Collections;

public class City : MonoBehaviour
{
    [SerializeField] private PuzzlePath puzzlePath;

    public bool IsDirty { get; private set; }
    
    void Start()
    {

    }

    public void TryBuild(Pylon pylon)
    {
        puzzlePath.TryConnectPylon(pylon);
    }
}
