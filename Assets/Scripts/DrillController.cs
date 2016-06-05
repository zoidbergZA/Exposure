using UnityEngine;
using System.Collections;

public class DrillController : MonoBehaviour
{
//    public enum Directions
//    {
//        NONE,
//        LEFT,
//        RIGHT,
//        UP,
//        DOWN
//    }

    private Vector2 mouseOld;

    public DrillingDirection Direction { get; private set; }

    public void Update()
    {
        Vector2 delta = GetInputMovement();

        //return if no significant delta
        if (delta.sqrMagnitude <= 0.05f)
        {
            Direction = DrillingDirection.NONE;
            return;
        }

        // get swipe direction
        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            if (delta.x <= 0)
                Direction = DrillingDirection.LEFT;
            else
                Direction = DrillingDirection.RIGHT;
        }
        else
        {
            if (delta.y >= 0)
                Direction = DrillingDirection.UP;
            else
                Direction = DrillingDirection.DOWN;
        }
    }

    private Vector2 GetInputMovement()
    {
        Vector2 movement = Vector2.zero;

        if (GameManager.Instance.TouchInput)
        {
            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Moved)
            {
                movement = Input.touches[0].deltaPosition;
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                movement = (Vector2)Input.mousePosition - mouseOld;
                mouseOld = Input.mousePosition;
            }
        }

        return movement;
    }

//    void OnGUI()
//    {
//        GUI.Label(new Rect(200, 180, 100, 40), Direction.ToString());
//    }
}
