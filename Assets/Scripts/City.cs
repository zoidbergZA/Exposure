using UnityEngine;
using System.Collections;

public class City : Connectable
{
    [SerializeField] private GameObject dirtyModel;
    [SerializeField] private GameObject cleanModel;
    [SerializeField] private PuzzlePath puzzlePath;

    public bool IsDirty { get; private set; }
    public PuzzlePath PuzzlePath { get { return puzzlePath; } }

    void Awake()
    {
        Reset();
    }

    void Start()
    {

    }

    public override void OnConnected()
    {
        CleanUp();
    }

    public override void Reset()
    {
        base.Reset();

//        if(puzzlePath)
//            puzzlePath.Reset();

        dirtyModel.SetActive(true);
        cleanModel.SetActive(false);

        IsDirty = true;
    }

    public void CleanUp()
    {
        if (!IsDirty)
            return;

        IsDirty = false;

        dirtyModel.SetActive(false);
        cleanModel.SetActive(true);

        ConnectionState = ConnectionStates.Built;
    }
}
