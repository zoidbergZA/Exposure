using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private UnityEngine.UI.Image directionArrows;
    [SerializeField] private UnityEngine.UI.Image innerPad;
    [SerializeField] private Sprite arrowDown;
    [SerializeField] private Sprite arrowUpDown;
    [SerializeField] private Sprite arrowLeftRight;
    [SerializeField] private GameObject joystickPanel;

    public DrillingDirection CurrentInput { get; private set; }
    public DrillingDirection PrevInput { get; private set; }
    public GameObject JoystickPanel { get { return joystickPanel; } }
    public UnityEngine.UI.Image InnerPad { get { return innerPad; } }
    public bool JustTurned { get; set; }
    public Vector3 StartPosition { set { m_StartPos = value; } }

    private float joystickX, joystickY = 0;
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

    void Start()
    {
        CreateVirtualAxes();
        CurrentInput = DrillingDirection.NONE;
        PrevInput = DrillingDirection.NONE;
    }

    void Update()
    {
        updateJoystick();
        updateJoystickImages();
    }

    void UpdateVirtualAxes(Vector3 value)
    {
        var delta = m_StartPos - value;
        delta.y = -delta.y;
        delta /= MovementRange;
        joystickX = -delta.x;
        joystickY = delta.y;
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
            if (CurrentInput == DrillingDirection.DOWN || CurrentInput == DrillingDirection.UP)
            {
                if (m_UseX)
                {
                    int delta = (int)(data.position.x - m_StartPos.x);
                    newPos.x = delta;
                }
            }
            else if (CurrentInput == DrillingDirection.RIGHT || CurrentInput == DrillingDirection.LEFT)
            {
                if (m_UseY)
                {
                    int delta = (int)(data.position.y - m_StartPos.y);
                    newPos.y = delta;
                }
            }
        }
        innerPad.transform.position = Vector3.ClampMagnitude(new Vector3(newPos.x, newPos.y, newPos.z), MovementRange) + m_StartPos;
        UpdateVirtualAxes(innerPad.transform.position);
    }


    public void OnPointerUp(PointerEventData data)
    {
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING ||
            GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            innerPad.transform.position = m_StartPos;
            UpdateVirtualAxes(m_StartPos);
        }
    }


    public void OnPointerDown(PointerEventData data) { }

    void OnDisable()
    {
        
    }

    private void updateJoystick()
    {
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            if (joystickX >= 0.707f) // sin 45 deg
            {
                //right
                if (CurrentInput != DrillingDirection.RIGHT) PrevInput = CurrentInput;
                if (PrevInput != DrillingDirection.LEFT && CurrentInput != DrillingDirection.RIGHT)
                {
                    CurrentInput = DrillingDirection.RIGHT;
                    JustTurned = true;
                }
            }
            else if (joystickX <= -0.707) // sin 45 deg
            {
                //left
                if (CurrentInput != DrillingDirection.LEFT) PrevInput = CurrentInput;
                if (PrevInput != DrillingDirection.RIGHT && CurrentInput != DrillingDirection.LEFT)
                {
                    CurrentInput = DrillingDirection.LEFT;
                    JustTurned = true;
                }
            }
            else if (joystickY >= 0.707f) // cos 45 deg
            {
                //up
                if (CurrentInput != DrillingDirection.UP) PrevInput = CurrentInput;
                if (PrevInput != DrillingDirection.DOWN && CurrentInput != DrillingDirection.UP)
                {
                    CurrentInput = DrillingDirection.UP;
                    JustTurned = true;
                }
            }
            else if (joystickY <= -0.707f) // cos 45 deg
            {
                //down
                if (CurrentInput != DrillingDirection.DOWN) PrevInput = CurrentInput;
                if (PrevInput != DrillingDirection.UP && CurrentInput != DrillingDirection.DOWN)
                {
                    CurrentInput = DrillingDirection.DOWN;
                    JustTurned = true;
                }
            }
        }
        else if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING)
        {
            if (joystickY < -0.707f)
            {
                GameManager.Instance.DrillingGame.MakeDrill(true);
                GameManager.Instance.DrillingGame.Animator.SetBool("shouldJump", true);
            }
        }
    }

    private void updateJoystickImages()
    {
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING) directionArrows.sprite = arrowDown;
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            if (CurrentInput == DrillingDirection.UP || CurrentInput == DrillingDirection.DOWN) directionArrows.sprite = arrowLeftRight;
            else if (CurrentInput == DrillingDirection.LEFT || CurrentInput == DrillingDirection.RIGHT) directionArrows.sprite = arrowUpDown;
        }
        else directionArrows.sprite = arrowDown;
    }
}
