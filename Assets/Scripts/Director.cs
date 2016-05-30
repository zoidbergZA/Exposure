using UnityEngine;
using System.Collections;

public class Director : MonoBehaviour
{
    public enum Modes
    {
        Orbit,
        Grid
    }

    public float buildHeight = 100f;

    [SerializeField] private Shaker shaker;

    private Vector3 orbitPosition;
    private Quaternion orbitRotation;
    private float orbitHeight;

    //tweener values
    private Vector3 targetPosition;
    private Quaternion fromRotation;
    private Quaternion targetRotation;
    private float rotateProgress;
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
	    transform.position = targetPosition;
	    Camera.main.fieldOfView = targetFoV;

        transform.rotation = Quaternion.Slerp(fromRotation, targetRotation, rotateProgress);
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

    public void SwoopTo(Vector3 position, Quaternion rotation, float fov, float time, float delay = 0f)
    {
        //todo: rotation lerp

        //        Quaternion deltaQuaternion = Quaternion.Inverse(targetRotation) * transform.rotation;
        //        float angle = 0;
        //        Vector3 axis = Vector3.zero;
        //
        //        deltaQuaternion.ToAngleAxis(out angle, out axis);
        //
        //        Debug.Log(angle + ", " + axis);
        //
        //        LeanTween.rotateAround(gameObject, axis, angle, time);

        fromRotation = transform.rotation;
        targetRotation = rotation;
        rotateProgress = 0f;

        LeanTween.value(gameObject, updatePosCallback, targetPosition, position, time);
        LeanTween.value(gameObject, updateFOVCallback, targetFoV, fov, time).setEase(LeanTweenType.easeInOutSine);
    }

    void updatePosCallback(Vector3 val)
    {
        targetPosition = val;
    }

    void updateFOVCallback(float val, float ratio)
    {
        targetFoV = val;
        rotateProgress = ratio;
    }
}
