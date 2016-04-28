using System;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(GameManager)) as GameManager;

                if (_instance == null)
                {
                    throw new Exception("no GameManager!");
                }
            }
            return _instance;
        }
    }

    [SerializeField] private float powerGoal = 5f;

    public float PowerOutput { get; private set; }

    void Awake()
    {

    }

    void Start()
    {

    }

    void OnGUI()
    {
        GUI.Label(new Rect(2, 2, 100, 30), PowerOutput + "/" + powerGoal);
    }

    public void AddPowerOutput(float amount)
    {
        PowerOutput += amount;
    }
}
