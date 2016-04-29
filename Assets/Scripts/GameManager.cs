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
    
    public int TotalChimneys { get; private set; }
    public float ChimneyValue { get { return 100f/TotalChimneys; } }
    public float PowerOutput { get; private set; }
    public Player Player { get; private set; }

    void Awake()
    {
        Player = GetComponentInChildren<Player>();

        City[] cities = FindObjectsOfType<City>();

        for (int i = 0; i < cities.Length; i++)
        {
            TotalChimneys += cities[i].ChimneyCount;
        }
    }

    void Start()
    {
        
    }

    void OnGUI()
    {
        GUI.Label(new Rect(2, 2, 100, 30), "score: " + Player.Score + "/100");
    }

    public void AddPowerOutput(float amount)
    {
        PowerOutput += amount;
    }
}
