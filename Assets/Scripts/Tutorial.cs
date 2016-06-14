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
                if (flickTip) flickTip.enabled = true;
                GameManager.Instance.Scanner.gameObject.SetActive(false);
                Player.PlanetFlicked += OnPlanetFlicked;
                break;

            case Progression.ActivateScanner:
                if(flickTip) flickTip.enabled = false;
                GameManager.Instance.Scanner.gameObject.SetActive(true);
                //                KillTutorial();
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
