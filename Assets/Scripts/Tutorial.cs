using System;
using UnityEngine;
using System.Collections;

public class Tutorial : MonoBehaviour
{
    public enum Progression
    {
        ActivateScanner,
        FindHotspot,
        Completed
    }

    private Progression progress;
    private float promptAt;
    private bool isShowingPrompt;

    void Awake()
    {
        SetProgress(Progression.ActivateScanner, Time.time + 5f);
    }

    void OnEnable()
    {
        Scanner.ScanStarted += OnScanStarted;
    }

    void OnDisable()
    {
        Scanner.ScanStarted -= OnScanStarted;
    }

    void Update()
    {
        if (Time.time >= promptAt && !isShowingPrompt)
        {
            ShowPrompt();
        }
    }

    private void SetProgress(Progression newProgess, float promptAt)
    {
        if (newProgess == Progression.Completed)
        {
            KillTutorial();
            return;
        }

        progress = newProgess;
        this.promptAt = promptAt;
    }

    private void ShowPrompt()
    {
        isShowingPrompt = true;

        Debug.Log("todo: show activate scanner tutorial " + Time.time);
    }

    private void KillTutorial()
    {
        
    }

    private void OnScanStarted()
    {
        if (progress == Progression.ActivateScanner)
        {
            Debug.Log("scan started for tutorial " + Time.time);

            SetProgress(Progression.FindHotspot, Time.time + 5f);
        }
    }
}
