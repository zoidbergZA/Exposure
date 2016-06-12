using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridBuilder : Minigame
{
    public PuzzlePath PuzzlePath { get; private set; }

    private float lastPylonConnectedAt;
    private int nextPylon;

    public void SetPuzzlePath(PuzzlePath puzzlePath)
    {
        PuzzlePath = puzzlePath;
    }

    public override void Begin(float difficulty)
    {
        base.Begin(difficulty);

        nextPylon = 0;
        lastPylonConnectedAt = Time.time;

//        foreach (Pylon pylon in PuzzlePath.PathPylons)
//        {
//            pylon.Build();
//        }
    }

    public override void Update()
    {
        base.Update();
        
    }

    public override void End(bool succeeded)
    {
        base.End(succeeded);

        GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
    }
}
