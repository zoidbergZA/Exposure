﻿using UnityEngine;
using System.Collections;

public class DrillingGame : Minigame
{
    [SerializeField] private GeoThermalPlant geoThermalPlantPrefab;
    [SerializeField] private UnityEngine.UI.Image bgActive;
    [SerializeField] private UnityEngine.UI.Image bgInactive;
    [SerializeField] private UnityEngine.UI.Image drill;
    [SerializeField] private Vector3[] columns;
    private Drillspot drillspot;
    public enum DrillingGameState { INACTIVE, SLIDING, DRILLING, SUCCESS }
    private DrillingGameState state;
    private bool makeDrill = false;
    private Vector3 initDrillPos;
    private int targetColumn;
    private bool slidingLeft = false;
    public DrillingGameState GetState { get { return state; } }
    public void SetMakeDrill(bool value) { makeDrill = value; }
    public UnityEngine.UI.Image GetDrill { get { return drill; } }

    void Start()
    {
        activateImages(false);
        if (drill) initDrillPos = drill.transform.position;
    }

    public void StartGame(Drillspot drillspot)
    {
        if (IsRunning) return;
        this.drillspot = drillspot;
        Begin();
        state = DrillingGameState.SLIDING;
    }

    private void updateState()
    {
        switch(state)
        {
            case DrillingGameState.DRILLING:
                handleDrillingState();
                break;
            case DrillingGameState.SLIDING:
                handleSlidingState();
                break;
            case DrillingGameState.INACTIVE:
                handleInactiveState();
                break;
            case DrillingGameState.SUCCESS:
                handleSuccessState();
                break;
        }
    }

    private void handleDrillingState()
    {
        if (drill && drill.transform.position.y > initDrillPos.y - 350)
        {
            drill.transform.Translate(0, -1.0f, 0);
        }
        else
        {
            drill.transform.position = initDrillPos;
            state = DrillingGameState.SUCCESS;
        }
    }

    public void MoveRight()
    {
        Debug.Log("moving right");
    }

    public void MoveLeft()
    {
        Debug.Log("moving left");
    }

    private void handleSlidingState()
    {
        activateImages(true);
        if(drill) updateDrillMovement();
    }

    private void updateDrillMovement()
    {
        if (!makeDrill)
        {
            if (slidingLeft == false)
            {
                drill.transform.Translate(new Vector3(1, 0, 0));
                if (targetColumn < columns.Length - 1)
                {
                    if (drill.rectTransform.anchoredPosition.x == columns[targetColumn + 1].x) targetColumn += 1;
                }
                else slidingLeft = true;
            }
            else
            {
                drill.transform.Translate(new Vector3(-1, 0, 0));
                if (targetColumn > 0)
                {
                    if (drill.rectTransform.anchoredPosition.x == columns[targetColumn - 1].x) targetColumn -= 1;
                }
                else slidingLeft = false;
            }
        }
        else state = DrillingGameState.DRILLING;
    }

    private void handleInactiveState()
    {
        activateImages(false);
    }

    private void handleSuccessState()
    {
        state = DrillingGameState.INACTIVE;
        End(true);
    }

    public override void Update()
    {
        base.Update();
        updateState();

        //todo: minigame logic here, auto-win close to timeOut for now
        if (IsRunning && Timeleft <= 0.5f) End(false);
        if (GameManager.Instance.Player.PlayerState == Player.PlayerStates.Normal)
        {
            state = DrillingGameState.INACTIVE;
            End(false);
            slidingLeft = false;
        }
    }

    public override void End(bool succeeded)
    {
        base.End(succeeded);

        if (succeeded)
        {
            GeoThermalPlant plant = Instantiate(geoThermalPlantPrefab, drillspot.transform.position, drillspot.transform.rotation) as GeoThermalPlant;
            Destroy(drillspot.gameObject);
            
            makeDrill = false;
            slidingLeft = false;
            GameManager.Instance.Player.StartBuildMinigame(plant);
        }
        else
        {
            GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
            makeDrill = false;
            slidingLeft = false;
        }
    }

    private void activateImages(bool activate)
    {
        if(activate)
        {
            if (bgActive) bgActive.gameObject.SetActive(true);
            if (bgInactive) bgInactive.gameObject.SetActive(false);
            if (drill) drill.gameObject.SetActive(true);
        } else
        {
            if (bgActive) bgActive.gameObject.SetActive(false);
            if (bgInactive) bgInactive.gameObject.SetActive(true);
            if (drill) drill.gameObject.SetActive(false);
        }
    }
}
