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
    public Count houseCount;
    public Count houseWallLength;
    public GameObject player;
    public GameObject exit;
    public GameObject[] groundTiles;
    public GameObject[] objects;
    public GameObject[] enemyTiles;
    public GameObject[] borderTiles;
    public List<List<GameObject>> furniture;

    public GameObject houseWallFront;
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

    private Transform boardHolder;
    private List<Vector3> gridPositions = new List<Vector3>();
    private List<List<Node>> Grid;
    private List<List<Roof>> Roofs;

    void InitializeList()
    {
        gridPositions.Clear();
        for (int x = 1; x < columns - 1; x++)
        {
            for (int y = 1; y < rows - 1; y++)
            {
                gridPositions.Add(new Vector3(x, y, 0f));
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
                GameObject toInstantiate = groundTiles[Random.Range(0, groundTiles.Length)];
                if (x == -1 || x == columns || y == -1 || y == rows)
                {
                    toInstantiate = borderTiles[Random.Range(0, borderTiles.Length)];
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

    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
    {
        int objectCount = Random.Range(minimum, maximum + 1);

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition = RandomPosition();
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    Vector2 RandomHousePosition(int length, int attempt = 0)
    {
        attempt++;
        if (attempt > 10)
        {
            return new Vector2(-1,-1);
        }

        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector2 randomPosition = gridPositions[randomIndex];
        for (int x = 0; x <= length; x++)
        {
            for (int y = 0; y <= length; y++)
            {
                Vector2 checkPosition = new Vector2(x + (int)randomPosition.x, y + (int)randomPosition.y);
                if (!gridPositions.Contains(checkPosition)) { return RandomHousePosition(length, attempt); }
            }
        }

        return randomPosition;
    }

    void LayoutRoom(Vector2 start, Vector2 stop, String roomType, int house, GameObject[] furniture, int topRoomDoor, int bottomRoomDoor, int mainRoomDoor, int topStartX = -1, int topStopX = -1, int bottomStartX = -1, int bottomStopX = -1) //roomType can be "Main", "Top", or "Bottom"
    {
        for (float x = start.x; x <= stop.x; x++)
        {
            for (float y = start.y; y <= stop.y; y++)
            {
                GameObject objectTile = null;
                GameObject roofTile = null;
                GameObject floor = floorTile;
                Vector2 tile = new Vector2(x, y);

                if (roomType != "Top")
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
                        objectTile = houseWallFront;
                        if ((roomType == "Bottom" && bottomRoomDoor == tile.x) || (roomType == "Main" && mainRoomDoor == tile.x))
                            objectTile = Random.Range(0, 3) == 0 ? keyDoor : basicDoor;
                        else if (roomType == "Main")
                        {
                            if (tile.x == bottomStartX)
                                objectTile = houseWallCornerLeft;
                            else if (tile.x == bottomStopX)
                                objectTile = houseWallCornerRight;
                        }
                        if (roomType == "Main")
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
                        if (roomType == "Main")
                        {
                            if (tile.x == bottomStartX + 1)
                                roofTile = roofFrontInnerCornerLeft;
                            else if (tile.x == bottomStopX - 1)
                                roofTile = roofFrontInnerCornerRight;
                            else if (tile.x > bottomStartX + 1 && tile.x < bottomStopX - 1)
                                roofTile = roofFlat;
                        }
                    }
                }
                if (roomType != "Bottom")
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
                        if (roomType == "Main")
                        {
                            if (tile.x == topRoomDoor)
                            {
                                floor = floorTile;
                                objectTile = Random.Range(0, 3) == 0 ? keyDoor : basicDoor;
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
                        objectTile = (Random.Range(0, 2) == 0) ? storage[Random.Range(0, storage.Length)] : null;
                    }
                    else if (tile.x == stop.x - 1)
                    {
                        roofTile = roofRight;
                        objectTile = (Random.Range(0, 2) == 0) ? storage[Random.Range(0, storage.Length)] : null;
                    }
                    else
                    {
                        roofTile = roofFlat;
                        objectTile = (Random.Range(0, 7) == 0) ? storage[Random.Range(0, storage.Length)] : null;
                    }
                }
                if (objectTile != null)
                    Instantiate(objectTile, tile, Quaternion.identity);
                if (roofTile != null)
                {
                    GameObject roofObject = Instantiate(roofTile, tile, Quaternion.identity);
                    Roof roof = roofObject.GetComponent<Roof>();
                    roof.setRoofIndex(house);
                    Roofs[house].Add(roof);
                }
                if (floor != null)
                {
                    Instantiate(floor, tile, Quaternion.identity);
                }
                gridPositions.Remove(tile);
            }
        }
    }

    void LayoutHouses()
    {
        int houseCount = Random.Range(this.houseCount.minimum, this.houseCount.maximum + 1);

        for (int i = 0; i < houseCount; i++)
        {
            int length = Random.Range(houseWallLength.minimum, houseWallLength.maximum);
            Vector2 position = RandomHousePosition(length);
            if (position == new Vector2(-1, -1))
                return;
            //string houseType = houseTypeAndFurniture.Keys.ToList()[Random.Range(0, houseTypeAndFurniture.Count)];
            //List<GameObject> furniture;
            //houseTypeAndFurniture.TryGetValue(houseType, out furniture);
            Vector2 start = new Vector2(0, Random.Range(0, length / 2)) + position;
            Vector2 stop = new Vector2((int) (position.x + length), (int) Random.Range(start.y + 5, position.y + length));
            Roofs.Add(new List<Roof>());
            int topRoomDoor = -1;
            int topStartX = -1;
            int topStopX = -1;
            int bottomRoomDoor = -1;
            int bottomStartX = -1;
            int bottomStopX = -1;
            int mainRoomDoor;
            if (start.y >= position.y + 3)
            {
                Vector2 roomStart = new Vector2(Random.Range(0, length / 2), 0) + position;
                bottomStartX = (int) roomStart.x;
                Vector2 roomStop = new Vector2((int) Random.Range(roomStart.x + 4, position.x + length), (int) (start.y - 1));
                bottomStopX = (int) roomStop.x;
                bottomRoomDoor = (int) Random.Range(roomStart.x + 1, roomStop.x - 1);
                mainRoomDoor = (int) Random.Range(roomStart.x + 1, roomStop.x - 1);
                LayoutRoom(roomStart, roomStop, "Bottom", i, storage, topRoomDoor, bottomRoomDoor, mainRoomDoor);
            }
            else
            {
                mainRoomDoor = (int) Random.Range(start.x + 1, stop.x - 1);
            }
            if (stop.y <= position.y + length - 3)
            {
                Vector2 roomStart = new Vector2((int) (position.x + Random.Range(0, length / 2)), (int) (stop.y + 1));
                topStartX = (int) roomStart.x;
                Vector2 roomStop = new Vector2((int) Random.Range(roomStart.x + 4, position.x + length), (int) (position.y + length));
                topStopX = (int) roomStop.x;
                topRoomDoor = (int) Random.Range(roomStart.x + 1, roomStop.x - 1);
                LayoutRoom(roomStart, roomStop, "Top", i, storage, topRoomDoor, bottomRoomDoor, mainRoomDoor);
            }

            LayoutRoom(start, stop, "Main", i, storage, topRoomDoor, bottomRoomDoor, mainRoomDoor, topStartX, topStopX, bottomStartX, bottomStopX);

            //tileChoice = triggerDoor;
            //GameObject instance = Instantiate(tileChoice, position, Quaternion.identity) as GameObject;
            //Vector2 randomPos = RandomPosition();
            //while (randomPos.x >= (int)randomPosition.x && randomPos.x <= (int)randomPosition.x + width && randomPos.y >= (int)randomPosition.y && randomPos.y <= (int)randomPosition.y + length) //ensure lever not in building
            //    randomPos = RandomPosition();
            //instance.GetComponent<Door>().SetupTrigger(randomPos);
            //continue;
        }
    }

    public List<List<Node>> SetupScene(int level)
    {
        BoardSetup();
        InitializeList();
        LayoutHouses();
        LayoutObjectAtRandom(objects, objectCount.minimum, objectCount.maximum);
        int enemyCount = (int)Mathf.Log(level, 2f) + 2; //added 2
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
        Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);
        //Instantiate(player, new Vector3(0, 0, 0f), Quaternion.identity);

        return Grid;
    }

    public List<List<Roof>> GetRoofs()
    {
        return Roofs;
    }
}
