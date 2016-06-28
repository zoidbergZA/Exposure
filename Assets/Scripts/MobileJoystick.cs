using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

public class MobileJoystick : MonoBehaviour
{
    [SerializeField] private GameObject joystickPanel;
    [SerializeField] private float minDragDistance = 20f;
    [SerializeField] private TouchInputType inputType;

    public DrillingDirection CurrentInput { get; private set; }
    public Vector2 JoystickInput { get; private set; }
    public enum ScreenTriangle { LEFT, RIGHT, UP, DOWN, NONE }
    public enum TouchInputType { TAP, SWIPE }
    public DrillGameMap Map { get; private set; }
    public string DebugText { get; set; }

    private Vector2 dragPrevious;
    private Vector2 dragStart;

    private ScreenTriangle inputTriangle;
    private Vector2 tap;
    private float touchDuration;
    private Touch toucH;
    private int clicks;

    void Awake()
    {
        Reset();
        Map = FindObjectOfType<DrillGameMap>();
    }

    void Update()
    {
        UpdateInput();
        //cheat toggle between TAP and SWIPE input types
        //if (Input.GetKeyDown(KeyCode.S) && inputType == TouchInputType.TAP) inputType = TouchInputType.SWIPE;
        //else if (Input.GetKeyDown(KeyCode.S) && inputType == TouchInputType.SWIPE) inputType = TouchInputType.TAP;
        processDoubleTap();
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
                        tap = Camera.main.ScreenToViewportPoint(Input.touches[0].position);
                        inputTriangle = getCurrentTriangle(tap);
                        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
                            GameManager.Instance.DrillingGame.Hud.JoystickArrow.color = new Color(1, 1, 1, 1);
                        setDirection();
                    }
                    break;
                default:
                    Debug.LogException(new Exception("No touch input obj has been selected!"));
                    break;
            }
        }
        else
        {
            switch(inputType) //mouse input
            {
                case TouchInputType.TAP:
                    if (Input.GetMouseButtonDown(0))
                    {
                        tap = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                        inputTriangle = getCurrentTriangle(tap);
                        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
                            GameManager.Instance.DrillingGame.Hud.JoystickArrow.color = new Color(1, 1, 1, 1);
                        setDirection();
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        clicks++;
                    }
                    break;
                case TouchInputType.SWIPE:
                    if (input.sqrMagnitude < 0.1f)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            dragStart = Input.mousePosition;
                            input = Input.mousePosition - (Vector3)GameManager.Instance.DrillingGame.Driller.Position;
                            dragPrevious = Input.mousePosition;
                        }
                        else if (Input.GetMouseButton(0))
                        {
                            Vector2 dragDelta = (Vector2)Input.mousePosition - dragStart;
                            if (dragDelta.sqrMagnitude >= minDragDistance) input = (Vector2)Input.mousePosition - dragPrevious;
                            dragPrevious = Input.mousePosition;
                        }
                    }
                    break;
                default:
                    if (Input.GetMouseButtonDown(0))
                    {
                        tap = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                        inputTriangle = getCurrentTriangle(tap);
                        if(GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
                            GameManager.Instance.DrillingGame.Hud.JoystickArrow.color = new Color(1, 1, 1, 1);
                        setDirection();
                    }
                    break;
            }
        }

        JoystickInput = input;
        SetCurrentDirection();
    }

    private void SetCurrentDirection()
    {
        if (JoystickInput.sqrMagnitude < 0.1f) return;
        if (Mathf.Abs(JoystickInput.x) > Mathf.Abs(JoystickInput.y))
        {
            if (JoystickInput.x < 0) CurrentInput = DrillingDirection.LEFT;
            else CurrentInput = DrillingDirection.RIGHT;
        }
        else
        {
            if (JoystickInput.y < 0) CurrentInput = DrillingDirection.DOWN;
            else CurrentInput = DrillingDirection.UP;
        }
        //GameManager.Instance.DrillingGame.Hud.PointJoystickArrow(CurrentInput);
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
        GameManager.Instance.DrillingGame.Hud.PointJoystickArrow(CurrentInput);
    }

    private ScreenTriangle getCurrentTriangle(Vector2 tapPosition)
    {
        if (IsTapInsideTriangle(tapPosition, new Vector2(-1, -1) + getDrillerOffset(), new Vector2(2, -1) + getDrillerOffset(), getDrillerPosition()))
            return ScreenTriangle.DOWN;
        else if (IsTapInsideTriangle(tapPosition, new Vector2(2, -1) + getDrillerOffset(), new Vector2(2, 2) + getDrillerOffset(), getDrillerPosition()))
            return ScreenTriangle.RIGHT;
        else if (IsTapInsideTriangle(tapPosition, new Vector2(2, 2) + getDrillerOffset(), new Vector2(-1, 2) + getDrillerOffset(), getDrillerPosition()))
            return ScreenTriangle.UP;
        else if (IsTapInsideTriangle(tapPosition, new Vector2(-1, 2) + getDrillerOffset(), new Vector2(-1, -1) + getDrillerOffset(), getDrillerPosition()))
            return ScreenTriangle.LEFT;

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

    private Vector2 getDrillerOffset()
    {
        return Camera.main.ScreenToViewportPoint((Vector2)GameManager.Instance.DrillingGame.Driller.Drill.rectTransform.position
            - new Vector2(Screen.width/2, Screen.height/2));
    }

    public void Reset()
    {
        CurrentInput = DrillingDirection.NONE;
        inputTriangle = ScreenTriangle.NONE;
    }

    IEnumerator singleOrDouble()
    {
        yield return new WaitForSeconds(0.6f);
        switch(GameManager.Instance.TouchInput)
        {
            case true:
                if (toucH.tapCount == 1) GameManager.Instance.DrillingGame.Boost = false;
                else if (toucH.tapCount == 2)
                {
                    //this coroutine has been called twice. We should stop the next one here otherwise we get two double tap
                    StopCoroutine("singleOrDouble");
                    GameManager.Instance.DrillingGame.Boost = true;
                }
                break;
            case false:
                if (clicks == 1) GameManager.Instance.DrillingGame.Boost = false;
                else if (clicks == 2)
                {
                    StopCoroutine("singleOrDouble");
                    GameManager.Instance.DrillingGame.Boost = true;
                }
                clicks = 0;
                break;
        }
    }

    private void processDoubleTap()
    {
        switch (GameManager.Instance.TouchInput)
        {
            case true:
                if (Input.touchCount > 0) //if there is any touch
                {
                    touchDuration += Time.deltaTime;
                    toucH = Input.GetTouch(0);

                    if (toucH.phase == TouchPhase.Ended && touchDuration < 0.2f) //making sure it only check the touch once && it was a short touch/tap and not a dragging.
                        StartCoroutine("singleOrDouble");
                }
                else
                    touchDuration = 0.0f;
                break;
            case false:
                if(clicks > 0)
                {
                    touchDuration += Time.deltaTime;
                    if (touchDuration < 0.5f) StartCoroutine("singleOrDouble");
                }
                else
                    touchDuration = 0.0f;
                break;
        }
    }
}
