using UnityEngine;
using System.Collections;

public class DrillGameMap
{
    public Vector2 Dimmensions { get; private set; }
    public Vector2 TileSize { get; private set; }

    private int[] tileData;

    public DrillGameMap(Vector2 dimmensions, Vector2 tileSize, int[] tileData)
    {
        Dimmensions = dimmensions;
        TileSize = tileSize;
        this.tileData = tileData;
    }
}
