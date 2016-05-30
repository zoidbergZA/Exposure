using UnityEngine;
using System.Collections;

public class Director : MonoBehaviour
{
    public enum Modes
    {
        Orbit,
        Grid
    }

    [SerializeField] private Shaker shaker;
    [SerializeField] private float buildHeight = 100f;

//    private Transform targetTransform;

    private Vector3 orbitPosition;
    private Quaternion orbitRotation;
    private float orbitHeight;

    //tweener values
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float targetFoV;

    public bool OrbitPaused { get; set; }

    public Modes Mode { get; private set; }

    void Awake()
    {
        orbitPosition = transform.position;
        orbitRotation = transform.rotation;
        targetFoV = Camera.main.fieldOfView;
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

	void Start ()
	{
        orbitHeight = Vector3.Distance(transform.position, GameManager.Instance.PlanetTransform.position);
    }
	
	void Update ()
    {
	    Camera.main.fieldOfView = targetFoV;
    }

    public void SetMode(Modes mode, Transform targetTransform, float delay = 2f)
    {
//        if (delay > 0)
//            StartCoroutine(DelayedStart(mode, targetTransform, delay));
//        else
            SwitchMode(mode, targetTransform);
    }

    public void Shake()
    {
        shaker.Shake();
    }

    private IEnumerator DelayedStart(Modes newMode, Transform newTargetTransform, float delay)
    {
        yield return new WaitForSeconds(delay);

        SwitchMode(newMode, newTargetTransform);
    }

    private void SwitchMode(Modes newMode, Transform targetTransform)
    {
        Mode = newMode;

        switch (Mode)
        {
            case Modes.Grid:
                GameManager.Instance.Planet.IsSpinning = false;

                Vector3 newPos = targetTransform.position + targetTransform.up * buildHeight;
                Quaternion newRot = Quaternion.LookRotation(targetTransform.position - newPos, Vector3.up);

                SwoopTo(newPos, newRot, 20f, 2f);
                break;

            case Modes.Orbit:
                GameManager.Instance.Planet.IsSpinning = true;

                SwoopTo(orbitPosition, orbitRotation, 40f, 2f);
                break;
        }
    }

    private void SwoopTo(Vector3 position, Quaternion rotation, float fov, float time, float delay = 0f)
    {
        LeanTween.value(gameObject, updateFOVCallback, targetFoV, fov, time).setEase(LeanTweenType.easeInOutSine);
    }

    void updateFOVCallback(float val, float ratio)
    {
        targetFoV = val;
    }
}
