using UnityEngine;
using System.Collections;

public class City : MonoBehaviour
{
    [SerializeField] private GameObject dirtyModel;
    [SerializeField] private GameObject cleanModel;
    [SerializeField] private PuzzlePath puzzlePath;

    public bool IsDirty { get; private set; }
    public PuzzlePath PuzzlePath { get { return puzzlePath; } }

    void Awake()
    {
        if (puzzlePath)
            puzzlePath.SetParentCity(this);

        Reset();
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
    }

    public void Reset()
    {
        if(puzzlePath)
            puzzlePath.Reset();

        dirtyModel.SetActive(true);
        cleanModel.SetActive(false);

        IsDirty = true;
    }
}
