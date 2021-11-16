using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
using AStarSharp;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    [Serializable]
    public class Count
    {
        public int minimum;
        public int maximum;

        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    public int columns;
    public int rows;
    public Count objectCount;
    public Count enemyCount;
    public Count buildingCount;
    public Count buildingLength;

    public GameObject[] players;
    public GameObject[] enemies;
    public GameObject[] trees;
    public GameObject[] natureObjects;
    public GameObject[] streetObjects;

    public GameObject wall;
    // public GameObject roof;
    public GameObject basicDoor;
    public GameObject keyDoor;
    public GameObject triggerDoor;
    public GameObject[] storage;
    public GameObject[] shelves;
    public GameObject[] bar;
    public GameObject table;
    public GameObject chairFront;
    public GameObject chairBack;
    public GameObject chairLeft;
    public GameObject chairRight;
    public GameObject bed;
    public GameObject stool;
    public GameObject stove;

    public Tilemap groundTilemap;
    public Tilemap roofTilemap;
    public RuleTile dirtTile;
    public RuleTile grassTile;
    public RuleTile roofTile;
    public RuleTile floorTile;

    public Dictionary<String, String[]> roomTypes = new Dictionary<String, String[]>
    {
        {"Bar", new []{"Storage"} },
        {"House", new []{"Bedroom", "Storage"} }
    };
    public List<Vector3> spawnPositions = new List<Vector3>();

    private List<Vector3> gridPositions = new List<Vector3>();
    private List<List<Node>> Grid;
    // private Grid tileGrid;
    // private Tilemap groundTilemap;
    // private Tilemap roofTilemap;

    void InitializeList()
    {
        gridPositions.Clear();
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                gridPositions.Add(new Vector3(x, y, y));
            }
        }
    }

    private void BoardSetup()
    {
        Grid = new List<List<Node>>();
        for (int i = 0; i < columns; i++)
        {
            Grid.Add(new List<Node>());
            for (int j = 0; j < rows; j++)
            {
                Grid[i].Add(null);
            }
        }

        // tileGrid = new GameObject("Grid").AddComponent<Grid>();
        // groundTilemap = new GameObject("Tilemap").AddComponent<Tilemap>();
        // groundTilemap.transform.SetParent(tileGrid.gameObject.transform);
        // roofTilemap = new GameObject("Tilemap").AddComponent<Tilemap>();
        // roofTilemap.transform.SetParent(tileGrid.gameObject.transform);

        groundTilemap.ClearAllTiles();
        roofTilemap.ClearAllTiles();
        for (int x = -1; x <= columns; x++)
        {
            for (int y = -1; y <= rows; y++)
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), grassTile);
                if (x >= 0 && x < columns && y >= 0 && y < rows)
                {
                    Grid[x][y] = new Node(new Vector2(x, y), true, 1);
                }
            }
        }
    }

    Node GetNode(Vector2 tile)
    {
        return Grid[(int)tile.x][(int)tile.y];
    }

    Vector3 RandomPosition()
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 randomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return randomPosition;
    }

    GameObject RandomObject(GameObject[] objects)
    {
        return objects[Random.Range(0, objects.Length)];
    }

    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
    {
        int objectCount = Random.Range(minimum, maximum + 1);

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition = RandomPosition();
            GameObject tileChoice = RandomObject(tileArray);
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    void LayoutObjectAtCorners(GameObject[] tileArray)
    {
        for (int i = 0; i < gridPositions.Count; i++)
        {
            Vector3 position = gridPositions[i];
            double spawnChance = Math.Pow((Math.Max(position.x, columns - position.x) * Math.Max(position.y, rows - position.y)), 3) / Math.Pow((columns * rows), 3);
            if (Random.value < spawnChance)
            {
                gridPositions.RemoveAt(i);
                GameObject tileChoice = RandomObject(tileArray);
                Instantiate(tileChoice, position, Quaternion.identity);
            }
        }
    }

    private void SpawnPlayers()
    {
        int i = 0;
        foreach (GameObject player in players) {
            if (i >= spawnPositions.Count)
                return;
            Vector3 position = spawnPositions[i];
            Instantiate(player, position, Quaternion.identity);
            gridPositions.Remove(position);
            i++;
        }
    }

    Vector3 RandomHousePosition()
    {
        return new Vector3(0,0,0);
    }

    void PlaceTable(Vector2 tile, Vector2 start, Vector2 stop)
    {
        Instantiate(table, new Vector3(tile.x, tile.y, tile.y), Quaternion.identity);
        Grid[(int)tile.x][(int)tile.y] = new Node(tile, true, 2);
        if (tile.y + 1 != stop.y && Random.Range(0, 2) == 0 && GetNode(new Vector2(tile.x, tile.y + 1)).Weight == 1)
        { Instantiate((Random.Range(0, 5) == 0) ? RandomObject(enemies) : chairFront, new Vector3(tile.x, tile.y + 1, tile.y + 1), Quaternion.identity);
            Grid[(int)tile.x][(int)tile.y + 1] = new Node(tile, true, 2); }
        if (tile.y != start.y && Random.Range(0, 2) == 0 && GetNode(new Vector2(tile.x, tile.y - 1)).Weight == 1)
        { Instantiate((Random.Range(0, 5) == 0) ? RandomObject(enemies) : chairBack, new Vector3(tile.x, tile.y - 1, tile.y - 1), Quaternion.identity);
            Grid[(int)tile.x][(int)tile.y - 1] = new Node(tile, true, 2); }
        if (tile.x - 1 != start.x && Random.Range(0, 2) == 0 && GetNode(new Vector2(tile.x - 1, tile.y)).Weight == 1)
        { Instantiate((Random.Range(0, 5) == 0) ? RandomObject(enemies) : chairLeft, new Vector3(tile.x - 1, tile.y, tile.y), Quaternion.identity);
            Grid[(int)tile.x - 1][(int)tile.y] = new Node(tile, true, 2); }
        if (tile.x + 1 != stop.x && Random.Range(0, 2) == 0 && GetNode(new Vector2(tile.x + 1, tile.y)).Weight == 1)
        { Instantiate((Random.Range(0, 5) == 0) ? RandomObject(enemies) : chairRight, new Vector3(tile.x + 1, tile.y, tile.y), Quaternion.identity);
            Grid[(int)tile.x + 1][(int)tile.y] = new Node(tile, true, 2); }
    }

    void PlaceObject(String type, Vector2 tile, Vector2 start, Vector2 stop, int topRoomDoor = -1)
    {
        GameObject objectTile = null;
        switch (type)
        {
            case "House":
                switch (Random.Range(0, 2))
                {
                    case 0:
                        objectTile = RandomObject(storage);
                        break;
                    case 1:
                        PlaceTable(tile, start, stop);
                        break;
                    default:
                        break;
                }
                break;
            case "Bar":
                if (tile.y == stop.y - 1 && tile.x != topRoomDoor)
                    objectTile = RandomObject(shelves);
                else if (tile.y == stop.y - 3 && tile.x > start.x + 1)
                    objectTile = RandomObject(bar);
                else if (tile.y == stop.y - 4 && tile.x > start.x + 1)
                    objectTile = (Random.Range(0, 3) == 0) ? RandomObject(enemies) : stool;
                else if (tile.y < stop.y - 5)
                    if (Random.Range(0, 5) == 0) { PlaceTable(tile, start, new Vector2(stop.x, stop.y - 5)); }
                break;
            case "Entry":
                switch (Random.Range(0, 2))
                {
                    case 0:
                        objectTile = RandomObject(storage);
                        break;
                    default:
                        break;
                }
                break;
            case "Storage":
                switch (Random.Range(0, 2))
                {
                    case 0:
                        objectTile = RandomObject(storage);
                        break;
                    default:
                        break;
                }
                break;
            case "Bedroom":
                switch (Random.Range(0, 3))
                {
                    case 0:
                        objectTile = RandomObject(storage);
                        break;
                    case 1:
                        PlaceTable(tile, start, stop);
                        break;
                    default:
                        break;
                }
                break;
            case "Center":
                switch (Random.Range(0, 10))
                {
                    case 0:
                        PlaceTable(tile, start, stop);
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        if (objectTile != null)
        {
            Instantiate(objectTile, new Vector3(tile.x, tile.y, tile.y), Quaternion.identity);
            Grid[(int)tile.x][(int)tile.y] = new Node(tile, true, 2);
        }
    }

    void PlaceTiles()
    {
        // if (isRoof)
        //     roofTilemap.SetTile(coords, roofTile);
        // if (isFloor)
        //     groundTilemap.SetTile(coords, floorTile);
    }

    void LayoutRoom()
    {
        // for (float x = start.x; x <= stop.x; x++)
        // {
        //     for (float y = start.y; y <= stop.y; y++)
        //     {
        //         PlaceTiles();
        //         gridPositions.Remove(new Vector3(x, y, y));
        //     }
        // }
    }

    String GetRoomType(String houseType)
    {
        String[] types = roomTypes[houseType];
        return types[Random.Range(0, types.Length)];
    }

    void LayoutBuilding(String type)
    {

    }

    void LayoutBuildings()
    {
        
    }

    public List<List<Node>> SetupScene(int level)
    {
        BoardSetup();
        InitializeList();
        SpawnPlayers();
        // LayoutBuildings();
        LayoutObjectAtCorners(trees);
        LayoutObjectAtRandom(natureObjects, objectCount.minimum, objectCount.maximum/2);
        LayoutObjectAtRandom(streetObjects, objectCount.minimum, objectCount.maximum/2);
        LayoutObjectAtRandom(enemies, enemyCount.minimum, enemyCount.maximum);
        groundTilemap.RefreshAllTiles();
        roofTilemap.RefreshAllTiles();
        GameManager.instance.FinishSetup();
        return Grid;
    }
}