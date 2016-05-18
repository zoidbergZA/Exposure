using UnityEngine;
using System.Collections;

public class Chimney : MonoBehaviour
{
    public enum ChimneyStates
    {
        Working,
        Unused,
        Exploded
    }

    [SerializeField] private Texture2D demolishIcon;
    [SerializeField] private ParticleSystem smokeSystem;
    [SerializeField] private MeshRenderer unusedModel;

    public ChimneyStates ChimneyState { get; private set; }

    void Awake()
    {
        ChimneyState = ChimneyStates.Working;
    }

    public void DisableChimney()
    {
        if (ChimneyState != ChimneyStates.Working)
            return;

        ChimneyState = ChimneyStates.Unused;
        smokeSystem.enableEmission = false;
    }

    void OnGUI()
    {
        if (ChimneyState == ChimneyStates.Unused)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

            if (GUI.Button(new Rect(screenPos.x - 20, Screen.height - screenPos.y - 20, 40, 40), demolishIcon, ""))
            {
                Demolish();
            }
        }
    }

    private void Demolish()
    {
        if (ChimneyState != ChimneyStates.Unused)
            return;

        ChimneyState = ChimneyStates.Exploded;
        unusedModel.enabled = false;

        GameManager.Instance.Player.ScorePoints(10, transform);
    }
}
