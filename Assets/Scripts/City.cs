using UnityEngine;
using System.Collections;

public class City : Connectable
{
    [SerializeField] private GameObject dirtyModel;
    [SerializeField] private GameObject cleanModel;
    [SerializeField] private GameObject nukeEffect;
    [SerializeField] private float nukeEffectDuration = 3f;
    [SerializeField] private PuzzlePath puzzlePath;

    public bool IsDirty { get; private set; }
    public PuzzlePath PuzzlePath { get { return puzzlePath; } }
    public CityIcon CityIcon { get; set; }

    void Awake()
    {
        Reset();
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            CleanUp();
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

        nukeEffect.SetActive(true);
        GameManager.Instance.Director.Shake();
        ConnectionState = ConnectionStates.Built;
        CityIcon.ToggleIcon(true);

        StartCoroutine(SpawnCleanCity(nukeEffectDuration));

        GameManager.Instance.Player.ScorePoints(PointValue, transform);
        GameManager.Instance.Planet.RefreshHealth();
        GameManager.Instance.Hud.CitiesBar.SortIcons();
    }

    private IEnumerator SpawnCleanCity(float delay)
    {
        yield return new WaitForSeconds(delay);

        dirtyModel.SetActive(false);
        cleanModel.SetActive(true);
        GetComponentInChildren<Animation>().Play("NewCityRise");
    }
}
