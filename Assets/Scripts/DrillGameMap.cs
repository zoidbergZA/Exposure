using UnityEngine;
using System.Collections;

public class DrillGameMap : MonoBehaviour
{
    public Vector2 Dimmensions { get; private set; }
    public Vector2 TileSize { get; private set; }
    public Rect BoundingRect { get; private set; }

    [SerializeField] private DrillingGameTile[] tilePrefabs;

    private RectTransform parentPanel;
    private int[] tileData;
    private DrillingGameTile[,] tiles;

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
//                    Debug.Log(new Vector2(j, i) + " " + id);

                    DrillingGameTile t = tiles[j, i] = Instantiate(tilePrefabs[id]);
                    t.transform.SetParent(parentPanel);
                    t.GetComponent<RectTransform>().localScale = Vector3.one;
                    t.GetComponent<RectTransform>().anchoredPosition = new Vector2(j * tileSize.x, dimmensions.y * tileSize.y - i * tileSize.y);
                    t.gameObject.SetActive(true);
                }
            }
        }
    }
}
