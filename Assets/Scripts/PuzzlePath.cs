using UnityEngine;
using System.Collections;
using System.Linq;

public class PuzzlePath : MonoBehaviour
{
    [SerializeField] private Pylon[] pathPylons;

    public int CurrentTarget { get; private set; }
    public bool IsCompleted { get; private set; }

    public void TryConnectPylon(Pylon pylon)
    {
        if (IsCompleted)
            return;

        if (pathPylons.Contains(pylon) && pathPylons[CurrentTarget] == pylon)
        {
            pylon.Build();

            CurrentTarget++;

            if (CurrentTarget >= pathPylons.Length)
            {
                IsCompleted = true;
                Debug.Log("puzzle completed! " + Time.time);
            }
        }
    }
}
