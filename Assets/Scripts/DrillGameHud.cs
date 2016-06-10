using UnityEngine;
using System.Collections;

public class DrillGameHud : MonoBehaviour
{
    [SerializeField] private float joystickArrowFadeSpeed = 2f;
    [SerializeField] private float toastMessageTime = 3.0f;
    [SerializeField] private float panelSlidingTime = 1.5f;
    [SerializeField] private UnityEngine.UI.Image joystickArrow;
    [SerializeField] private UnityEngine.UI.Image endOkToast;
    [SerializeField] private UnityEngine.UI.Image brokenDrillToast;
    [SerializeField] private UnityEngine.UI.Image brokenPipeToast;
    [SerializeField] private UnityEngine.UI.Image explodeBombToast;
    [SerializeField] private UnityEngine.UI.Image waterBar;
    [SerializeField] private UnityEngine.UI.Image steamImage;
    [SerializeField] private UnityEngine.UI.Image drillLife;

    public UnityEngine.UI.Image JoystickArrow { get { return joystickArrow; } }
    public UnityEngine.UI.Image EndOkToast { get { return endOkToast; } }
    public UnityEngine.UI.Image BrokenDrillToast { get { return brokenDrillToast; } }
    public UnityEngine.UI.Image BrokenPipeToast { get { return brokenPipeToast; } }
    public UnityEngine.UI.Image WaterBar { get { return waterBar; } }
    public UnityEngine.UI.Image SteamImage { get { return steamImage; } }
    public UnityEngine.UI.Image DrillLife { get { return drillLife; } }
    public UnityEngine.UI.Image ExplodeBombToast { get { return explodeBombToast; } }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
