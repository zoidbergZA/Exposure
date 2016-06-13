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

        if (IsRunning && Time.time >= lastConnectionAt + connectionTimeOut)
        {
            MakeNextConnection();
        }
    }

    void OnGUI()
    {
//        if (!IsRunning)
//            return;
//
//        if (nextConnectable < PuzzlePath.ConnectablePath.Length)
//        {
//            Vector2 screenPos = Camera.main.WorldToScreenPoint(PuzzlePath.ConnectablePath[nextConnectable].transform.position);
//
//            if (GUI.Button(GameManager.Instance.Hud.CenteredRect(new Rect(screenPos.x, screenPos.y, 50, 50)),
//                buildPylonIcon))
//            {
//                MakeNextConnection();
//            }
//        }
//        else
//        {
//            Vector2 screenPos = Camera.main.WorldToScreenPoint(PuzzlePath.ParentCity.transform.position);
//
//            if (GUI.Button(GameManager.Instance.Hud.CenteredRect(new Rect(screenPos.x, screenPos.y, 50, 50)),
//                buildCityIcon))
//            {
//                End(true);
//            }
//        }
    }

    public override void End(bool succeeded)
    {
        base.End(succeeded);

        if (!succeeded)
        {
            PuzzlePath.Reset();
        }

        PuzzlePath.ParentCity.CleanUp();
        GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
    }

    public void MakeConnection(Connectable connectable)
    {
        Debug.Log("connect " + connectable.transform.name);
    }

    private void MakeNextConnection()
    {
//        if (nextConnectable < PuzzlePath.ConnectablePath.Length)
//            PuzzlePath.ConnectablePath[nextConnectable].Build();

//        //pipe test
//        PuzzlePath.PathPylons[nextConnectable].MakeConnection(PuzzlePath.PathPylons[nextConnectable+1]);

        lastConnectionAt = Time.time;
        nextConnectable++;
        PreviewNextConnectable();
    }

    private void PreviewNextConnectable()
    {
        Debug.Log(PuzzlePath.ConnectablePath.Length);
        if (nextConnectable < PuzzlePath.ConnectablePath.Length)
        {
            PuzzlePath.ConnectablePath[nextConnectable].ShowPreview();
            GameManager.Instance.Director.SetMode(Director.Modes.Grid, PuzzlePath.ConnectablePath[nextConnectable].transform, cameraSwoopTime);
        }

//        if (nextConnectable >= PuzzlePath.ConnectablePath.Length)
//        {
//            GameManager.Instance.Director.SetMode(Director.Modes.Grid, PuzzlePath.ParentCity.transform, cameraSwoopTime);
//        }
//        else
//        {
//            PuzzlePath.ConnectablePath[nextConnectable].ShowPreview();
//            GameManager.Instance.Director.SetMode(Director.Modes.Grid, PuzzlePath.ConnectablePath[nextConnectable].transform, cameraSwoopTime);
//        }
    }
}
