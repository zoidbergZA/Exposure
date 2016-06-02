using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public enum Progression
    {
        ActivateScanner,
        FindHotspot,
        Completed
    }

    //todo: refactor to prompt class
    [SerializeField] private Image scannerTip;
    [SerializeField] private Image hotspotTip;
    [SerializeField] private float scannerWait = 5f;
    [SerializeField] private float hotspotWait = 8f;

    private Progression progress;
    private float promptAt;

    void Awake()
    {
        SetProgress(Progression.ActivateScanner, Time.time + scannerWait);
    }

    void OnEnable()
    {
        Scanner.ScanStarted += OnScanStarted;
        Scanner.HotspotFound += OnHotspotFound;
    }

    void OnDisable()
    {
        Scanner.ScanStarted -= OnScanStarted;
        Scanner.HotspotFound -= OnHotspotFound;
    }

    void Update()
    {
        if (Time.time >= promptAt)
        {
            ShowPrompt();
        }
    }

    private void SetProgress(Progression newProgess, float promptAt)
    {
        DisableAllTips();
        progress = newProgess;
        this.promptAt = promptAt;

        switch (newProgess)
        {
            case Progression.FindHotspot:

                break;

            case Progression.Completed:
                KillTutorial();
                break;
        }
    }

    private void ShowPrompt()
    {
        switch (progress)
        {
            case Progression.ActivateScanner:
                scannerTip.enabled = true;
                break;
            case Progression.FindHotspot:
                hotspotTip.enabled = true;
                break;
        }
    }

    private void DisableAllTips()
    {
        scannerTip.enabled = false;
        hotspotTip.enabled = false;
    }

    private void KillTutorial()
    {
        Destroy(gameObject);
    }

    private void OnScanStarted()
    {
        if (progress == Progression.ActivateScanner)
        {
            SetProgress(Progression.FindHotspot, Time.time + hotspotWait);
        }
    }

    private void OnHotspotFound()
    {
        if (progress == Progression.FindHotspot)
        {
            SetProgress(Progression.Completed, 0);
        }
    }
}
