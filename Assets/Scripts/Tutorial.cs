using System;
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
    
    [SerializeField] private GameObject flickTip;

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
                flickTip.SetActive(true);
                Player.PlanetFlicked += OnPlanetFlicked;
                break;

            case Progression.ActivateScanner:
                flickTip.SetActive(false);
                GameManager.Instance.Player.EnableRadar(true, GameManager.Instance.ScannerGadget.transform.position);
//                GameManager.Instance.Hud.ShowTipBubble(GameManager.Instance.Scanner.transform, true, 5f);
                KillTutorial();
                break;
        }
    }

    private void DisableAllTips()
    {
        flickTip.SetActive(false);
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
