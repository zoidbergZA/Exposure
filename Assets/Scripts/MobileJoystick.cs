using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class MobileJoystick : MonoBehaviour
{
    [SerializeField] private GameObject joystickPanel;
    [SerializeField] private float minDragDistance = 20f;
    [SerializeField] private TouchInputType inputType;

    public DrillingDirection CurrentInput { get; private set; }
    public Vector2 JoystickInput { get; private set; }
    public enum ScreenTriangle { LEFT, RIGHT, UP, DOWN, NONE }
    public enum TouchInputType { TAP, SWIPE }

    private Vector2 dragPrevious;
    private Vector2 dragStart;

    private float lastInputAt = 0;
    private float inputCooldown = 0.15f;
    private ScreenTriangle inputTriangle;
    private Vector2 tap;

    void Awake()
    {
        CurrentInput = DrillingDirection.NONE;
        inputTriangle = ScreenTriangle.NONE;
    }

    void Update()
    {
        UpdateInput();
    }

    private void UpdateInput()
    {
        Vector2 input = Vector2.zero;

        if (GameManager.Instance.TouchInput)
        {
            switch(inputType)
            {
                case TouchInputType.SWIPE:
                    if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
                    {
                        dragStart = Input.touches[0].position;
                        input = Input.touches[0].position - GameManager.Instance.DrillingGame.Driller.Position;
                        dragPrevious = Input.touches[0].position;
                    }
                    else if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Moved)
                    {
                        Vector2 dragDelta = Input.touches[0].position - dragStart;
                        if (dragDelta.sqrMagnitude >= minDragDistance) input = Input.touches[0].deltaPosition;
                    }
                    break;
                case TouchInputType.TAP:
                    if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
                    {
                        tap = Input.touches[0].position;
                        inputTriangle = getCurrentTriangle(tap);
                    }
                    break;
                default:
                    Debug.LogException(new Exception("No touch input type has been selected!"));
                    break;
            }
        }
        else
        {
            //try keyboard input first
            if(Time.time - lastInputAt > inputCooldown) //cooldown
            {
                if (Input.GetKey(KeyCode.UpArrow))
                    input.y = 1f;
                else if (Input.GetKey(KeyCode.DownArrow))
                    input.y = -1f;
                else if (Input.GetKey(KeyCode.LeftArrow))
                    input.x = -1f;
                else if (Input.GetKey(KeyCode.RightArrow))
                    input.x = 1f;
                lastInputAt = Time.time;
            }
            
            //then try mouse input
            //if (input.sqrMagnitude < 0.1f)
            //{
            //    if (Input.GetMouseButtonDown(0))
            //    {
            //        dragStart = Input.mousePosition;
            //        input = Input.mousePosition - (Vector3)GameManager.Instance.DrillingGame.Driller.Position;
            //        dragPrevious = Input.mousePosition;
            //    }

            //    else if (Input.GetMouseButton(0))
            //    {
            //        Vector2 dragDelta = (Vector2)Input.mousePosition - dragStart;

            //        if (dragDelta.sqrMagnitude >= minDragDistance)
            //        {
            //            input = (Vector2) Input.mousePosition - dragPrevious;
            //        }

            //        dragPrevious = Input.mousePosition;
            //    }
            //}
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

        GameManager.Instance.DrillingGame.Hud.PointJoystickArrow(CurrentInput);
    }

    private void setDirection()
    {
        switch(inputTriangle)
        {
            case ScreenTriangle.DOWN:
                CurrentInput = DrillingDirection.DOWN;
                break;
            case ScreenTriangle.LEFT:
                CurrentInput = DrillingDirection.LEFT;
                break;
            case ScreenTriangle.RIGHT:
                CurrentInput = DrillingDirection.RIGHT;
                break;
            case ScreenTriangle.UP:
                CurrentInput = DrillingDirection.UP;
                break;
            case ScreenTriangle.NONE:
                CurrentInput = DrillingDirection.NONE;
                break;
            default:
                CurrentInput = DrillingDirection.NONE;
                break;
        }
    }

    private ScreenTriangle getCurrentTriangle(Vector2 tapPosition)
    {
        if (IsTapInsideTriangle(tapPosition, Vector2.zero, new Vector2(1, 0), getDrillerPosition())) return ScreenTriangle.DOWN;
        else if (IsTapInsideTriangle(tapPosition, new Vector2(1, 0), Vector2.one, getDrillerPosition())) return ScreenTriangle.RIGHT;
        else if (IsTapInsideTriangle(tapPosition, Vector2.one, new Vector2(0, 1), getDrillerPosition())) return ScreenTriangle.UP;
        else if (IsTapInsideTriangle(tapPosition, new Vector2(0, 1), Vector2.zero, getDrillerPosition())) return ScreenTriangle.LEFT;

        return ScreenTriangle.NONE;
    }

    private bool IsTapInsideTriangle(Vector2 s, Vector2 a, Vector2 b, Vector2 c)
    {
        float as_x = s.x - a.x;
        float as_y = s.y - a.y;

        bool s_ab = (b.x - a.x) * as_y - (b.y - a.y) * as_x > 0;

        if ((c.x - a.x) * as_y - (c.y - a.y) * as_x > 0 == s_ab) return false;

        if ((c.x - b.x) * (s.y - b.y) - (c.y - b.y) * (s.x - b.x) > 0 != s_ab) return false;

        return true;
    }

    private Vector2 getDrillerPosition()
    {
        return Camera.main.ScreenToViewportPoint(GameManager.Instance.DrillingGame.Driller.Drill.rectTransform.position);
    }
}
