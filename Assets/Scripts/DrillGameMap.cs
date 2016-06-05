using UnityEngine;
using System.Collections;

public class DrillGameMap : MonoBehaviour
{
    public Vector2 Dimmensions { get; private set; }
    public Vector2 TileSize { get; private set; }

    [SerializeField] private DrillingGameTile[] tilePrefabs;

    private RectTransform parentPanel;
    private int[] tileData;
    private DrillingGameTile[,] tiles;

    public void Initialize(RectTransform parentPanel, Vector2 dimmensions, Vector2 tileSize, int[] tileData)
    {
        Dimmensions = dimmensions;
        TileSize = tileSize;
        this.tileData = tileData;
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
