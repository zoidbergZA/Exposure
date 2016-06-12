using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridBuilder : Minigame
{
    public PuzzlePath PuzzlePath { get; private set; }

    [SerializeField] private Texture2D buildPylonIcon;
    [SerializeField] private Texture2D buildCityIcon;

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
        PreviewNextPylon();
    }

    public override void Update()
    {
        base.Update();
        
        
    }

    void OnGUI()
    {
        if (!IsRunning)
            return;

        if (nextPylon < PuzzlePath.PathPylons.Length)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(PuzzlePath.PathPylons[nextPylon].transform.position);

            if (GUI.Button(GameManager.Instance.Hud.CenteredRect(new Rect(screenPos.x, screenPos.y, 50, 50)),
                buildPylonIcon))
            {
                PuzzlePath.PathPylons[nextPylon].Build();
                nextPylon++;
                PreviewNextPylon();
            }
        }
        else
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(PuzzlePath.ParentCity.transform.position);

            if (GUI.Button(GameManager.Instance.Hud.CenteredRect(new Rect(screenPos.x, screenPos.y, 50, 50)),
                buildCityIcon))
            {
                End(true);
            }
        }
    }

    public override void End(bool succeeded)
    {
        base.End(succeeded);

        if (!succeeded)
        {
            PuzzlePath.Reset();
        }

        GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
    }

    private void PreviewNextPylon()
    {
        if (nextPylon >= PuzzlePath.PathPylons.Length)
            return;

        PuzzlePath.PathPylons[nextPylon].ShowPreview();
        GameManager.Instance.Director.SetMode(Director.Modes.Grid, PuzzlePath.PathPylons[nextPylon].transform, 1f);
    }
}
