using UnityEngine;
using System.Collections.Generic;
using System.IO;
using LitJson;

public class DrillGameMap : MonoBehaviour
{
    public Rect BoundingRect { get; private set; }

    [SerializeField] private DrillingGameTile[] tilePrefabs;
    [SerializeField] private DrillingGameTile[] pipePrefabs;
    [SerializeField] private UnityEngine.UI.Image flashTile;
    [SerializeField] private float flashFadeSpeed = 3.0f;
    [SerializeField] private Sprite crackedBlock;

    public bool TriggerFlash { get; set; }
    public Vector2 FlashCoords { get; set; }
    public int GetPipePartsCount { get { return pipeParts.Count; } }
    public UnityEngine.UI.Image FlashTile { get { return flashTile; } }

    private GameObject ceiling;
    private GameObject rightWall;
    private GameObject leftWall;
    private GameObject floor;
    private RectTransform parentPanel;
    private int[] tileData;
    private DrillingGameTile[,] tiles;
    private List<DrillingGameTile> tilesList = new List<DrillingGameTile>();
    private List<DrillingGameTile> toBeExploded = new List<DrillingGameTile>();
    private List<DrillingGameTile> UIpipeParts = new List<DrillingGameTile>();
    private List<DrillingGameTile> pipeParts = new List<DrillingGameTile>();
    //vars for JSON parser
    private string jsonString;
    private JsonData itemData;

    public const int TILE_SIZE = 71, MAP_WIDTH = 13, MAP_HEIGHT = 8;

    void Start()
    {
        ceiling = GameObject.Find("Ceiling");
        rightWall = GameObject.Find("Right wall");
        leftWall = GameObject.Find("Left wall");
        floor = GameObject.Find("Floor");
        flashTile.color = new Color(1, 1, 1, 0);
        jsonString = File.ReadAllText(Application.dataPath + "/DrillingGameMaps/JSONTest.json");
        itemData = JsonMapper.ToObject(jsonString);
    }

    void Update()
    {
        updateWallsEnabling();
        processWaterProgression();
        if (flashTile.color.a > 0) flashTile.color = new Color(1, 1, 1, flashTile.color.a - Time.deltaTime * flashFadeSpeed);
    }

    public DrillingGameTile GetTileAtCoordinate(int x, int y)
    {
        return tiles[x, y];
    }

    public Vector2 GetTilePivotPosition(int x, int y)
    {
        return new Vector2(TILE_SIZE * x + TILE_SIZE / 2f, TILE_SIZE * MAP_HEIGHT - TILE_SIZE * y - TILE_SIZE / 2f);
    }

    public Vector2 GetCoordinateAt(Vector2 position)
    {
        //if (!BoundingRect.Contains(position))
        //{
        //    Debug.LogException(new UnityException("GetCoordinate our of bounds!"));
        //}

        Vector2 coord = new Vector2(position.x / TILE_SIZE, -position.y / TILE_SIZE);
        coord.x = Mathf.FloorToInt(coord.x);
        coord.y = Mathf.FloorToInt(coord.y);

        return coord;
    }

    public void Initialize(RectTransform parentPanel, int[] tileData, TextAsset properties)
    {
        this.tileData = tileData;
        BoundingRect = new Rect(0, 0, MAP_WIDTH * TILE_SIZE, MAP_HEIGHT * TILE_SIZE);
        tiles = new DrillingGameTile[MAP_WIDTH, MAP_HEIGHT];

        //set parent panel size
        parentPanel.sizeDelta = new Vector2(MAP_WIDTH * TILE_SIZE, MAP_HEIGHT * TILE_SIZE);

        //populate map
        for (int i = 0; i < MAP_HEIGHT; i++)
        {
            for (int j = 0; j < MAP_WIDTH; j++)
            {
                int id = tileData[(MAP_WIDTH * i) + j];

                if (id >= 0)
                {
                    DrillingGameTile t = tiles[j, i] = Instantiate(tilePrefabs[id]);
                    t.transform.SetParent(parentPanel);
                    t.gameObject.SetActive(true);
                    if (id == 9)
                    {
                        UIpipeParts.Add(t);
                        relocatePipeTiles(UIpipeParts.Count, t, j * TILE_SIZE, MAP_HEIGHT * TILE_SIZE - i * TILE_SIZE);
                    }
                    else
                    {
                        t.GetComponent<RectTransform>().localScale = Vector3.one;
                        t.GetComponent<RectTransform>().anchoredPosition = new Vector2(j * TILE_SIZE, MAP_HEIGHT * TILE_SIZE - i * TILE_SIZE);
                    }
                    tilesList.Add(t);
                    if (id == 6) toBeExploded.Add(t);
                }
            }
        }
    }

    public void Reset()
    {
        foreach (DrillingGameTile tile in tilesList) if(tile != null) Destroy(tile.gameObject);
        foreach (DrillingGameTile tile in toBeExploded) if (tile != null) Destroy(tile.gameObject);
        foreach (DrillingGameTile tile in UIpipeParts) if (tile != null) Destroy(tile.gameObject);
        foreach (DrillingGameTile tile in pipeParts) if (tile != null) Destroy(tile.gameObject);
        tilesList.Clear();
        toBeExploded.Clear();
        UIpipeParts.Clear();
        pipeParts.Clear();

        if (ceiling.GetComponent<BoxCollider2D>().enabled) ceiling.GetComponent<BoxCollider2D>().enabled = false;
        if (rightWall.GetComponent<BoxCollider2D>().enabled) rightWall.GetComponent<BoxCollider2D>().enabled = false;
        if (leftWall.GetComponent<BoxCollider2D>().enabled) leftWall.GetComponent<BoxCollider2D>().enabled = false;
        if (floor.GetComponent<BoxCollider2D>().enabled) floor.GetComponent<BoxCollider2D>().enabled = false;
    }

    public void AddPipePart(DrillingGameTile pipePiece)
    {
        pipeParts.Add(pipePiece);
    }

    private void relocatePipeTiles(int id, DrillingGameTile tile, int x, int y)
    {
        switch(id)
        {
            case 1:
                tile.GetComponent<RectTransform>().anchoredPosition = new Vector2(355, -66);
                LeanTween.move(tile.gameObject.GetComponent<RectTransform>(), new Vector2(x, y), 2.5f).setEase(LeanTweenType.easeOutQuad);
                LeanTween.scale(tile.GetComponent<RectTransform>(), Vector3.one, 1f);
                break;
            case 2:
                tile.GetComponent<RectTransform>().anchoredPosition = new Vector2(426, -66);
                LeanTween.move(tile.gameObject.GetComponent<RectTransform>(), new Vector2(x, y), 2.5f).setEase(LeanTweenType.easeOutQuad);
                LeanTween.scale(tile.GetComponent<RectTransform>(), Vector3.one, 1f);
                break;
            case 3:
                tile.GetComponent<RectTransform>().anchoredPosition = new Vector2(489, -66);
                LeanTween.move(tile.gameObject.GetComponent<RectTransform>(), new Vector2(x, y), 2.5f).setEase(LeanTweenType.easeOutQuad);
                LeanTween.scale(tile.GetComponent<RectTransform>(), Vector3.one, 1f);
                break;
        }
    }

    public void instantiatePipe(int x, int y, int pipeId, RectTransform parentPanel)
    {
        DrillingGameTile pipe = Instantiate(pipePrefabs[pipeId]);
        pipe.transform.SetParent(parentPanel, false);
        pipe.GetComponent<RectTransform>().anchoredPosition = new Vector2(x * TILE_SIZE, MAP_HEIGHT * TILE_SIZE - y * TILE_SIZE);
        pipe.GetComponent<RectTransform>().localScale = Vector3.one;
        pipe.gameObject.SetActive(true);
        tilesList.Add(pipe);
    }

    private void updateWallsEnabling()
    {
        if (GameManager.Instance.DrillingGame.Driller.Position.y <= -TILE_SIZE)
        {
            if (!ceiling.GetComponent<BoxCollider2D>().enabled) ceiling.GetComponent<BoxCollider2D>().enabled = true;
            if (!rightWall.GetComponent<BoxCollider2D>().enabled) rightWall.GetComponent<BoxCollider2D>().enabled = true;
            if (!leftWall.GetComponent<BoxCollider2D>().enabled) leftWall.GetComponent<BoxCollider2D>().enabled = true;
            if (!floor.GetComponent<BoxCollider2D>().enabled) floor.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    private void processWaterProgression()
    {
        if (pipeParts.Count == 1)
        {
            foreach (DrillingGameTile tile in toBeExploded)
            {
                tile.GetComponent<UnityEngine.UI.Image>().sprite = crackedBlock;
                LeanTween.scale(tile.GetComponent<RectTransform>(), tile.GetComponent<RectTransform>().localScale * 1.1f, 0.8f)
                        .setEase(LeanTweenType.punch);
            }
        }
        else if (pipeParts.Count == 2)
        {
            triggerSetDestruction(toBeExploded);
        }
    }

    private void triggerSetDestruction(List<DrillingGameTile> set)
    {
        foreach (DrillingGameTile item in set) if (item != null) Destroy(item.gameObject);
        set.Clear();
    }

    public void DoFlashTile(Vector2 coords)
    {
        flashTile.color = new Color(1, 1, 1, 1);
        flashTile.rectTransform.anchoredPosition = coords;
    }
}
