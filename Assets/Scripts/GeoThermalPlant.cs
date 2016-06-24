using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class GeoThermalPlant : Connectable
{
    public enum States
    {
        Ready,
        Built
    }

    [SerializeField] private GameObject Model;
    [SerializeField] private GameObject previewModel;

    public States State { get; private set; }
    public PuzzlePath PuzzlePath { get; private set; }

    void Awake()
    {
        Reset();
    }

    void Update()
    {
        if (previewModel.activeSelf)
        {
            previewModel.transform.Rotate(0, 55f * Time.deltaTime, 0);
        }
    }

    public override void OnConnected()
    {
//        throw new System.NotImplementedException();
    }

    public override void Reset()
    {
        base.Reset();

        State = States.Ready;
        Model.SetActive(false);
        previewModel.SetActive(false);
    }

    public void SetMyPuzzlePath(PuzzlePath puzzlePath)
    {
        PuzzlePath = puzzlePath;
    }

    public void Build()
    {
        if (State != States.Ready)
            return;

        State = States.Built;

        previewModel.SetActive(false);
        Model.SetActive(true);
        Model.GetComponentInChildren<Animator>().SetBool("StartConstruction", true);
    }

    public void ShowPreview(bool show)
    {
        if (State != States.Ready)
            return;

        if (show)
        {
            previewModel.SetActive(true);
        }
        else
        {
            previewModel.SetActive(false);
        }
    }
}
