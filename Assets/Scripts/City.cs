using UnityEngine;
using System.Collections;

public class City : MonoBehaviour
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
                GameManager.Instance.Player.ScorePoints(GameManager.Instance.ChimneyValue);
                return;
            }
        }
    }
}
