using UnityEngine;
using System.Collections;

public class Intro : MonoBehaviour
{
    [SerializeField] private float tipDisplayTime = 6f;
    [SerializeField] private float tipDelay;
    [SerializeField] private string[] introTips;
    [SerializeField] private Vector3 focusPoint;
    [SerializeField] private float swoopTime = 4f;
    [SerializeField] private float waitTime = 7f;

    private Vector3 camStartPos;
    private Quaternion camStartRot;
    private int tipIndex;

    void Awake()
    {
        camStartPos = Camera.main.transform.position;
        camStartRot = Camera.main.transform.rotation;
    }

    public void StartIntro()
    {
        if (GameManager.Instance.skipIntro)
        {
            foreach (City city in GameManager.Instance.Cities)
            {
                city.SpawnDirtyCity();
            }

            FinishIntro();
            return;
        }

        GameManager.Instance.Planet.normalSpin = 0;
        SpawnNextCity();
    }

    public void OnTipBubbleClicked()
    {
        SpawnNextCity();
    }

    private void FinishIntro()
    {
        GameManager.Instance.Director.SwoopTo(camStartPos, camStartRot, 40f, 2f);

        GameManager.Instance.StartRound();
        enabled = false;
    }

    private void SpawnNextCity()
    {
        if (tipIndex >= introTips.Length)
        {
			foreach (City city in GameManager.Instance.Cities)
			{
				city.SpawnDirtyCity();
			}

            FinishIntro();
            return;
        }

        foreach (City city in GameManager.Instance.Cities)
        {
            if (city.CityState == CityStates.HIDDEN)
            {
                StartCoroutine(WaitAndSpawnCity(city));
                return;
            }
        }

        FinishIntro();
    }

    private IEnumerator WaitAndSpawnCity(City city)
    {
        Vector3 targetPos = city.transform.position + city.transform.right * focusPoint.x + city.transform.up * focusPoint.y + city.transform.forward * focusPoint.z;

        GameManager.Instance.Director.SwoopTo(targetPos, Quaternion.LookRotation(city.transform.position - targetPos, city.transform.up), 40, swoopTime);
        GameManager.Instance.Hud.ShowTipBubble(introTips[tipIndex], city.transform, tipDisplayTime, OnTipBubbleClicked, waitTime + tipDelay);

        yield return new WaitForSeconds(waitTime);

        city.SpawnDirtyCity();
        tipIndex++;
    }

    private IEnumerator DelayedSpawnNextCity()
    {
        yield return new WaitForSeconds(waitTime);

        SpawnNextCity();
    }
}
