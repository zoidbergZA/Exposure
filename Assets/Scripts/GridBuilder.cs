using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridBuilder : Minigame
{
    public PuzzlePath PuzzlePath { get; private set; }
    
    [SerializeField] private float connectionTimeOut = 2f;
    [SerializeField] private float cameraSwoopTime = 1f;
    [SerializeField] private float cityCinematicDuration = 8f;

    private float lastConnectionAt;
    private int nextConnectable;

    public void SetPuzzlePath(PuzzlePath puzzlePath)
    {
        PuzzlePath = puzzlePath;
    }

    public override void Begin(float difficulty)
    {
        base.Begin(difficulty);

        GameManager.Instance.Director.SetSunlightBrightness(false);
        GameManager.Instance.Hud.ShowStatusPanel(true);
        nextConnectable = 1; // first pylon in array
        lastConnectionAt = Time.time;
        PreviewNextConnectable();
    }

    public override void Update()
    {
        base.Update();

        if (IsRunning && Time.time >= lastConnectionAt + connectionTimeOut)
        {
            AutoMakeNextConnection();
        }
    }
    
    public override void End(bool succeeded)
    {
        base.End(succeeded);

        if (succeeded)
        {
            PuzzlePath.ParentCity.CleanUp();
        }
        else
        {
            PuzzlePath.Reset();
        }

        StartCoroutine(GoToNormalStateAfter(cityCinematicDuration));
    }

    public void MakeConnection(Connectable connectable)
    {
        //find previous connectable
        int previous = 0;
        for (int i = 0; i < PuzzlePath.ConnectablePath.Length; i++)
        {
            if (PuzzlePath.ConnectablePath[i] == connectable)
            {
                previous = i - 1;
            }
        }

        connectable.MakeConnection(PuzzlePath.ConnectablePath[previous]);
        nextConnectable++;
        lastConnectionAt = Time.time;

        if (nextConnectable >= PuzzlePath.ConnectablePath.Length)
        {
            End(true);
        }
        else
            PreviewNextConnectable();
    }

    private void AutoMakeNextConnection()
    {
        MakeConnection(PuzzlePath.ConnectablePath[nextConnectable]);

        string message = GameManager.Instance.Hud.NegativeMessages[Random.Range(0, GameManager.Instance.Hud.NegativeMessages.Length)];
        GameManager.Instance.Hud.NewFloatingText(message, PuzzlePath.ConnectablePath[nextConnectable-1].transform, false);
    }

    private IEnumerator GoToNormalStateAfter(float delay)
    {
        Vector3 target = PuzzlePath.GeoPlant.transform.position;

        if (PuzzlePath.GeoPlant.transform.position.y - PuzzlePath.ParentCity.transform.position.y > 0)
        {
            target.y -= 250f;
        }
        
        Vector3 pos = target + PuzzlePath.GeoPlant.transform.up * 80f;
        Quaternion rot = Quaternion.LookRotation(PuzzlePath.ParentCity.transform.position - pos, PuzzlePath.ParentCity.transform.up);

        //flip pos if its 'y' is lower
        float yDelta = pos.y - PuzzlePath.GeoPlant.transform.position.y;
        if (yDelta > 0)
        {
            pos.y += yDelta*2f;
        }

        GameManager.Instance.Director.SwoopTo(pos, rot, 35f, delay);

        yield return new WaitForSeconds(delay);

        // set scanner position to last pylon
        GameManager.Instance.Player.GoToNormalState(PuzzlePath.ConnectablePath[PuzzlePath.ConnectablePath.Length-2].transform.position);
    }

    private void PreviewNextConnectable()
    {
        if (nextConnectable < PuzzlePath.ConnectablePath.Length)
        {
            PuzzlePath.ConnectablePath[nextConnectable].ShowPreview();
            GameManager.Instance.Director.SetMode(Director.Modes.Grid, PuzzlePath.ConnectablePath[nextConnectable].transform, cameraSwoopTime);
        }
    }
}
