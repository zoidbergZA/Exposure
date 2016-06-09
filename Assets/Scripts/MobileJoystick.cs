using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class MobileJoystick : MonoBehaviour
{
    [SerializeField] private GameObject joystickPanel;
    [SerializeField] private float minDragDistance = 20f;

    public DrillingDirection CurrentInput { get; private set; }
    public GameObject JoystickPanel { get { return joystickPanel; } }
    public Vector2 JoystickInput { get; private set; }

    private Vector2 dragPrevious;
    private Vector2 dragStart;

    void Awake()
    {
        CurrentInput = DrillingDirection.NONE;   
    }

    void Update()
    {
        UpdateInput();
        //Debug.Log("x: " + joystickX + " | y: " + joystickY);
//        Debug.Log(CurrentInput);
    }

    private void UpdateInput()
    {
        Vector2 input = Vector2.zero;

        if (GameManager.Instance.TouchInput)
        {
            if (Input.touchCount > 0)
            {
                if (Input.touches[0].phase == TouchPhase.Began)
                {
                    dragStart = Input.touches[0].position;
                    input = Input.touches[0].position - (Vector2)GameManager.Instance.DrillingGame.Driller.Drill.rectTransform.position;
                    dragPrevious = Input.touches[0].position;
                }
                else if (Input.touches[0].phase == TouchPhase.Moved)
                {
                    Vector2 dragDelta = Input.touches[0].position - dragStart;

                    if (dragDelta.sqrMagnitude >= minDragDistance)
                    {
                        input = Input.touches[0].deltaPosition;
                    }
                }
            }
        }
        else
        {
            //try keyboard input first
            if (Input.GetKey(KeyCode.UpArrow))
                input.y = 1f;
            else if (Input.GetKey(KeyCode.DownArrow))
                input.y = -1f;
            else if (Input.GetKey(KeyCode.LeftArrow))
                input.x = -1f;
            else if (Input.GetKey(KeyCode.RightArrow))
                input.x = 1f;

            //then try mouse input
            if (input.sqrMagnitude < 0.1f)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    dragStart = Input.mousePosition;
                    input = Input.mousePosition - GameManager.Instance.DrillingGame.Driller.Drill.rectTransform.position;
                    dragPrevious = Input.mousePosition;
                }

                else if (Input.GetMouseButton(0))
                {
                    Vector2 dragDelta = (Vector2)Input.mousePosition - dragStart;

                    if (dragDelta.sqrMagnitude >= minDragDistance)
                    {
                        input = (Vector2) Input.mousePosition - dragPrevious;
                    }

                    dragPrevious = Input.mousePosition;
                }
            }
        }

        JoystickInput = input;
        SetCurrentDirection();
    }

    private void SetCurrentDirection()
    {
        if (JoystickInput.sqrMagnitude < 0.1f)
            return;

        if (Mathf.Abs(JoystickInput.x) > Mathf.Abs(JoystickInput.y))
        {
            if (JoystickInput.x < 0)
                CurrentInput = DrillingDirection.LEFT;
            else
                CurrentInput = DrillingDirection.RIGHT;
        }
        else
        {
            if (JoystickInput.y < 0)
                CurrentInput = DrillingDirection.DOWN;
            else
                CurrentInput = DrillingDirection.UP;
        }

        GameManager.Instance.DrillingGame.PointJoystickArrow(CurrentInput);
    }
}
