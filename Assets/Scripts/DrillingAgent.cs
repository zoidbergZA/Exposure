using UnityEngine;
using System.Collections;

public class DrillingAgent : MonoBehaviour
{
    public DrillGameMap Map { get; set; }
    public RectTransform RectTransform { get; private set; }
    public DrillingDirection DrillingDirection { get; private set; }
    public DrillingDirection NextDirection { get; private set; }
    public Vector2 CurrentTile { get; private set; } // current point of control (where Direction will change)

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        DrillingDirection = DrillingDirection.NONE;
    }

    void Update()
    {
        Move(DrillingDirection);
    }

    public void SetGridPosition(int x, int y)
    {
        RectTransform.anchoredPosition = Map.GetTilePivotPosition(x, y);
        CurrentTile = new Vector2(x, y);
    }

    public void Move(DrillingDirection direction)
    {
        if (direction == DrillingDirection.NONE)
            return;
        
        //todo: handle movement
        
        //check if current tile reached, call function        
    }

    private void OnCurrentTileReached()
    {
        //set current tile based on movement Direction

        //change current DrillingDirection to next Direction

        //handle collition with next tile
    }
}
