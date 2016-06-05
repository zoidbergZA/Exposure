using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum AxisOption
    {
        // Options for which axes to use
        Both, // Use both
        OnlyHorizontal, // Only horizontal
        OnlyVertical // Only vertical
    }

    public int MovementRange = 100;
    public AxisOption axesToUse = AxisOption.Both; // The options for the axes that the still will use
    public string horizontalAxisName = "Horizontal"; // The name given to the horizontal axis for the cross platform input
    public string verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input

    Vector3 m_StartPos;
    bool m_UseX; // Toggle for using the x axis
    bool m_UseY; // Toggle for using the Y axis
    CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
    CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input

    public Vector3 StartPosition { set { m_StartPos = value; } }

    void Start()
    {
        CreateVirtualAxes(); //chiggy
    }

    void UpdateVirtualAxes(Vector3 value)
    {
        var delta = m_StartPos - value;
        delta.y = -delta.y;
        delta /= MovementRange;
        GameManager.Instance.Hud.JoystickX = -delta.x;
        GameManager.Instance.Hud.JoystickY = delta.y;
    }

    void CreateVirtualAxes()
    {
        // set axes to use
        m_UseX = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal);
        m_UseY = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical);
    }


    public void OnDrag(PointerEventData data)
    {
        Vector3 newPos = Vector3.zero;

        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING)
        {
            if (m_UseY)
            {
                int delta = (int)(data.position.y - m_StartPos.y);
                newPos.y = delta;
            }
        }
        else if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            if (GameManager.Instance.DrillingGame.DrillDirection == DrillingGame.DrillingDirection.DOWN ||
                GameManager.Instance.DrillingGame.DrillDirection == DrillingGame.DrillingDirection.UP)
            {
                if (m_UseX)
                {
                    int delta = (int)(data.position.x - m_StartPos.x);
                    newPos.x = delta;
                }
            }
            else if (GameManager.Instance.DrillingGame.DrillDirection == DrillingGame.DrillingDirection.RIGHT ||
                GameManager.Instance.DrillingGame.DrillDirection == DrillingGame.DrillingDirection.LEFT)
            {
                if (m_UseY)
                {
                    int delta = (int)(data.position.y - m_StartPos.y);
                    newPos.y = delta;
                }
            }
        }
        transform.position = Vector3.ClampMagnitude(new Vector3(newPos.x, newPos.y, newPos.z), MovementRange) + m_StartPos;
        UpdateVirtualAxes(transform.position);
    }


    public void OnPointerUp(PointerEventData data)
    {
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING ||
            GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            transform.position = m_StartPos;
            UpdateVirtualAxes(m_StartPos);
        }
    }


    public void OnPointerDown(PointerEventData data) { }

    void OnDisable()
    {
        
    }
}
