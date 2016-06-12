using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class GeoThermalPlant : MonoBehaviour
{
    public enum States
    {
        Ready,
        Built
    }

    [SerializeField] private GameObject Model;

    public States State { get; private set; }

    void Awake()
    {
        Reset();
    }

    public void Build()
    {
        if (State != States.Ready)
            return;

        State = States.Built;

        GameManager.Instance.Director.SetMode(Director.Modes.Grid, transform, 2f);
        //todo: start drilling game
    }

    public void ShowPreview(bool show)
    {
        if (State != States.Ready)
            return;

        if (show)
        {
            Model.SetActive(true);
        }
        else
        {
            Model.SetActive(false);
        }
    }

    public void Reset()
    {
        State = States.Ready;
        Model.SetActive(false);
    }
}
