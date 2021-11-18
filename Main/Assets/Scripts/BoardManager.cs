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

    public class Building
    {
        public Vector2Int center;
        public Vector2Int widthHeight;

        public Building(Vector2Int newCenter, Vector2Int newWidthHeight)
        {
            center = newCenter;
            widthHeight = newWidthHeight;
        }
    }
    
    public class Roof
    {
        public List<Vector2Int> positions;
        public Tilemap tiles;
        public TilemapRenderer tileRenderer;

        public Roof()
        {
            positions = new List<Vector2Int>();
            GameObject roofObject = new GameObject("Roof");
            tiles = roofObject.AddComponent<Tilemap>();
            tiles.tileAnchor = new Vector3(0, -0.25f, 0);
            tileRenderer = roofObject.AddComponent<TilemapRenderer>();
            tileRenderer.sortingLayerName = "Roof";
            tiles.transform.SetParent(GameManager.instance.GetComponent<Grid>().transform);
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

    public Grid tileGrid;
    public Tilemap bottomTilemap;
    public Tilemap groundTilemap;
    // public Tilemap roofTilemap;
    public RuleTile dirtTile;
    public RuleTile grassTile;
    public RuleTile roofTile;
    public RuleTile floorTile;
    
    public List<Vector3Int> spawnPositions;

    private List<Vector3Int> gridPositions;
    private List<List<Node>> Grid;
    private Dictionary<String, Room[]> buildings;
    private List<Roof> roofs;

    void InitializeList()
    {
        gridPositions = new List<Vector3Int>();
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                gridPositions.Add(new Vector3Int(x, y, y));
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

        bottomTilemap.ClearAllTiles();
        groundTilemap.ClearAllTiles();
        roofs = new List<Roof>();
        for (int x = -1; x <= columns; x++)
        {
            for (int y = -1; y <= rows; y++)
            {
                bottomTilemap.SetTile(new Vector3Int(x, y, 0), dirtTile);
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
            Vector3Int position = gridPositions[i];
            double spawnChance = Math.Pow((Math.Max(position.x, columns - position.x) * Math.Max(position.y, rows - position.y)), 3) / Math.Pow((columns * rows), 3);
            if (Random.value < spawnChance)
            {
                GameObject tileChoice = RandomObject(tileArray);
                Instantiate(tileChoice, position, Quaternion.identity);
                for (int x = -2; x <= 2; x++) {
                    for (int y = -2; y <= 2; y++) {
                        Vector3Int newPos = new Vector3Int(position.x + x, position.y + y, 0);
                        if (newPos.x < 0 || newPos.x == columns || newPos.y < 0 || newPos.y == rows || gridPositions.Contains(new Vector3Int(newPos.x, newPos.y, newPos.y)))
                            groundTilemap.SetTile(newPos, grassTile);
                    }
                }
                gridPositions.RemoveAt(i);
            }
        }
    }

    private void SpawnPlayers()
    {
        int i = 0;
        foreach (GameObject player in players) {
            if (i >= spawnPositions.Count)
                return;
            Vector3Int position = spawnPositions[i];
            Instantiate(player, position, Quaternion.identity);
            gridPositions.Remove(position);
            i++;
        }
    }

    private void SpawnBuildings()
    {
        int targetCount = Random.Range(buildingCount.minimum, buildingCount.maximum + 1);
        List<Building> buildings = new List<Building>() { new Building(new Vector2Int(columns/2,rows/2), new Vector2Int(0,0)) };

        while (targetCount > 0) {
            targetCount--;
            int[,] rooms = GenerateBuildingLayout();
            int roomLength = Random.Range(roomLengthCount.minimum, roomLengthCount.maximum + 1);

            bool placed = false;
            for (int i = 0; i < buildings.Count; i++) {
                for (int j = 0; j < 8; j++) {
                    Vector2Int center = buildings[i].center;
                    if (j == 1 || j == 2 || j == 3)
                        center = center + new Vector2Int(buildings[i].widthHeight.x/2 + Random.Range(buildingDistance.minimum, buildingDistance.maximum), 0);
                    else if (j == 5 || j == 6 || j == 7)
                        center = center + new Vector2Int(0 - buildings[i].widthHeight.x/2 - Random.Range(buildingDistance.minimum, buildingDistance.maximum), 0);
                    if (j == 7 || j == 0 || j == 1)
                        center = center + new Vector2Int(0, buildings[i].widthHeight.y/2 + Random.Range(buildingDistance.minimum, buildingDistance.maximum));
                    else if (j == 3 || j == 4 || j == 5)
                        center = center + new Vector2Int(0, 0 - buildings[i].widthHeight.y/2 - Random.Range(buildingDistance.minimum, buildingDistance.maximum));
                    placed = CheckBuilding(center, rooms, roomLength);
                    if (placed) {
                        LayoutBuilding(center, rooms, roomLength);
                        buildings.Add(new Building(center, GetBuildingWidthHeight(rooms, roomLength)));
                        if (buildings[0].widthHeight == new Vector2Int(0,0))
                            buildings.RemoveAt(0);
                        break;
                    }
                }
                if (placed)
                    break;
            }
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

    private Vector2Int GetBuildingWidthHeight(int[,] rooms, int roomLength)
    {
        int minX = buildingRoomGrid.x;
        int maxX = 0;
        int minY = buildingRoomGrid.y;
        int maxY = 0;
        for (int x = 0; x < buildingRoomGrid.x; x++) {
            for (int y = 0; y < buildingRoomGrid.y; y++) {
                if (rooms[x, y] > 0) {
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }
            }
        }
        int width = (maxX - minX + 1) * roomLength - (maxX - minX);
        int height = (maxY - minY + 1) * roomLength - (maxY - minY);
        return new Vector2Int(width, height);
    }

    private Vector2Int GetBuildingMinXY(int[,] rooms, int roomLength)
    {
        int minX = buildingRoomGrid.x;
        int minY = buildingRoomGrid.y;
        for (int x = 0; x < buildingRoomGrid.x; x++) {
            for (int y = 0; y < buildingRoomGrid.y; y++) {
                if (rooms[x, y] > 0) {
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                }
            }
        }
        return new Vector2Int(minX, minY);
    }

    private bool CheckBuilding(Vector2Int center, int[,] rooms, int roomLength)
    {
        Vector2Int widthHeight = GetBuildingWidthHeight(rooms, roomLength);
        Vector2Int minXY = GetBuildingMinXY(rooms, roomLength);
        Vector2Int bottomLeft = new Vector2Int(center.x - widthHeight.x/2, center.y - widthHeight.y/2);
        Debug.Log(widthHeight.x + ", " + widthHeight.y + " " + bottomLeft.x + ", " + bottomLeft.y + " " + (bottomLeft.x + widthHeight.x) + ", " + (bottomLeft.y + widthHeight.y));
        List<int> completed = new List<int>() { 0 };

        for (int x = 0; x < buildingRoomGrid.x; x++) {
            for (int y = 0; y < buildingRoomGrid.y; y++) {
                if (completed.Contains(rooms[x,y]))
                    continue;
                List<Vector2Int> mergedRoom = GetMergedNeighbors(x, y, rooms);
                mergedRoom.Add(new Vector2Int(x,y));
                if (!CheckRoom(bottomLeft, mergedRoom, roomLength, minXY))
                    return false;
                completed.Add(rooms[x,y]);
            }
        }

        return true;
    }

    private bool CheckRoom(Vector2Int bottomLeft, List<Vector2Int> mergedRoom, int roomLength, Vector2Int minXY)
    {
        foreach (Vector2Int cell in mergedRoom)
        {
            Vector2Int newCell = cell - minXY;
            Vector2Int start = new Vector2Int(bottomLeft.x + (newCell.x * roomLength), bottomLeft.y + (newCell.y * roomLength));
            Vector2Int end = new Vector2Int(bottomLeft.x + ((newCell.x + 1) * roomLength), bottomLeft.y + ((newCell.y + 1) * roomLength));
            Vector2Int offset = new Vector2Int(buildingRoomGrid.x/2 - newCell.x - 1, buildingRoomGrid.y/2 - newCell.y - 1);
            Vector2Int spacing = new Vector2Int(buildingDistance.minimum, buildingDistance.minimum);
            start = start + offset - spacing;
            end = end + offset + spacing;
            Debug.Log("Start: " + start + ", End: " + end);

            for (int x = start.x; x < end.x; x++) {
                for (int y = start.y; y < end.y; y++) {
                    Vector3Int position = new Vector3Int(x, y, y);
                    if (!gridPositions.Contains(position)) {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private Vector2 LayoutBuilding(Vector2Int center, int[,] rooms, int roomLength)
    {
        Roof roof = new Roof();
        roofs.Add(roof);

        Vector2Int widthHeight = GetBuildingWidthHeight(rooms, roomLength);
        Vector2Int minXY = GetBuildingMinXY(rooms, roomLength);
        Vector2Int bottomLeft = new Vector2Int(center.x - widthHeight.x/2, center.y - widthHeight.y/2);
        Debug.Log(widthHeight.x + ", " + widthHeight.y + " " + bottomLeft.x + ", " + bottomLeft.y + " " + (bottomLeft.x + widthHeight.x) + ", " + (bottomLeft.y + widthHeight.y));
        List<int> completed = new List<int>() { 0 };
        Vector2Int placedFrontDoor = new Vector2Int(0,0);

        for (int x = 0; x < buildingRoomGrid.x; x++) {
            for (int y = 0; y < buildingRoomGrid.y; y++) {
                if (completed.Contains(rooms[x,y]))
                    continue;
                List<Vector2Int> mergedRoom = GetMergedNeighbors(x, y, rooms);
                mergedRoom.Add(new Vector2Int(x,y));
                placedFrontDoor = LayoutRoom(bottomLeft, mergedRoom, roomLength, minXY, placedFrontDoor, roof);
                completed.Add(rooms[x,y]);
            }
        }

        return placedFrontDoor;
    }

    private Vector2Int LayoutRoom(Vector2Int bottomLeft, List<Vector2Int> mergedRoom, int roomLength, Vector2Int minXY, Vector2Int placedFrontDoor, Roof roof)
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

            Vector2Int newCell = cell - minXY;
            Vector2Int start = new Vector2Int(bottomLeft.x + (newCell.x * roomLength), bottomLeft.y + (newCell.y * roomLength));
            Vector2Int end = new Vector2Int(bottomLeft.x + ((newCell.x + 1) * roomLength), bottomLeft.y + ((newCell.y + 1) * roomLength));
            Vector2Int offset = new Vector2Int(buildingRoomGrid.x/2 - newCell.x - 1, buildingRoomGrid.y/2 - newCell.y - 1);
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
                    if (up || y != end.y - 1)
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                    if ((!left && x == start.x) || (!right && x == end.x - 1) || (!up && y == end.y - 1) || (!down && y == start.y)) {
                        if (placedFrontDoor == new Vector2Int(0,0) && gridPositions.Contains(position - new Vector3Int(0,1,1)) && !down && y == start.y && (x == (start.x*2 + roomLength)/2 || x == (end.x*2 - roomLength)/2)) {
                            Instantiate(basicDoor, position, Quaternion.identity);
                            placedFrontDoor = new Vector2Int(position.x, position.y);
                        } else {
                            GameObject newWall = Instantiate(wall, position, Quaternion.identity);
                            if (gridPositions.Contains(position - new Vector3Int(0,1,1)) && !down && y == start.y)
                                newWall.GetComponent<Wall>().IsFront();
                        }
                    }
                    gridPositions.Remove(position);
                    roof.positions.Add(new Vector2Int(x, y + 1));
                    roof.tiles.SetTile(new Vector3Int(x, y + 1, 0), roofTile);
                }
            }
        }
        return placedFrontDoor;
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

    public List<Roof> GetRoofs() {
        return roofs;
    }
}