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
    public Count buildingCount;
    public Count buildingWallLength;

    public GameObject[] players;
    public GameObject exit;
    public GameObject[] enemies;
    public GameObject[] groundTiles;
    public GameObject[] streetTiles;
    public GameObject[] trees;
    public GameObject[] natureObjects;
    public GameObject[] streetObjects;
    public GameObject[] borderWalls;

    public GameObject wall;
    public GameObject roof;
    public GameObject floorTile;
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

    private Transform boardHolder;
    private List<Vector3> gridPositions = new List<Vector3>();
    private List<List<Node>> Grid;
    private List<List<Roof>> Roofs;
    private List<List<Wall>> Walls;
    private List<List<GameObject>> Floors;
    private Dictionary<String, String[]> roomTypes = new Dictionary<String, String[]>
    {
        {"Bar", new []{"Storage"} },
        {"House", new []{"Bedroom", "Storage"} }
    };

    public List<Vector3> spawnPositions = new List<Vector3>();

    void InitializeList()
    {
        Roofs = new List<List<Roof>>();
        Walls = new List<List<Wall>>();
        Floors = new List<List<GameObject>>();

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
        boardHolder = new GameObject("Board").transform;
        Grid = new List<List<Node>>();
        for (int i = 0; i < columns; i++)
        {
            Grid.Add(new List<Node>());
            for (int j = 0; j < rows; j++)
            {
                Grid[i].Add(null);
            }
        }

        for (int x = -1; x < columns + 1; x++)
        {
            for (int y = -1; y < rows + 1; y++)
            {
                GameObject toInstantiate = RandomObject(groundTiles);
                if (x == -1 || x == columns || y == -1 || y == rows)
                {
                    toInstantiate = RandomObject(borderWalls);
                }
                else
                    Grid[x][y] = new Node(new Vector2(x, y), true, 1);

                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                instance.transform.SetParent(boardHolder);
            }
        }
    }

    private void SpawnPlayers() {
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

    Vector3 RandomHousePosition(int length, int attempt = 0)
    {
        attempt++;
        if (attempt > 20)
        {
            return new Vector2(-1, -1);
        }

        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 randomPosition = gridPositions[randomIndex];
        for (int x = 0; x <= length; x++)
        {
            for (int y = 0; y <= length; y++)
            {
                Vector3 checkPosition = new Vector3(x + (int)randomPosition.x, y + (int)randomPosition.y, y + (int)randomPosition.y);
                if (!gridPositions.Contains(checkPosition)) { return RandomHousePosition(length, attempt); }
            }
        }
        return randomPosition;
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

    Node GetNode(Vector2 tile)
    {
        return Grid[(int)tile.x][(int)tile.y];
    }

    void PlaceTiles(float x, float y, String type, Vector2 start, Vector2 stop, String location, int house, int topRoomDoor, int bottomRoomDoor, int mainRoomDoor, int topStartX, int topStopX, int bottomStartX, int bottomStopX)
    {
        if (!gridPositions.Contains(new Vector3(x, y, y)))
            return;
        GameObject objectTile = null;
        bool roofTile = false;
        GameObject floor = floorTile;
        Vector2 tile = new Vector2(x, y);
        if (location != "Top")
        {
            if (tile == start)
            {
                floor = null;
                objectTile = wall;
            }
            else if (tile == new Vector2(stop.x, start.y))
            {
                floor = null;
                objectTile = wall;
            }
            else if (tile == new Vector2(start.x + 1, start.y + 1))
                roofTile = true;
            else if (tile == new Vector2(stop.x - 1, start.y + 1))
                roofTile = true;
            else if (tile.y == start.y)
            {
                objectTile = wall;
                if ((location == "Bottom" && bottomRoomDoor == tile.x) || (location == "Main" && mainRoomDoor == tile.x))
                {
                    objectTile = (type != "Bar" && Random.Range(0, 3) == 0) ? keyDoor : basicDoor;
                    Grid[(int)x][(int)y] = new Node(new Vector2(x, y), true, 2);
                    Grid[(int)x][(int)y + 1] = new Node(new Vector2(x, y + 1), true, 2);
                    if (location == "Bottom" || location == "Main" && bottomRoomDoor == -1)
                        gridPositions.Remove(new Vector3(x, y - 1, y - 1));
                    else
                        Grid[(int)x][(int)y - 1] = new Node(new Vector2(x, y - 1), true, 2);
                }
                else if (location == "Main")
                {
                    if (tile.x == bottomStartX)
                        objectTile = wall;
                    else if (tile.x == bottomStopX)
                        objectTile = wall;
                }
                if (location == "Main")
                {
                //    roofTile = true;
                }
            }
            else if (tile.y == start.y + 1 && tile.x != start.x && tile.x != stop.x)
            {
                roofTile = true;
                if (location == "Main")
                {
                    if (type == "Bar")
                        PlaceObject("Bar", tile, start, stop, topRoomDoor);
                }
            }
        }
        if (location != "Bottom")
        {
            if (tile == stop)
            {
                floor = null;
                objectTile = wall;
            }
            else if (tile == new Vector2(start.x, stop.y))
            {
                objectTile = wall;
                floor = null;
            }
            else if (tile.y == stop.y && tile.x > start.x && tile.x < stop.x)
            {
                floor = null;
                objectTile = wall;
                if (location == "Main")
                {
                    if (tile.x == topRoomDoor)
                    {
                        floor = floorTile;
                        objectTile = Random.Range(0, 3) == 0 ? keyDoor : basicDoor;
                        Grid[(int)x][(int)y] = new Node(new Vector2(x, y), true, 2);
                        Grid[(int)x][(int)y + 1] = new Node(new Vector2(x, y + 1), true, 2);
                    }
                }
                roofTile = true;
            }
        }
        if (objectTile == null && !roofTile)
        {
            if (tile.x == start.x)
            {
                floor = null;
                objectTile = wall;
            }
            else if (tile.x == stop.x)
            {
                floor = null;
                objectTile = wall;
            }
            else if (tile.x == start.x + 1)
            {
                roofTile = true;
                if (location == "Main" && type == "Bar")
                    PlaceObject("Bar", tile, start, stop, topRoomDoor);
                else if (GetNode(new Vector2(tile.x, tile.y - 1)).Weight == 1 && GetNode(new Vector2(tile.x, tile.y + 1)).Weight == 1)
                    PlaceObject(type == "Bottom" ? "Entry" : type, tile, start, stop);
            }
            else if (tile.x == stop.x - 1)
            {
                roofTile = true;
                if (location == "Main" && type == "Bar")
                    PlaceObject("Bar", tile, start, stop, topRoomDoor);
                else if (GetNode(new Vector2(tile.x, tile.y - 1)).Weight == 1 && GetNode(new Vector2(tile.x, tile.y + 1)).Weight == 1)
                    PlaceObject(type == "Bottom" ? "Entry" : type, tile, start, stop);
            }
            else
            {
                roofTile = true;
                if (location == "Main" && type == "Bar")
                    PlaceObject("Bar", tile, start, stop, topRoomDoor);
                else if (GetNode(new Vector2(tile.x, tile.y - 1)).Weight == 1 && GetNode(new Vector2(tile.x, tile.y + 1)).Weight == 1 && GetNode(new Vector2(tile.x - 1, tile.y)).Weight == 1 && GetNode(new Vector2(tile.x + 1, tile.y)).Weight == 1)
                    PlaceObject(type == "Bottom" ? "Nothing" : "Center", tile, start, stop);
            }
        }
        if (objectTile != null)
        {
            GameObject newObject = Instantiate(objectTile, new Vector3(x, y, y), Quaternion.identity);
            Grid[(int)x][(int)y] = new Node(new Vector2(x, y), true, 2);
            Wall wallScript = newObject.GetComponent<Wall>();
            if (wallScript != null) {
                Walls[house].Add(wallScript);
                wallScript.setWallIndex(house);
            }
        }
        if (roofTile)
        {
            GameObject roofObject = Instantiate(roof, new Vector3(x, y, y-1.5f), Quaternion.identity);
            Roof roofScript = roofObject.GetComponent<Roof>();
            roofScript.setRoofIndex(house);
            Roofs[house].Add(roofScript);
        }
        if (floor != null) {
            GameObject floorObject = Instantiate(floor, tile, Quaternion.identity);
            Floors[house].Add(floorObject);
        }
    }

    void LayoutRoom(String type, Vector2 start, Vector2 stop, String location, int house, int topRoomDoor, int bottomRoomDoor, int mainRoomDoor, int topStartX = -1, int topStopX = -1, int bottomStartX = -1, int bottomStopX = -1) //room location can be "Main", "Top", or "Bottom"
    {
        for (float x = start.x; x <= stop.x; x++)
        {
            for (float y = start.y; y <= stop.y; y++)
            {
                PlaceTiles(x, y, type, start, stop, location, house, topRoomDoor, bottomRoomDoor, mainRoomDoor, topStartX, topStopX, bottomStartX, bottomStopX);
                gridPositions.Remove(new Vector3(x, y, y));
            }
        }
    }

    String GetRoomType(String houseType)
    {
        String[] types = roomTypes[houseType];
        return types[Random.Range(0, types.Length)];
    }

    void LayoutBuilding(int i, String type)
    {
        int length = type == "Bar" ? buildingWallLength.maximum : Random.Range(buildingWallLength.minimum, buildingWallLength.maximum);
        Vector2 position = RandomHousePosition(length);
        Roofs.Add(new List<Roof>());
        Walls.Add(new List<Wall>());
        Floors.Add(new List<GameObject>());
        if (position == new Vector2(-1, -1))
            return;
        Vector2 start = new Vector2(0, Random.Range(0, length / (type == "Bar" ? 3 : 2))) + position;
        Vector2 stop = new Vector2((int)(position.x + length), (int)Random.Range(start.y + (type == "Bar" ? 6 : 5), position.y + length));
        int topRoomDoor = -1;
        int topStartX = -1;
        int topStopX = -1;
        int bottomRoomDoor = -1;
        int bottomStartX = -1;
        int bottomStopX = -1;
        int mainRoomDoor;
        if (start.y > position.y + 1)
        {
            Vector2 roomStart = new Vector2(Random.Range(0, length / 2), 0) + position;
            bottomStartX = (int)roomStart.x;
            Vector2 roomStop = new Vector2((int)Random.Range(roomStart.x + 4, position.x + length), (int)(start.y - 1));
            bottomStopX = (int)roomStop.x;
            bottomRoomDoor = (int)Random.Range(roomStart.x + 1, roomStop.x - 1);
            mainRoomDoor = (int)Random.Range(roomStart.x + 1, roomStop.x - 1);
            LayoutRoom(type, roomStart, roomStop, "Bottom", i, topRoomDoor, bottomRoomDoor, mainRoomDoor);
        }
        else
        {
            mainRoomDoor = (int)Random.Range(start.x + 1, stop.x - 1);
        }
        if (stop.y < position.y + length - 1)
        {
            Vector2 roomStart = new Vector2((int)(position.x + Random.Range(0, length / 2)), (int)(stop.y + 1));
            topStartX = (int)roomStart.x;
            Vector2 roomStop = new Vector2((int)Random.Range(roomStart.x + 4, position.x + length), (int)(position.y + length));
            topStopX = (int)roomStop.x;
            topRoomDoor = (int)Random.Range(roomStart.x + 1, roomStop.x - 1);
            LayoutRoom(GetRoomType(type), roomStart, roomStop, "Top", i, topRoomDoor, bottomRoomDoor, mainRoomDoor);
        }

        LayoutRoom(type, start, stop, "Main", i, topRoomDoor, bottomRoomDoor, mainRoomDoor, topStartX, topStopX, bottomStartX, bottomStopX);

        //tileChoice = triggerDoor;
        //GameObject instance = Instantiate(tileChoice, position, Quaternion.identity) as GameObject;
        //Vector2 randomPos = RandomPosition();
        //while (randomPos.x >= (int)randomPosition.x && randomPos.x <= (int)randomPosition.x + width && randomPos.y >= (int)randomPosition.y && randomPos.y <= (int)randomPosition.y + length) //ensure lever not in building
        //    randomPos = RandomPosition();
        //instance.GetComponent<Door>().SetupTrigger(randomPos);
        //continue;
    }

    void LayoutBuildings()
    {
        int buildingCount = Random.Range(this.buildingCount.minimum, this.buildingCount.maximum + 1);

        for (int i = 0; i < buildingCount; i++)
        {
            String type = (i == 0 || Random.Range(0, 5) == 0) ? "Bar" : "House";
            LayoutBuilding(i, type);
        }
    }

    public List<List<Node>> SetupScene(int level)
    {
        BoardSetup();
        InitializeList();
        SpawnPlayers();
        LayoutBuildings();
        LayoutObjectAtCorners(trees);
        LayoutObjectAtRandom(natureObjects, objectCount.minimum, objectCount.maximum/2);
        LayoutObjectAtRandom(streetObjects, objectCount.minimum, objectCount.maximum/2);
        int enemyCount = (int)Mathf.Log(level, 2f); //added 2
        LayoutObjectAtRandom(enemies, enemyCount, enemyCount);
        // Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);

        // foreach(List<Roof> subRoofs in Roofs){
        //     foreach(Roof roof in subRoofs){
        //         Debug.Log(roof);
        //     }
        // }
        GameManager.instance.FinishSetup(Roofs, Walls, Floors);
        return Grid;
    }

    // public List<List<Roof>> GetRoofs()
    // {
    //     return Roofs;
    // }
}
