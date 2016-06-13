using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridBuilder : Minigame
{
    public PuzzlePath PuzzlePath { get; private set; }
    
    [SerializeField] private float connectionTimeOut = 2f;
    [SerializeField] private float cameraSwoopTime = 1f;

    private float lastConnectionAt;
    private int nextConnectable;

    public void SetPuzzlePath(PuzzlePath puzzlePath)
    {
        PuzzlePath = puzzlePath;
    }

    public override void Begin(float difficulty)
    {
        base.Begin(difficulty);

        nextConnectable = 1; // first pylon in array
        lastConnectionAt = Time.time;
        PreviewNextConnectable();
    }

    public override void Update()
    {
        base.Update();

//        if (IsRunning && Time.time >= lastConnectionAt + connectionTimeOut)
//        {
//            MakeNextConnection();
//        }
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
        
        GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
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

        if (nextConnectable >= PuzzlePath.ConnectablePath.Length)
        {
            End(true);
        }
        else
            PreviewNextConnectable();
    }

    private void MakeNextConnection()
    {
        lastConnectionAt = Time.time;
        nextConnectable++;
        PreviewNextConnectable();
    }

    private void PreviewNextConnectable()
    {
        //todo: hide previous connectable preview


        if (nextConnectable < PuzzlePath.ConnectablePath.Length)
        {
            PuzzlePath.ConnectablePath[nextConnectable].ShowPreview();
            GameManager.Instance.Director.SetMode(Director.Modes.Grid, PuzzlePath.ConnectablePath[nextConnectable].transform, cameraSwoopTime);
        }
    }
}
