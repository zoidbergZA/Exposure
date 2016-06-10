using UnityEngine;
using System.Collections;

public class City : MonoBehaviour
{
    [SerializeField] private GameObject dirtyModel;
    [SerializeField] private GameObject cleanModel;
    [SerializeField] private PuzzlePath puzzlePath;

    public bool IsDirty { get; private set; }

    void Awake()
    {
        if (puzzlePath)
            puzzlePath.SetParentCity(this);

        dirtyModel.SetActive(true);
        cleanModel.SetActive(false);

        IsDirty = true;
    }

    void Start()
    {

    }

    public void CleanUp()
    {
        if (!IsDirty)
            return;

        IsDirty = false;

        dirtyModel.SetActive(false);
        cleanModel.SetActive(true);

        GameManager.Instance.Scanner.DeselectCity();
    }

    public void Reset()
    {
        puzzlePath.Reset();
    }

    public void TryBuild(Pylon pylon)
    {
        puzzlePath.TryConnectPylon(pylon);
    }
}
