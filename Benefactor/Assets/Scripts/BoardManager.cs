using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
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

    public GameObject player;
    public GameObject exit;
    public GameObject[] enemies;
    public GameObject[] groundTiles;
    public GameObject[] streetTiles;
    public GameObject[] natureObjects;
    public GameObject[] streetObjects;
    public GameObject[] borderWalls;

    public GameObject houseWallFront;
    public GameObject houseWallFrontBar;
    public GameObject houseWallRight;
    public GameObject houseWallLeft;
    public GameObject houseWallFrontRight;
    public GameObject houseWallFrontLeft;
    public GameObject houseWallBackRight;
    public GameObject houseWallBackLeft;
    public GameObject houseWallCornerLeft;
    public GameObject houseWallCornerRight;

    public GameObject roofFlat;
    public GameObject roofFront;
    public GameObject roofRight;
    public GameObject roofLeft;
    public GameObject roofFrontInnerCornerRight;
    public GameObject roofFrontInnerCornerLeft;
    public GameObject roofFrontOuterCornerRight;
    public GameObject roofFrontOuterCornerLeft;
    public GameObject roofBackCornerLeft;
    public GameObject roofBackCornerRight;

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
    private Dictionary<String, String[]> roomTypes = new Dictionary<String, String[]>
    {
        {"Bar", new []{"Storage"} },
        {"House", new []{"Bedroom", "Storage"} }
    };

    void InitializeList()
    {
        gridPositions.Clear();
        for (int x = 1; x < columns - 1; x++)
        {
            for (int y = 1; y < rows - 1; y++)
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

        Roofs = new List<List<Roof>>();
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
        { Instantiate(chairFront, new Vector3(tile.x, tile.y + 1, tile.y + 1), Quaternion.identity);
            Grid[(int)tile.x][(int)tile.y + 1] = new Node(tile, true, 2); }
        if (tile.y != start.y - 1 && Random.Range(0, 2) == 0 && GetNode(new Vector2(tile.x, tile.y - 1)).Weight == 1)
        { Instantiate(chairBack, new Vector3(tile.x, tile.y - 1, tile.y - 1), Quaternion.identity);
            Grid[(int)tile.x][(int)tile.y - 1] = new Node(tile, true, 2); }
        if (tile.x - 1 != start.x && Random.Range(0, 2) == 0 && GetNode(new Vector2(tile.x - 1, tile.y)).Weight == 1)
        { Instantiate(chairLeft, new Vector3(tile.x - 1, tile.y, tile.y), Quaternion.identity);
            Grid[(int)tile.x - 1][(int)tile.y] = new Node(tile, true, 2); }
        if (tile.x + 1 != stop.x && Random.Range(0, 2) == 0 && GetNode(new Vector2(tile.x + 1, tile.y)).Weight == 1)
        { Instantiate(chairRight, new Vector3(tile.x + 1, tile.y, tile.y), Quaternion.identity);
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
        GameObject roofTile = null;
        GameObject floor = floorTile;
        Vector2 tile = new Vector2(x, y);
        if (location != "Top")
        {
            if (tile == start)
            {
                floor = null;
                objectTile = (tile.x == bottomStartX) ? houseWallLeft : houseWallFrontLeft;
            }
            else if (tile == new Vector2(stop.x, start.y))
            {
                floor = null;
                objectTile = (tile.x == bottomStopX) ? houseWallRight : houseWallFrontRight;
            }
            else if (tile == new Vector2(start.x + 1, start.y + 1))
                roofTile = (tile.x == bottomStartX + 1) ? roofLeft : roofFrontOuterCornerLeft;
            else if (tile == new Vector2(stop.x - 1, start.y + 1))
                roofTile = (tile.x == bottomStopX - 1) ? roofRight : roofFrontOuterCornerRight;
            else if (tile.y == start.y)
            {
                objectTile = type == "Bar" && ((bottomRoomDoor == -1 && location == "Main" && (tile.x - 1 == mainRoomDoor) || (mainRoomDoor - 1 == start.x - 1 && tile.x + 1 == mainRoomDoor)) || 
                    (location == "Bottom" && (tile.x - 1 == bottomRoomDoor) || (bottomRoomDoor - 1 == bottomStartX - 1 && tile.x + 1 == bottomRoomDoor))) ? houseWallFrontBar : houseWallFront;
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
                        objectTile = houseWallCornerLeft;
                    else if (tile.x == bottomStopX)
                        objectTile = houseWallCornerRight;
                }
                if (location == "Main")
                {
                    if (tile.x == bottomStartX + 1)
                        roofTile = roofLeft;
                    else if (tile.x == bottomStopX - 1)
                        roofTile = roofRight;
                    else if (tile.x > bottomStartX + 1 && tile.x < bottomStopX - 1)
                        roofTile = roofFlat;
                }
            }
            else if (tile.y == start.y + 1 && tile.x != start.x && tile.x != stop.x)
            {
                roofTile = roofFront;
                if (location == "Main")
                {
                    if (tile.x == bottomStartX + 1)
                        roofTile = roofFrontInnerCornerLeft;
                    else if (tile.x == bottomStopX - 1)
                        roofTile = roofFrontInnerCornerRight;
                    else if (tile.x > bottomStartX + 1 && tile.x < bottomStopX - 1)
                        roofTile = roofFlat;
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
                objectTile = houseWallBackRight;
            }
            else if (tile == new Vector2(start.x, stop.y))
            {
                objectTile = houseWallBackLeft;
                floor = null;
            }
            else if (tile.y == stop.y && tile.x > start.x && tile.x < stop.x)
            {
                floor = null;
                objectTile = houseWallFront;
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
                if (tile.x == start.x + 1)
                    roofTile = (tile.x == topStartX + 1) ? roofLeft : roofBackCornerLeft;
                else if (tile.x == stop.x - 1)
                    roofTile = (tile.x == topStopX - 1) ? roofRight : roofBackCornerRight;
                else
                    roofTile = roofFlat;
            }
        }
        if (objectTile == null && roofTile == null)
        {
            if (tile.x == start.x)
            {
                floor = null;
                objectTile = houseWallLeft;
            }
            else if (tile.x == stop.x)
            {
                floor = null;
                objectTile = houseWallRight;
            }
            else if (tile.x == start.x + 1)
            {
                roofTile = roofLeft;
                if (location == "Main" && type == "Bar")
                    PlaceObject("Bar", tile, start, stop, topRoomDoor);
                else if (GetNode(new Vector2(tile.x, tile.y - 1)).Weight == 1 && GetNode(new Vector2(tile.x, tile.y + 1)).Weight == 1)
                    PlaceObject(type == "Bottom" ? "Entry" : type, tile, start, stop);
            }
            else if (tile.x == stop.x - 1)
            {
                roofTile = roofRight;
                if (location == "Main" && type == "Bar")
                    PlaceObject("Bar", tile, start, stop, topRoomDoor);
                else if (GetNode(new Vector2(tile.x, tile.y - 1)).Weight == 1 && GetNode(new Vector2(tile.x, tile.y + 1)).Weight == 1)
                    PlaceObject(type == "Bottom" ? "Entry" : type, tile, start, stop);
            }
            else
            {
                roofTile = roofFlat;
                if (location == "Main" && type == "Bar")
                    PlaceObject("Bar", tile, start, stop, topRoomDoor);
                else if (GetNode(new Vector2(tile.x, tile.y - 1)).Weight == 1 && GetNode(new Vector2(tile.x, tile.y + 1)).Weight == 1 && GetNode(new Vector2(tile.x - 1, tile.y)).Weight == 1 && GetNode(new Vector2(tile.x + 1, tile.y)).Weight == 1)
                    PlaceObject(type == "Bottom" ? "Nothing" : "Center", tile, start, stop);
            }
        }
        if (objectTile != null)
        {
            Instantiate(objectTile, new Vector3(x, y, y), Quaternion.identity);
            Grid[(int)x][(int)y] = new Node(new Vector2(x, y), true, 2);
        }
        if (roofTile != null)
        {
            GameObject roofObject = Instantiate(roofTile, new Vector3(x, y, y-1.5f), Quaternion.identity);
            Roof roof = roofObject.GetComponent<Roof>();
            roof.setRoofIndex(house);
            Roofs[house].Add(roof);
        }
        if (floor != null)
            Instantiate(floor, tile, Quaternion.identity);
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
        LayoutBuildings();
        LayoutObjectAtRandom(natureObjects, objectCount.minimum, objectCount.maximum/2);
        LayoutObjectAtRandom(streetObjects, objectCount.minimum, objectCount.maximum/2);
        int enemyCount = (int)Mathf.Log(level, 2f) + 2; //added 2
        LayoutObjectAtRandom(enemies, enemyCount, enemyCount);
        Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);
        //Instantiate(player, new Vector3(0, 0, 0f), Quaternion.identity);

        return Grid;
    }

    public List<List<Roof>> GetRoofs()
    {
        return Roofs;
    }
}
