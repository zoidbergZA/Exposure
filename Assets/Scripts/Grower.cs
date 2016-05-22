using UnityEngine;
using System.Collections;

public class Grower : MonoBehaviour
{
    private float tweenValue;
    private int growTweenId;
    private MeshRenderer[] meshRenderers;
    private Vector3 normalScale;

    void Awake()
    {
        
    }

	void Start ()
	{
	    growTweenId = LeanTween.value(gameObject, OnTweenUpdate, 0f, 1f, 1.5f).setOnComplete(OnGrowComplete).id;
	}
	
	void Update ()
    {
	    if (LeanTween.isTweening(growTweenId))
	    {
	        foreach (MeshRenderer meshRenderer in meshRenderers)
	        {

	        }
	    }
	}

    private void OnTweenUpdate(float val, float ratio)
    {
        tweenValue = val;
    }

    private void OnGrowComplete()
    {
        
    }
}
