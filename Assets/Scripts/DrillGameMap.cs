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

    public bool TriggerFlash { get; set; }
    public Vector2 FlashCoords { get; set; }
    public int GetWaterCount { get { return water.Count; } }
    public UnityEngine.UI.Image FlashTile { get { return flashTile; } }

    private GameObject ceiling;
    private GameObject rightWall;
    private GameObject leftWall;
    private RectTransform parentPanel;
    private int[] tileData;
    private DrillingGameTile[,] tiles;
    private List<DrillingGameTile> tilesList = new List<DrillingGameTile>();
    private List<DrillingGameTile> bottomRow = new List<DrillingGameTile>();
    private List<DrillingGameTile> UIwater = new List<DrillingGameTile>();
    private List<DrillingGameTile> water = new List<DrillingGameTile>();
    private List<DrillingGameTile> blockSet_1 = new List<DrillingGameTile>();
    private List<DrillingGameTile> blockSet_2 = new List<DrillingGameTile>();

    public const int TILE_SIZE = 70, MAP_WIDTH = 12, MAP_HEIGHT = 9;

    void Start()
    {
        ceiling = GameObject.Find("Ceiling");
        rightWall = GameObject.Find("Right wall");
        leftWall = GameObject.Find("Left wall");
        flashTile.color = new Color(1, 1, 1, 0);
    }

    void Update()
    {
        updateWallsEnabling();
        checkWaterAndDestroyBottom();
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
        if (!BoundingRect.Contains(position))
        {
            Debug.LogException(new UnityException("GetCoordinate our of bounds!"));
        }

        Vector2 coord = new Vector2(position.x / TILE_SIZE, position.y / TILE_SIZE);
        coord.x = Mathf.FloorToInt(coord.x);
        coord.y = Mathf.FloorToInt(coord.y);

        return coord;
    }

    public void Initialize(RectTransform parentPanel, int[] tileData)
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
                        UIwater.Add(t);
                        relocateWaterTiles(UIwater.Count, t, j * TILE_SIZE, MAP_HEIGHT * TILE_SIZE - i * TILE_SIZE);
                    }
                    else
                    {
                        t.GetComponent<RectTransform>().localScale = Vector3.one;
                        t.GetComponent<RectTransform>().anchoredPosition = new Vector2(j * TILE_SIZE, MAP_HEIGHT * TILE_SIZE - i * TILE_SIZE);
                    }
                    tilesList.Add(t);
                    if (id == 6 && i == 8) bottomRow.Add(t);
                }
            }
        }
    }

    public void Reset()
    {
        foreach (DrillingGameTile tile in tilesList) if(tile != null) Destroy(tile.gameObject);
        foreach (DrillingGameTile tile in bottomRow) if (tile != null) Destroy(tile.gameObject);
        foreach (DrillingGameTile tile in UIwater) if (tile != null) Destroy(tile.gameObject);
        foreach (DrillingGameTile tile in water) if (tile != null) Destroy(tile.gameObject);
        tilesList.Clear();
        bottomRow.Clear();
        UIwater.Clear();
        water.Clear();

        if (ceiling.GetComponent<BoxCollider2D>().enabled) ceiling.GetComponent<BoxCollider2D>().enabled = false;
        if (rightWall.GetComponent<BoxCollider2D>().enabled) rightWall.GetComponent<BoxCollider2D>().enabled = false;
        if (leftWall.GetComponent<BoxCollider2D>().enabled) leftWall.GetComponent<BoxCollider2D>().enabled = false;
    }

    public void AddWater(DrillingGameTile waterPiece)
    {
        water.Add(waterPiece);
    }

    private void relocateWaterTiles(int id, DrillingGameTile tile, int x, int y)
    {
        switch(id)
        {
            case 1:
                tile.GetComponent<RectTransform>().anchoredPosition = new Vector2(-163, 525);
                LeanTween.move(tile.gameObject.GetComponent<RectTransform>(), new Vector2(x, y), 2.5f).setEase(LeanTweenType.easeOutQuad);
                LeanTween.scale(tile.GetComponent<RectTransform>(), Vector3.one, 1f);
                break;
            case 2:
                tile.GetComponent<RectTransform>().anchoredPosition = new Vector2(-163, 436);
                LeanTween.move(tile.gameObject.GetComponent<RectTransform>(), new Vector2(x, y), 2.5f).setEase(LeanTweenType.easeOutQuad);
                LeanTween.scale(tile.GetComponent<RectTransform>(), Vector3.one, 1f);
                break;
            case 3:
                tile.GetComponent<RectTransform>().anchoredPosition = new Vector2(-163, 345);
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
        }
    }

    private void checkWaterAndDestroyBottom()
    {
        if (water.Count == 3) triggerSet(bottomRow);
    }

    private void triggerSet(List<DrillingGameTile> set)
    {
        foreach (DrillingGameTile item in set) if (item != null) Destroy(item.gameObject);
    }

    public void DoFlashTile(Vector2 coords)
    {
        flashTile.color = new Color(1, 1, 1, 1);
        flashTile.rectTransform.anchoredPosition = coords;
        //flashTile.transform.SetAsLastSibling();
    }
}
