using UnityEngine;
using System.Collections;

public class Minigame : MonoBehaviour
{
    [SerializeField] private float timeOut = 30f;

    public float Difficulty { get; protected set; }
    public bool IsRunning { get; private set; }
    public float TimeOut { get { return timeOut; } }
    public float Timeleft { get; protected set; }

    public virtual void Update()
    {
        if (IsRunning)
        {
            Timeleft -= Time.deltaTime;

            if (Timeleft <= 0)
                End(false);
        }
    }

    public virtual void Begin(float difficulty)
    {
        Difficulty = difficulty;
        IsRunning = true;
        Timeleft = timeOut;
        //Debug.Log(name + " minigame started, Difficulty: " + Difficulty);
    }

    public virtual void End(bool succeeded)
    {
        IsRunning = false;
        //Debug.Log(name + " minigame ended at: " + Time.time + "result: " + succeeded);
    }
}
