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

    [SerializeField] private Animation chimneyAnimation;
    [SerializeField] private Texture2D demolishIcon;
    [SerializeField] private ParticleSystem smokeSystem;
    [SerializeField] private ParticleSystem explodeSystem;
    [SerializeField] private MeshRenderer unusedModel;

    public ChimneyStates ChimneyState { get; private set; }

    void Awake()
    {
        ChimneyState = ChimneyStates.Working;
    }

    void Start()
    {
//        chimneyAnimation.Play();
    }

    public void DisableChimney()
    {
        if (ChimneyState != ChimneyStates.Working)
            return;

        ChimneyState = ChimneyStates.Unused;
        smokeSystem.enableEmission = false;
        GameManager.Instance.Planet.RefreshHealth();
    }

    public void Demolish()
    {
        if (ChimneyState != ChimneyStates.Unused)
            return;

        ChimneyState = ChimneyStates.Exploded;
        unusedModel.enabled = false;

        explodeSystem.Play();
        chimneyAnimation.Play();
        GameManager.Instance.Player.ScorePoints(10, transform);
        GameManager.Instance.Director.Shake();
    }

    void OnGUI()
    {
        if (ChimneyState == ChimneyStates.Unused)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            GameManager.Instance.Hud.ShowWorldSpaceButton(demolishIcon, screenPos, Demolish);
        }
    }
}
