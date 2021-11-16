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

    public class Room
    {
        public String type;
        public bool mandatory;

        public Room(String roomType, bool isMandatory)
        {
            type = roomType;
            mandatory = isMandatory;
        }
    }

    public int columns;
    public int rows;
    public Count objectCount;
    public Count enemyCount;
    public Count buildingCount;
    public Vector2Int buildingRoomGrid;
    public Count roomLengthCount;
    public Count buildingDistance;

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
    // public Tilemap roofTilemap;
    public RuleTile dirtTile;
    public RuleTile grassTile;
    public RuleTile roofTile;
    public RuleTile floorTile;

    public Dictionary<String, Room[]> buildings;
    public List<Vector3> spawnPositions;

    private List<Vector3> gridPositions;
    private List<List<Node>> Grid;

    void InitializeList()
    {
        gridPositions = new List<Vector3>();
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

        // roofTilemap = new GameObject("Tilemap").AddComponent<Tilemap>();
        // roofTilemap.transform.SetParent(tileGrid.gameObject.transform);
        // roofTilemap.ClearAllTiles();
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

    private void SpawnBuildings()
    {
        int targetCount = Random.Range(buildingCount.minimum, buildingCount.maximum + 1);

        while (targetCount > 0) {
            targetCount--;
            int[,] rooms = GenerateBuildingLayout();

            Vector2Int bottomLeft = new Vector2Int(10,5);
            int roomLength = Random.Range(roomLengthCount.minimum, roomLengthCount.maximum + 1);
            LayoutBuilding(bottomLeft, rooms, roomLength);
        }
    }

    private int[,] GenerateBuildingLayout()
    {
        bool failed = true;
        int[,] rooms = new int[0,0];
        while (failed == true)
        {
            rooms = new int[buildingRoomGrid.x, buildingRoomGrid.y];

            int i = 1;
            for (int x = 0; x < buildingRoomGrid.x; x++) {
                for (int y = 0; y < buildingRoomGrid.y ; y++) {
                    if (Random.Range(0, 3) == 0) {
                        rooms[x, y] = i;
                        i++;
                    }
                }
            }

            for (int x = 0; x < buildingRoomGrid.x; x++) {
                for (int y = 0; y < buildingRoomGrid.y ; y++) {
                    if (rooms[x,y] == 0)
                        continue;
                    if (!RoomHasNeighbor(x, y, rooms))
                        rooms[x,y] = 0;
                    else {
                        List<Vector2Int> unmergedNeighbors = GetUnmergedNeighbors(x, y, rooms);
                        foreach (Vector2Int neighbor in unmergedNeighbors) {
                            if (Random.Range(0, 2) > 0) {
                                rooms[neighbor.x, neighbor.y] = rooms[x, y];
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < buildingRoomGrid.x; x++) {
                for (int y = 0; y < buildingRoomGrid.y ; y++) {
                    if (rooms[x, y] > 0)
                        failed = false;
                }
            }
        }

        return rooms;
    }

    private bool RoomHasNeighbor(int x, int y, int[,] rooms) {
        bool neighbor = false;
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                Vector2Int coords = new Vector2Int(x + i, y + j);
                if (((i == 0 && j != 0) || (j == 0 && i != 0)) && coords.x >= 0 && coords.x < buildingRoomGrid.x && coords.y >= 0 && coords.y < buildingRoomGrid.y)
                {
                    if (rooms[coords.x, coords.y] > 0)
                        neighbor = true;
                    Debug.Log("Room: (" + x + ", " + y + ") -> " + rooms[x, y] + "; Other: (" + coords.x + ", " + coords.y + ") -> " + rooms[coords.x, coords.y]);
                }
            }
        }
        return neighbor;
    }

    private List<Vector2Int> GetUnmergedNeighbors (int x, int y, int[,] rooms) {
        List<Vector2Int> unmergedNeighbors = new List<Vector2Int>();
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                Vector2Int coords = new Vector2Int(x + i, y + j);
                if (((i == 0 && j != 0) || (j == 0 && i != 0)) && coords.x >= 0 && coords.x < buildingRoomGrid.x && coords.y >= 0 && coords.y < buildingRoomGrid.y)
                {
                    if (rooms[coords.x, coords.y] != 0 && !RoomHasMerged(coords.x, coords.y, rooms))
                        unmergedNeighbors.Add(coords);
                }
            }
        }
        return unmergedNeighbors;
    }

    private List<Vector2Int> GetMergedNeighbors (int x, int y, int[,] rooms) {
        List<Vector2Int> mergedNeighbors = new List<Vector2Int>();
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                Vector2Int coords = new Vector2Int(x + i, y + j);
                if (((i == 0 && j != 0) || (j == 0 && i != 0)) && coords.x >= 0 && coords.x < buildingRoomGrid.x && coords.y >= 0 && coords.y < buildingRoomGrid.y)
                {
                    if (rooms[x,y] > 0 && rooms[coords.x, coords.y] == rooms[x,y])
                        mergedNeighbors.Add(coords);
                }
            }
        }
        return mergedNeighbors;
    }

    private bool RoomHasMerged(int x, int y, int[,] rooms) {
        bool merged = false;
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                Vector2Int coords = new Vector2Int(x + i, y + j);
                if (((i == 0 && j != 0) || (j == 0 && i != 0)) && coords.x >= 0 && coords.x < buildingRoomGrid.x && coords.y >= 0 && coords.y < buildingRoomGrid.y)
                {
                    if (rooms[coords.x, coords.y] == rooms[x, y])
                        merged = true;
                }
            }
        }
        return merged;
    }

    private void LayoutBuilding(Vector2Int bottomLeft, int[,] rooms, int roomLength)
    {
        List<int> completed = new List<int>() { 0 };
        for (int x = 0; x < buildingRoomGrid.x; x++) {
            for (int y = 0; y < buildingRoomGrid.y; y++) {
                if (completed.Contains(rooms[x,y]))
                    continue;
                List<Vector2Int> mergedRoom = GetMergedNeighbors(x, y, rooms);
                mergedRoom.Add(new Vector2Int(x,y));
                LayoutRoom(bottomLeft, mergedRoom, roomLength);
                completed.Add(rooms[x,y]);
            }
        }
    }

    private void LayoutRoom(Vector2Int bottomLeft, List<Vector2Int> mergedRoom, int roomLength)
    {
        foreach (Vector2Int cell in mergedRoom)
        {
            bool left = false;
            bool right = false;
            bool up = false;
            bool down = false;

            foreach (Vector2Int other in mergedRoom)
            {
                if (cell + new Vector2Int(-1, 0) == other)
                    left = true;
                if (cell + new Vector2Int(1, 0) == other)
                    right = true;
                if (cell + new Vector2Int(0, 1) == other)
                    up = true;
                if (cell + new Vector2Int(0, -1) == other)
                    down = true;
            }

            Vector2Int start = new Vector2Int(bottomLeft.x + (cell.x * roomLength), bottomLeft.y + (cell.y * roomLength));
            Vector2Int end = new Vector2Int(bottomLeft.x + ((cell.x + 1) * roomLength), bottomLeft.y + ((cell.y + 1) * roomLength));
            Vector2Int offset = new Vector2Int((int) buildingRoomGrid.x/2 - cell.x, (int) buildingRoomGrid.y/2 - cell.y);
            start = start + offset;
            end = end + offset;
            Debug.Log("Start: " + start + ", End: " + end);
            for (int x = start.x; x < end.x; x++) {
                for (int y = start.y; y < end.y; y++) {
                    Vector3Int position = new Vector3Int(x, y, y);
                    if (!gridPositions.Contains(position)) {
                        if (x == (start.x*2 + roomLength)/2 || x == (end.x*2 - roomLength)/2 || y == (start.y*2 + roomLength)/2 || y == (end.y*2 - roomLength)/2) {
                            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(new Vector2(x,y), 0.5f);
                            foreach (Collider2D hitCollider in hitColliders)
                            {
                                Wall wall = hitCollider.GetComponent<Wall>();
                                if (wall != null) {
                                    GameObject.Destroy(wall.gameObject);
                                    Instantiate(basicDoor, position, Quaternion.identity);
                                }
                            }
                        }
                        continue;
                    }
                    if (down || y != 0)
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                    if ((!left && x == start.x) || (!right && x == end.x - 1) || (!up && y == end.y - 1) || (!down && y == start.y))
                        Instantiate(wall, position, Quaternion.identity);
                    gridPositions.Remove(position);
                }
            }
        }
    }

    void BuildWall(Vector2Int start, Vector2Int stop)
    {
        if (start.y == stop.y)
        {
            int x = start.x;
            while (x != stop.x + 1)
            {
                Instantiate(wall, new Vector3(x, start.y, start.y), Quaternion.identity);
                x += (stop.x > start.x) ? 1 : -1;
            }
        }
        else if (start.x == stop.x)
            {
                int y = start.y;
                while (y != stop.y + 1)
                {
                    Instantiate(wall, new Vector3(start.x, y, y), Quaternion.identity);
                    y += (stop.y > start.y) ? 1 : -1;
                }
            }
    }

    String GetRoomType(String type)
    {
        Room[] rooms = buildings[type];
        return rooms[Random.Range(0, rooms.Length)].type;
    }

    // Vector3 RandomHousePosition()
    // {
    //     return new Vector3(0,0,0);
    // }

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

    public List<List<Node>> SetupScene(int level)
    {
        BoardSetup();
        InitializeList();
        SpawnPlayers();
        SpawnBuildings();
        LayoutObjectAtCorners(trees);
        LayoutObjectAtRandom(natureObjects, objectCount.minimum, objectCount.maximum/2);
        // LayoutObjectAtRandom(streetObjects, objectCount.minimum, objectCount.maximum/2);
        LayoutObjectAtRandom(enemies, enemyCount.minimum, enemyCount.maximum);
        groundTilemap.RefreshAllTiles();
        // roofTilemap.RefreshAllTiles();
        GameManager.instance.FinishSetup();
        return Grid;
    }
}