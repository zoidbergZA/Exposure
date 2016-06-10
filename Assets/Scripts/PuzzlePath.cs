using UnityEngine;
using System.Collections;
using System.Linq;

public class PuzzlePath : MonoBehaviour
{
    [SerializeField] private Pylon[] pathPylons;

    public int CurrentTarget { get; private set; }
    public bool IsCompleted { get; private set; }

    public void Reset()
    {
        CurrentTarget = 0;

        for (int i = 0; i < pathPylons.Length; i++)
        {
            pathPylons[i].Reset();
        }
    }

    public void TryConnectPylon(Pylon pylon)
    {
        if (IsCompleted)
            return;

        if (pathPylons.Contains(pylon) && pathPylons[CurrentTarget] == pylon)
        {
            pylon.Build();
            GameManager.Instance.Director.SetMode(Director.Modes.Grid, pylon.transform);

            CurrentTarget++;

            if (CurrentTarget >= pathPylons.Length)
            {
                IsCompleted = true;
                Debug.Log("puzzle completed! " + Time.time);
            }
        }
    }
}
