using UnityEngine;
using System.Collections;

public class City : Connectable
{
    [SerializeField] private Chimney[] chimneys;

    public int ChimneyCount { get { return chimneys.Length; } }

    public bool HasWorkingChimney
    {
        get
        {
            for (int i = 0; i < chimneys.Length; i++)
            {
                if (chimneys[i].ChimneyState == Chimney.ChimneyStates.Working)
                    return true;
            }
            return false;
        }
    }

    public void DisableChimney()
    {
        for (int i = 0; i < ChimneyCount; i++)
        {
            if (chimneys[i] != null && chimneys[i].ChimneyState == Chimney.ChimneyStates.Working)
            {
                chimneys[i].DisableChimney();
                return;
            }
        }
    }

    public override void Start()
    {
        base.Start();

        //test floating text
//        GameManager.Instance.Hud.NewFloatingText("hello!", transform);
    }

    public override void CheckConnectable(Vector3 location)
    {
        float dist = Vector3.Distance(transform.position, location);

        if (HasWorkingChimney && dist <= GameManager.Instance.PylonSeparation * 1.3f)
        {
            IsConnectable = true;
            Highlight(true);
        }
        else
        {
            IsConnectable = false;
            Highlight(false);
        }
    }

    public override void Highlight(bool hightlight)
    {
        if (hightlight)
        {
//            GetComponent<MeshRenderer>().material.color = Color.green;
        }
        else
        {
//            GetComponent<MeshRenderer>().material.color = Color.white;
        }
    }

    public override void OnConnected()
    {
        DisableChimney();
    }
}
