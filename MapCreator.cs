using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapCreator : MonoBehaviour
{
    public enum RoomTypes
    {
        Room,
    };

    [System.Serializable]
    public struct Room
    {
        public string RoomName;
        public Sprite RoomSprite;
        public Material SpriteMaterial;
        public RoomTypes RoomType;
        public Vector2 Size;
        [HideInInspector]
        public Vector2 Position;
        [HideInInspector]
        public GameObject RoomObject;
    };

    [System.Serializable]
    public struct Corridor
    {
        [HideInInspector]
        public int Size;
        public Material SpriteMaterial;
        public Sprite VerticalHallway;
        public Sprite HorizontalHallway;
        public Sprite UpLeftTurnHallway;
        public Sprite UpRightTurnHallway;
        public Sprite DownLeftTurnHallway;
        public Sprite DownRightTurnHallway;
        [HideInInspector]
        public Vector2 Position;
        [HideInInspector]
        public GameObject CorridorObject;
    }

    public Room[] rooms;
    public Corridor[] corridors;
    public int maxRooms;
    public int minRooms = 1;
    public int minRoomDist;
    public int hallwayDistFromCorners;
    public bool showGrid;
    public bool showMapBoundaries;

    [HideInInspector]
    public int[] mapSize = new int[2];
    [HideInInspector]
    public int testSize;

    bool[,] grid;
    bool sendDebugMessages = true;
    private int failedAttemps = 0;
    private List<Room> builtRooms = new List<Room>();

    private void Start()
    {
        generateMap();
    }

    public void generateMap()
    {
        // create rooms
        roundSize();
        correctInputs();
        deleteAllChildren();
        grid = createGrid();
        generateRooms();
        createCorridors();
    }

    private void OnDrawGizmos()
    {
        if(showMapBoundaries == true)
        {
            Vector3 mapSize3 = new Vector3(mapSize[0], mapSize[1], 0);
            Gizmos.DrawWireCube(transform.position + mapSize3 / 2f, mapSize3);
        }
        if(showGrid == true)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    if (grid[x, y])
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Color.white;
                    }
                    Vector3 pos = new Vector3(x, y, 0) + transform.position + Vector3.one * 0.5f;
                    Gizmos.DrawCube(pos, Vector3.one * 0.9f);
                }
            }
        }
    }

    // create all corridors
    private void createCorridors()
    {
        // delaunay triangulation of paths beween built rooms
        Vector2[] points = new Vector2[builtRooms.Count];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = builtRooms[i].Position + builtRooms[i].Size / 2f + new Vector2(transform.position.x, transform.position.y);
        }
        if (points.Length < 2)
        {
            return;
        }
        MyDelaunator myDelaunator = new MyDelaunator(points);
        Vector2[][] edges = myDelaunator.getEdges();

        // connect MST
        ConnectionsFinder connectionsFinder = new ConnectionsFinder(edges, points);
        int[][] connections = connectionsFinder.getMST();
        for (int i = 0; i < connections.Length; i++)
        {
            createCorridor(builtRooms[connections[i][0]], builtRooms[connections[i][1]], corridors[0]);
        }
    }

    // create one corridor
    private void createCorridor(Room room1, Room room2, Corridor corridor)
    {
        GameObject corridorParrent = new GameObject("corridor");
        corridorParrent.transform.parent = transform;
        PathFinder pathFinder = new PathFinder(grid);
        Vector2[] startEndCoords = getStartEndCorridorCoords(room1, room2);
        Vector2 pos = transform.position;

        if (isInsideRect(room1.Position - Vector2.one, room1.Position + room1.Size, startEndCoords[0]) || isInsideRect(room1.Position, room1.Position + room1.Size, startEndCoords[0] + pos))
        {
            startEndCoords[0] += startEndCoords[2];
        }
        if (isInsideRect(room2.Position - Vector2.one, room2.Position + room2.Size, startEndCoords[1]) || isInsideRect(room2.Position, room2.Position + room2.Size, startEndCoords[1] + pos))
        {
            startEndCoords[1] += startEndCoords[3];
        }

        List<Node> path = pathFinder.findPath((int)(startEndCoords[0].x), (int)(startEndCoords[0].y), (int)(startEndCoords[1].x), (int)(startEndCoords[1].y), startEndCoords[2], startEndCoords[3]);
        if(path == null)
        {
            return;
        }
        Vector2[] pathTiles = pathFinder.getPathPositions(path);
        Vector2 lastDir = Vector2.zero;
        Vector2 nextDir = Vector2.zero;
        for(int i = 0; i < pathTiles.Length; i++)
        {
            if(i < 1)
            {
                lastDir = startEndCoords[2];
                nextDir = pathTiles[i + 1] - pathTiles[i];
            }
            else if(i > pathTiles.Length - 2)
            {
                nextDir = -startEndCoords[3];
                lastDir = pathTiles[i] - pathTiles[i - 1];
            }
            else
            {
                lastDir = pathTiles[i] - pathTiles[i - 1];
                nextDir = pathTiles[i + 1] - pathTiles[i];
            }
            Sprite chosenSprite = chooseCorridorSprite(lastDir, nextDir, corridor);

            createCorridorChunk(chosenSprite, corridor.SpriteMaterial, corridor.Size * Vector2.one, pathTiles[i] + pos, corridorParrent);
        }
    }

    private Sprite chooseCorridorSprite(Vector2 lastDir, Vector2 nextDir, Corridor corridor)
    {
        Sprite chosenSprite;
        if(lastDir.x == nextDir.x || lastDir.y == nextDir.y)
        {
            if(lastDir.y == 0)
            {
                chosenSprite = corridor.HorizontalHallway;
            }
            else
            {
                chosenSprite = corridor.VerticalHallway;
            }
        }
        else
        {
            if(lastDir.y == 1)
            {
                if (nextDir.x == 1)
                {
                    chosenSprite = corridor.UpRightTurnHallway;
                }
                else
                {
                    chosenSprite = corridor.UpLeftTurnHallway;
                }
            }
            else if(lastDir.y == -1)
            {
                if (nextDir.x == 1)
                {
                    chosenSprite = corridor.DownRightTurnHallway;
                }
                else
                {
                    chosenSprite = corridor.DownLeftTurnHallway;
                }
            }
            else if(lastDir.x == 1)
            {
                if(nextDir.y == 1)
                {
                    chosenSprite = corridor.DownLeftTurnHallway;
                }
                else
                {
                    chosenSprite = corridor.UpLeftTurnHallway;
                }
            }
            else
            {
                if(nextDir.y == 1)
                {
                    chosenSprite = corridor.DownRightTurnHallway;
                }
                else
                {
                    chosenSprite = corridor.UpRightTurnHallway;
                }
            }
        }
        return chosenSprite;
    }

    private Vector2[] getStartEndCorridorCoords(Room room1, Room room2)
    {
        Vector2 room1Center = room1.Position + room1.Size / 2f;
        Vector2 room2Center = room2.Position + room2.Size / 2f;
        Vector2 dirFrom1to2 = room2Center - room1Center;
        Vector2 dir1 = getBinaryDir(room1, dirFrom1to2);
        Vector2 dir2 = getBinaryDir(room2, -dirFrom1to2);
        Vector2 posOnRoom1;
        Vector2 posOnRoom2;
        
        if((dirFrom1to2 * dir1).magnitude == 0 || (dirFrom1to2 * dir2).magnitude == 0)
        {
            posOnRoom1 = dir1 * room1.Size / 2f;
            posOnRoom2 = dir2 * room2.Size / 2f;
        }
        else
        {
            posOnRoom1 = dirFrom1to2 * (dir1 * room1.Size/2f).magnitude / (dirFrom1to2 * dir1).magnitude;
            posOnRoom2 = -dirFrom1to2 * (dir2 * room2.Size/2f).magnitude / (dirFrom1to2 * dir2).magnitude;
        }

        if(Mathf.Max(room1.Size.x, room1.Size.y)/2 <= hallwayDistFromCorners || Mathf.Max(room2.Size.x, room2.Size.y) / 2 <= hallwayDistFromCorners)
        {
            Debug.Log("hallway Dist From Corners is set too high this may result in unwanted results");
        }

        if((posOnRoom1 - (dir1 * room1.Size / 2f)).magnitude > (new Vector2(dir1.y, dir1.x) * room1.Size / 2f).magnitude - hallwayDistFromCorners)
        {
            posOnRoom1 -= (posOnRoom1 - (dir1 * room1.Size / 2f)).normalized;
        }
        if ((posOnRoom2 - (dir2 * room2.Size / 2f)).magnitude > (new Vector2(dir2.y, dir2.x) * room2.Size / 2f).magnitude - hallwayDistFromCorners)
        {
            posOnRoom2 -= (posOnRoom2 - (dir2 * room2.Size / 2f)).normalized;
        }

        if (dir1 == Vector2.down || dir1 == Vector2.left)
        {
            posOnRoom1 += dir1;
        }
        if (dir2 == Vector2.down || dir2 == Vector2.left)
        {
            posOnRoom1 += dir2;
        }

        Vector2[] startEndCoords = new Vector2[4] { Vector2Round(room1Center + posOnRoom1), Vector2Round(room2Center + posOnRoom2), dir1, dir2 };

        return startEndCoords;
    }

    private Vector2 Vector2Round(Vector2 vec)
    {
        return new Vector2((int)vec.x, (int)vec.y);
    }
    private Vector2 getBinaryDir(Room room, Vector2 dir)
    {
        Vector2 direction = Vector2.zero;
        Vector2 roomCenter = room.Position + room.Size / 2f;
        // if dir is perpendicular
        if(dir.x == 0 || dir.y == 0)
        {
            return new Vector2(dir.normalized.x, dir.normalized.y);
        }
        // is on horizontal wall
        if(Mathf.Abs((dir * (room.Size/2f).y / dir.y).x) < room.Size.x / 2f )
        {
            if(dir.y > 0)
            {
                direction = Vector2.up;
            }
            else
            {
                direction = Vector2.down;
            }
        }
        // is on vertical wall
        else
        {
            if (dir.x > 0)
            {
                direction = Vector2.right;
            }
            else
            {
                direction = Vector2.left;
            }
        }
        return direction;
    }

    // runs a test to see how correct are your settings
    public void testMapSize()
    {
        sendDebugMessages = false;
        failedAttemps = 0;
        for(int i = 0; i < testSize; i++)
        {
            // create rooms
            roundSize();
            deleteAllChildren();
            grid = createGrid();
            generateRooms();
        }
        sendDebugMessages = true;
        float succesPercantage = ((float)(testSize - failedAttemps) / (float)testSize) * 100f;
        string percentageString = (Mathf.Round(succesPercantage)).ToString();
        Debug.Log("Map Test Finished: " + percentageString + "% of maps generated with requiered amount of rooms");
    }

    // generates rooms on map
    private void generateRooms()
    {
        int roomCount = UnityEngine.Random.Range(minRooms, maxRooms + 1);
        Room[] roomTypeRooms = getRoomsOfType(rooms, RoomTypes.Room);
        int inFailedAttemps = failedAttemps;
        for (int i = 0; i < roomCount; i++)
        {
            int randRoomIndex = UnityEngine.Random.Range(0, roomTypeRooms.Length);
            Vector2 roomPosition = getValidRoomPosition(roomTypeRooms[randRoomIndex]);
            if(roomPosition == Vector2.left)
            {
                failedAttemps++;
            }
            else
            {
                CreateRoom(roomTypeRooms[randRoomIndex], roomPosition);
            }

        }
        if(inFailedAttemps < failedAttemps)
        {
            failedAttemps = inFailedAttemps + 1;
            if (sendDebugMessages || roomCount != builtRooms.Count)
            {
                Debug.Log((roomCount - builtRooms.Count).ToString() + " rooms" + " didnt fit in the map");
            }
        }
    }

    // return array of rooms of type
    private Room[] getRoomsOfType(Room[] rooms, RoomTypes roomType)
    {
        List<Room> roomsOfType = new List<Room>();
        foreach(Room room in rooms)
        {
            if(room.RoomType == roomType)
            {
                roomsOfType.Add(room);
            }
        }
        return roomsOfType.ToArray();
    }

    private void deleteAllChildren()
    {
        builtRooms = new List<Room>();
        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    // corrects inputs from inspector (used by MapCreatorEditor)
    public void correctInputs()
    {
        if(maxRooms < 0)
        {
            maxRooms = 0;
        }
        if(minRooms < 0)
        {
            minRooms = 0;
        }
        if(minRoomDist < 0)
        {
            minRoomDist = 0;
        }
        if(minRoomDist < 3)
        {
            minRoomDist = 3;
        }
        if (mapSize[0] < 0)
        {
            mapSize[0] = 0;
        }
        if(mapSize[1] < 0)
        {
            mapSize[0] = 0;
        }
        if(minRooms > maxRooms)
        {
            minRooms = maxRooms;
        }
        for(int i = 0; i < corridors.Length; i++)
        {
            corridors[i].Size = 1;
        }
    }

    // rounds size of all rooms
    private void roundSize()
    {
        for(int i = 0; i < rooms.Length; i++)
        {
            rooms[i].Size.x = Mathf.Round(rooms[i].Size.x);
            rooms[i].Size.y = Mathf.Round(rooms[i].Size.y);
        }
    }

    // create 2D bool array where each room / corridor ocupies a space
    private bool[,] createGrid()
    {
        if(mapSize[0] != 0 && mapSize[1] != 0)
        {
            return new bool[mapSize[0], mapSize[1]];
        }

        int maxSize = 0;
        foreach (Room room in rooms)
        {
            if(room.Size.x > maxSize)
            {
                maxSize = (int)room.Size.x;
            }
            if (room.Size.y > maxSize)
            {
                maxSize = (int)room.Size.y;
            }
        }
        maxSize = maxSize * (maxRooms + 1 );
        bool[,] grid = new bool[maxSize, maxSize];
        return grid;
    }

    // instantiates room sprite into scene and adds it to grid
    private void CreateRoom(Room room, Vector2 bottomLeft)
    {
        GameObject roomObject = new GameObject(room.RoomName);
        SpriteRenderer roomSpriteRenderer = roomObject.AddComponent<SpriteRenderer>();
        roomObject.transform.parent = transform;
        roomObject.transform.localPosition = bottomLeft + room.Size/2f;
        

        roomSpriteRenderer.sprite = room.RoomSprite;
        roomSpriteRenderer.sharedMaterial = room.SpriteMaterial;
        roomSpriteRenderer.sortingOrder = 2;

        room.Position = bottomLeft;
        room.RoomObject = roomObject;

        if (room.Size == Vector2.zero)
        {
            Debug.Log("room size of " + room.RoomName + " sprite not set");
            DestroyImmediate(roomObject);
            return;
        }
        else
        {
            roomSpriteRenderer.drawMode = SpriteDrawMode.Sliced;
            roomSpriteRenderer.size = room.Size;
            builtRooms.Add(room);
            fillRectInGrid(room, (int)bottomLeft.x, (int)bottomLeft.y);
        }
    }

    private void createCorridorChunk(Sprite corridorSprite, Material corridorMaterial, Vector2 corridorSize, Vector2 bottomLeft, GameObject parrent, string name = "corridor chunk")
    {
        GameObject roomObject = new GameObject(name);
        SpriteRenderer roomSpriteRenderer = roomObject.AddComponent<SpriteRenderer>();
        Vector2 pos = transform.position;

        roomObject.transform.parent = parrent.transform;
        roomObject.transform.localPosition = bottomLeft + corridorSize / 2f;


        roomSpriteRenderer.sprite = corridorSprite;
        roomSpriteRenderer.sharedMaterial = corridorMaterial;
        roomSpriteRenderer.sortingOrder = 1;

        if (corridorSize == Vector2.zero)
        {
            Debug.Log("size of corridoor not set");
            DestroyImmediate(roomObject);
            return;
        }
        else
        {
            roomSpriteRenderer.drawMode = SpriteDrawMode.Sliced;
            roomSpriteRenderer.size = corridorSize;
            fillRectInGrid(corridorSize, bottomLeft-pos);
        }
    }

    // fills 2D bool array acording to room size and position - LB = left bottom corner coords of rect
    private void fillRectInGrid(Room room, int LBx, int LBy)
    {
        int rectXSize = (int)room.Size.y;
        int rectYSize = (int)room.Size.x;

        for(int y = 0; y < rectXSize; y++)
        {
            for(int x = 0; x <rectYSize; x++)
            {
                if(x + LBx >= 0 && x + LBx < grid.GetLength(0) && y + LBy >= 0 && y + LBy< grid.GetLength(1))
                {
                    grid[LBx + x, LBy + y] = true;
                }
            }
        }
    }

    private void fillRectInGrid(Vector2 corridorSize, Vector2 LBcorner)
    {
        int size = (int)corridorSize.x;
        int startX = (int)LBcorner.x;
        int startY = (int)LBcorner.y;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (x + startX >= 0 && x + startX < grid.GetLength(0) && y + startY >= 0 && y + startY < grid.GetLength(1))
                {
                    grid[startX + x, startY + y] = true;
                }
            }
        }
    }

    // is there enought space to create room
    private bool roomFits(Room room, int LBx, int LBy)
    {
        for(int y = LBy - minRoomDist - (int)room.Size.y; y < LBy + room.Size.y + minRoomDist; y++)
        {
            for(int x = LBx - minRoomDist - (int)room.Size.x; x < LBx + room.Size.x + minRoomDist; x++)
            {
                if(x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1))
                {
                    // grid[x, y] = true if ocupied
                    if(grid[x, y])
                    {
                        return false;
                    }
                }
                else{ return false; }
            }
        }
        return true;
    }

    // return a valid position where selected room fits
    private Vector2 getValidRoomPosition(Room room, int maxIterations = 100)
    {
        int iterations = 0;
        bool fits = false;
        Vector2 possibleRoomPos = Vector2.zero;

        while (!fits && iterations < maxIterations)
        {
            possibleRoomPos = getRandomRoomPosition(room, 0, 0, grid.GetLength(0), grid.GetLength(1));
            fits = roomFits(room, (int)possibleRoomPos.x, (int)possibleRoomPos.y);
            iterations++;
        }
        if (!fits)
        {
            return Vector2.left;
        }
        return possibleRoomPos;

    }

    // return random room position where room could but doesnt have to fit
    private Vector2 getRandomRoomPosition(Room room, int startX, int startY, int endX, int endY)
    {
        Vector2 newPos = new Vector2(startX, startY);
        if(room.Size.x * 2 <= endX - startX && room.Size.y * 2 <= endY - startY)
        {
            int randX = UnityEngine.Random.Range(0, 2);
            int randY = UnityEngine.Random.Range(0, 2);
            int changeDir = UnityEngine.Random.Range(0, 2);

            if(changeDir == 0)
            {
                startX = startX + randX * (endX - startX) / 2;
                endX = startX + (randX + 1) * (endX - startX) / 2;
            }
            else 
            {
                startY = startY + randY * (endY - startY) / 2;
                endY = startY + (randY + 1) * (endY - startY) / 2;
            }


            newPos = getRandomRoomPosition(room, startX, startY, endX, endY);
        }

        return newPos;
    }
    private bool isInsideRect(Vector2 BLcorner, Vector2 TRcorner, Vector2 point)
    {
        Vector2 BL = new Vector2(Mathf.Min(BLcorner.x, TRcorner.x), Mathf.Min(BLcorner.y, TRcorner.y));
        Vector2 TR = new Vector2(Mathf.Max(BLcorner.x, TRcorner.x), Mathf.Max(BLcorner.y, TRcorner.y));
        if(BL.x < point.x && point.x < TR.x && BL.y < point.y && point.y < TR.y)
        {
            return true;
        }

        return false;
    }
}
