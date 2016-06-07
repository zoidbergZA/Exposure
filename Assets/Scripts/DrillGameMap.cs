using UnityEngine;
using System.Collections.Generic;

public class DrillGameMap : MonoBehaviour
{
    public Vector2 Dimmensions { get; private set; }
    public Vector2 TileSize { get; private set; }
    public Rect BoundingRect { get; private set; }

    [SerializeField] private DrillingGameTile[] tilePrefabs;
    [SerializeField] private DrillingGameTile[] pipePrefabs;

    private RectTransform parentPanel;
    private int[] tileData;
    private DrillingGameTile[,] tiles;
    private List<DrillingGameTile> tilesList = new List<DrillingGameTile>();
    private List<DrillingGameTile> bottomRow = new List<DrillingGameTile>();
    private List<DrillingGameTile> UIwater = new List<DrillingGameTile>();
    private List<DrillingGameTile> water = new List<DrillingGameTile>();

    public const int TILE_WIDTH = 44, TILE_HEIGHT = 44, MAP_WIDTH = 19, MAP_HEIGHT = 14;

    void Update()
    {
//        float phase = Time.time*0.1f;
//        int x = 2;
//        int y = Mathf.FloorToInt(phase);
//
//        DrillingGameTile tile = GetTileAtCoordinate(x, y);
//        Debug.Log(x + ", " + y + ", " + tile.name);
    }

    public DrillingGameTile GetTileAtCoordinate(int x, int y)
    {
        return tiles[x, y];
    }

    public Vector2 GetTilePivotPosition(int x, int y)
    {
        return  new Vector2(TileSize.x * x + TileSize.x/2f, TileSize.y * Dimmensions.y - TileSize.y * y - TileSize.x / 2f);
    }

    public Vector2 GetCoordinateAt(Vector2 position)
    {
        if (!BoundingRect.Contains(position))
        {
            Debug.LogException(new UnityException("GetCoordinate our of bounds!"));
        }

        Vector2 coord = new Vector2(position.x / TileSize.x, position.y / TileSize.y);
        coord.x = Mathf.FloorToInt(coord.x);
        coord.y = Mathf.FloorToInt(coord.y);

        return coord;
    }

    public void Initialize(RectTransform parentPanel, Vector2 dimmensions, Vector2 tileSize, int[] tileData)
    {
        Dimmensions = dimmensions;
        TileSize = tileSize;
        this.tileData = tileData;
        BoundingRect = new Rect(0, 0, Dimmensions.x * TileSize.x, Dimmensions.y * TileSize.y);
        tiles = new DrillingGameTile[(int)dimmensions.x, (int)dimmensions.y];

        //set parent panel size
        parentPanel.sizeDelta = new Vector2(dimmensions.x * tileSize.x, dimmensions.y * tileSize.y);

        //populate map
        for (int i = 0; i < dimmensions.y; i++)
        {
            for (int j = 0; j < dimmensions.x; j++)
            {
                int id = tileData[((int)dimmensions.x * i) + j];

                if (id >= 0)
                {
                    DrillingGameTile t = tiles[j, i] = Instantiate(tilePrefabs[id]);
                    t.transform.SetParent(parentPanel);
                    t.gameObject.SetActive(true);
                    if (id == 7)
                    {
                        UIwater.Add(t);
                        relocateWaterTiles(UIwater.Count, t, j * (int)tileSize.x, (int)dimmensions.y * (int)tileSize.y - i * (int)tileSize.y);
                    }
                    else
                    {
                        t.GetComponent<RectTransform>().localScale = Vector3.one;
                        t.GetComponent<RectTransform>().anchoredPosition = new Vector2(j * tileSize.x, dimmensions.y * tileSize.y - i * tileSize.y);
                    }
                    tilesList.Add(t);
                    if (id == 4 && j == 14) bottomRow.Add(t);
                }
            }
        }
    }

    public void ClearMap()
    {
        foreach (DrillingGameTile tile in tilesList) Destroy(tile);
        foreach (DrillingGameTile tile in bottomRow) Destroy(tile);
        foreach (DrillingGameTile tile in UIwater) Destroy(tile);
        foreach (DrillingGameTile tile in water) Destroy(tile);
        tilesList.Clear();
        bottomRow.Clear();
        UIwater.Clear();
        water.Clear();
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
                tile.GetComponent<RectTransform>().anchoredPosition = new Vector2(-38, 583);
                LeanTween.move(tile.gameObject.GetComponent<RectTransform>(), new Vector2(x, y), 1.5f).setEase(LeanTweenType.easeOutQuad);
                LeanTween.scale(tile.GetComponent<RectTransform>(), Vector3.one, 1f);
                break;
            case 2:
                tile.GetComponent<RectTransform>().anchoredPosition = new Vector2(-38, 497);
                LeanTween.move(tile.gameObject.GetComponent<RectTransform>(), new Vector2(x, y), 1.5f).setEase(LeanTweenType.easeOutQuad);
                LeanTween.scale(tile.GetComponent<RectTransform>(), Vector3.one, 1f);
                break;
            case 3:
                tile.GetComponent<RectTransform>().anchoredPosition = new Vector2(-38, 409);
                LeanTween.move(tile.gameObject.GetComponent<RectTransform>(), new Vector2(x, y), 1.5f).setEase(LeanTweenType.easeOutQuad);
                LeanTween.scale(tile.GetComponent<RectTransform>(), Vector3.one, 1f);
                break;
        }
    }

    public void instantiatePipe(int x, int y, int pipeId, RectTransform parentPanel, Vector2 tileSize, Vector2 dimentions)
    {
        DrillingGameTile pipe = Instantiate(pipePrefabs[pipeId]);
        pipe.transform.SetParent(parentPanel, false);
        pipe.GetComponent<RectTransform>().anchoredPosition = new Vector2(x * tileSize.x, dimentions.y * tileSize.y - y * tileSize.y);
        pipe.GetComponent<RectTransform>().localScale = Vector3.one;
        pipe.gameObject.SetActive(true);
        tilesList.Add(pipe);
    }
}
