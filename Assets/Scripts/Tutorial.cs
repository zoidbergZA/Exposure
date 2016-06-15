﻿using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public enum Progression
    {
        FlickPlanet,
        ActivateScanner,
    }
    
    [SerializeField] private Image flickTip;

    private Progression progress;

    void Start()
    {
        SetProgress(Progression.FlickPlanet);
    }
    
    void Update()
    {
        
    }

    public void SetProgress(Progression newProgess)
    {
//        DisableAllTips();
        progress = newProgess;
        
        switch (newProgess)
        {
            case Progression.FlickPlanet:
                flickTip.enabled = true;
                Player.PlanetFlicked += OnPlanetFlicked;
                break;

            case Progression.ActivateScanner:
                flickTip.enabled = false;
                GameManager.Instance.Hud.ShowTipBubble(GameManager.Instance.ScannerGadget.transform, true, 5f);
                KillTutorial();
                break;
        }
    }

    private void DisableAllTips()
    {
        flickTip.enabled = false;
    }

    private void KillTutorial()
    {
        Destroy(gameObject);
    }

    private void OnPlanetFlicked()
    {
        Player.PlanetFlicked -= OnPlanetFlicked;

        SetProgress(Progression.ActivateScanner);
    }

    private void OnScanStarted()
    {
//        if (progress == Progression.ActivateScanner)
//        {
//            SetProgress(Progression.FindHotspot);
//        }
    }

    private void OnHotspotFound()
    {
//        if (progress == Progression.FindHotspot)
//        {
//            SetProgress(Progression.Completed);
//        }
    }
}
