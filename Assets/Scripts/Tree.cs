using UnityEngine;
using System.Collections;

public class Tree : MonoBehaviour
{
    [SerializeField] private GameObject model;
    [SerializeField] private float growTime = 8f;

    private float growTimer;
    private float growTimeLeft;
    private bool isGrowing;

    void Awake()
    {
        
    }

    void Update()
    {
        if (isGrowing && growTimeLeft > 0)
        {
            float progress = 1f - growTimeLeft/growTimer;
            model.transform.localScale = new Vector3(1, progress, 1);

            growTimeLeft -= Time.deltaTime;

            if (growTimeLeft <= 0)
            {
                model.transform.localScale = new Vector3(1, 1, 1);
                isGrowing = false;
            }
        }
    }

    public void SetUngrown()
    {
        //randomize grow time
        growTimer = growTime + Random.Range(-growTime / 2f, growTime / 2f);

        model.transform.localScale = new Vector3(1, 0, 1);
        model.SetActive(false);
        isGrowing = false;
    }

    public void Grow()
    {
        model.SetActive(true);
        isGrowing = true;
        growTimeLeft = growTimer;
    }
}
