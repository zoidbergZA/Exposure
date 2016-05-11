using UnityEngine;
using System.Collections;

public class City : Connectable
{
    [SerializeField] private GameObject[] chimneys;

    public int ChimneyCount { get { return chimneys.Length; } }

    public void DisableChimney()
    {
        for (int i = 0; i < ChimneyCount; i++)
        {
            if (chimneys[i] != null && chimneys[i].activeSelf)
            {
                chimneys[i].SetActive(false);
                return;
            }
        }
    }

    public override void CheckConnectable(Vector3 location)
    {
        float dist = Vector3.Distance(transform.position, location);

        if (dist <= GameManager.Instance.GridBuilder.JumpDistance)
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
            GetComponent<MeshRenderer>().material.color = Color.green;
        }
        else
        {
            GetComponent<MeshRenderer>().material.color = Color.white;
        }
    }

    public override void OnConnected()
    {
        DisableChimney();
    }
}
