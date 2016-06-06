using UnityEngine;
using System.Collections;

public class Tree : MonoBehaviour
{
    [SerializeField] private GameObject model;

    void Awake()
    {

    }

    public void SetUngrown()
    {
        model.transform.localScale = new Vector3(1, 0, 1);
        model.SetActive(false);
    }

    public void Grow()
    {
        model.SetActive(true);
        model.transform.localScale = new Vector3(1, 1, 1);
    }
}
