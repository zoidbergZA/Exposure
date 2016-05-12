using UnityEngine;
using System.Collections;

public class Minigame : MonoBehaviour
{
    [SerializeField] private float timeOut = 30f;

    public bool IsRunning { get; private set; }
    public float TimeOut { get { return timeOut; } }
    public float Timeleft { get; private set; }

    public virtual void Update()
    {
        if (IsRunning)
        {
            Timeleft -= Time.deltaTime;

            if (Timeleft <= 0)
                End(false);
        }
    }

    public virtual void Begin()
    {
        IsRunning = true;
        Timeleft = timeOut;
        Debug.Log(name + " minigame started at: " + Time.time);
    }

    public virtual void End(bool succeeded)
    {
        IsRunning = false;
        Debug.Log(name + " minigame ended at: " + Time.time + "result: " + succeeded);
    }
}
