using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class MobileJoystick : MonoBehaviour
{
    [SerializeField] private GameObject joystickPanel;

    public DrillingDirection CurrentInput { get; private set; }
    public GameObject JoystickPanel { get { return joystickPanel; } }
    public Vector2 JoystickInput { get; private set; }

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

        if (Input.GetKey(KeyCode.UpArrow))
            input.y = 1f;
        else if (Input.GetKey(KeyCode.DownArrow))
            input.y = -1f;
        else if (Input.GetKey(KeyCode.LeftArrow))
            input.x = -1f;
        else if (Input.GetKey(KeyCode.RightArrow))
            input.x = 1f;

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

        GameManager.Instance.Hud.PointJoystickArrow(CurrentInput);
    }
}
