using UnityEngine;
using System.Collections;
using Pathfinding;

public class TestSeeker : MonoBehaviour
{
    public Transform start;
    public Transform goal;

    private Seeker seeker;

    void Awake()
    {
        seeker = GetComponent<Seeker>();
    }

	void Start ()
	{
	    seeker.StartPath(transform.position, goal.position, OnPathComplete);
	}
	
	void Update ()
    {
	
	}

    public void OnPathComplete(Path p)
    {
        Debug.Log("Yay, we got a path back. Did it have an error? " + p.error);
        Debug.Log(p.path.Count);
     
    }
}
