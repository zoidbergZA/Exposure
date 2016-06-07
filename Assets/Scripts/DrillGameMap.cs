﻿using UnityEngine;
using System.Collections.Generic;

public class DrillGameMap : MonoBehaviour
{
    public Rect BoundingRect { get; private set; }

    [SerializeField] private DrillingGameTile[] tilePrefabs;
    [SerializeField] private DrillingGameTile[] pipePrefabs;
    [SerializeField] private float flashTileTime = 1.0f;

    public bool TriggerFlash { get; set; }
    public Vector2 FlashCoords { get; set; }

    private GameObject ceiling;
    private GameObject rightWall;
    private GameObject leftWall;
    private RectTransform parentPanel;
    private UnityEngine.UI.Image flashTile;
    private float flashTileTimer;
    private int[] tileData;
    private DrillingGameTile[,] tiles;
    private List<DrillingGameTile> tilesList = new List<DrillingGameTile>();
    private List<DrillingGameTile> bottomRow = new List<DrillingGameTile>();
    private List<DrillingGameTile> UIwater = new List<DrillingGameTile>();
    private List<DrillingGameTile> water = new List<DrillingGameTile>();

    public const int TILE_WIDTH = 44, TILE_HEIGHT = 44, MAP_WIDTH = 19, MAP_HEIGHT = 14;

    void Start()
    {
        ceiling = GameObject.Find("Ceiling");
        rightWall = GameObject.Find("Right wall");
        leftWall = GameObject.Find("Left wall");
        if (tilePrefabs[14]) flashTile = tilePrefabs[14].GetComponent<UnityEngine.UI.Image>();
        flashTileTimer = flashTileTime;
    }

    void Update()
    {
        updateWallsEnabling();
        if (TriggerFlash) FlashNextTile();
    }

    public DrillingGameTile GetTileAtCoordinate(int x, int y)
    {
        return tiles[x, y];
    }

    public Vector2 GetTilePivotPosition(int x, int y)
    {
        return new Vector2(TILE_WIDTH * x + TILE_WIDTH / 2f, TILE_HEIGHT * MAP_HEIGHT - TILE_HEIGHT * y - TILE_WIDTH / 2f);
    }

    public Vector2 GetCoordinateAt(Vector2 position)
    {
        if (!BoundingRect.Contains(position))
        {
            Debug.LogException(new UnityException("GetCoordinate our of bounds!"));
        }

        Vector2 coord = new Vector2(position.x / TILE_WIDTH, position.y / TILE_HEIGHT);
        coord.x = Mathf.FloorToInt(coord.x);
        coord.y = Mathf.FloorToInt(coord.y);

        return coord;
    }

    public void Initialize(RectTransform parentPanel, int[] tileData)
    {
        this.tileData = tileData;
        BoundingRect = new Rect(0, 0, MAP_WIDTH * TILE_WIDTH, MAP_HEIGHT * TILE_HEIGHT);
        tiles = new DrillingGameTile[MAP_WIDTH, MAP_HEIGHT];

        //set parent panel size
        parentPanel.sizeDelta = new Vector2(MAP_WIDTH * TILE_WIDTH, MAP_HEIGHT * TILE_HEIGHT);

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
                    if (id == 7)
                    {
                        UIwater.Add(t);
                        relocateWaterTiles(UIwater.Count, t, j * TILE_WIDTH, MAP_HEIGHT * TILE_HEIGHT - i * TILE_HEIGHT);
                    }
                    else
                    {
                        t.GetComponent<RectTransform>().localScale = Vector3.one;
                        t.GetComponent<RectTransform>().anchoredPosition = new Vector2(j * TILE_WIDTH, MAP_HEIGHT * TILE_HEIGHT - i * TILE_HEIGHT);
                    }
                    tilesList.Add(t);
                    if (id == 4 && j == 14) bottomRow.Add(t);
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

    public void instantiatePipe(int x, int y, int pipeId, RectTransform parentPanel)
    {
        DrillingGameTile pipe = Instantiate(pipePrefabs[pipeId]);
        pipe.transform.SetParent(parentPanel, false);
        pipe.GetComponent<RectTransform>().anchoredPosition = new Vector2(x * TILE_WIDTH, MAP_HEIGHT * TILE_HEIGHT - y * TILE_HEIGHT);
        pipe.GetComponent<RectTransform>().localScale = Vector3.one;
        pipe.gameObject.SetActive(true);
        tilesList.Add(pipe);
    }

    private void updateWallsEnabling()
    {
        if (GameManager.Instance.DrillingGame.Driller.Drill.rectTransform.anchoredPosition.y <= (MAP_HEIGHT * TILE_HEIGHT) - TILE_HEIGHT)
        {
            if (!ceiling.GetComponent<BoxCollider2D>().enabled) ceiling.GetComponent<BoxCollider2D>().enabled = true;
            if (!rightWall.GetComponent<BoxCollider2D>().enabled) rightWall.GetComponent<BoxCollider2D>().enabled = true;
            if (!leftWall.GetComponent<BoxCollider2D>().enabled) leftWall.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    private void FlashNextTile()
    {
        flashTileTimer -= Time.deltaTime;
        flashTile.rectTransform.anchoredPosition = FlashCoords;
        flashTile.transform.SetAsLastSibling();
        flashTile.enabled = true;
        if (flashTileTimer <= 0)
        {
            flashTileTimer = flashTileTime;
            flashTile.transform.SetAsFirstSibling();
            flashTile.enabled = false;
            TriggerFlash = false;
        }
    }
}
