using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCreator : MonoBehaviour
{
    public enum RoomTypes
    {
        Room,
    };

    [System.Serializable]
    public struct Corridor
    {
        public GameObject StraightHallway;
        public GameObject TurnHallWay;
        [HideInInspector]
        public Vector2 Position;
    }

    public Room[] rooms;
    public Corridor[] corridors;
    public int maxRooms;
    public int minRooms = 1;
    public int minRoomDist;
    public int roomIterations;
    public int hallwayDistFromCorners;
    public float wallThicknes;
    public float scale;
    public bool showGrid;
    public bool showMapBoundaries;
    public bool attractRooms;
    public Transform playerTransform;

    [HideInInspector]
    public int[] mapSize = new int[2];
    [HideInInspector]
    public int testSize;

    bool[,] grid;
    bool sendDebugMessages = true;
    private int failedAttemps = 0;
    private List<Room> builtRooms = new List<Room>();
    bool secondTry = false;

    private void Start()
    {
        generateMap();
    }

    public void generateMap()
    {
        transform.localScale = Vector3.one;
        Vector3 pos = transform.position;
        transform.position = Vector3.zero;
        correctInputs();
        deleteAllChildren();
        grid = createGrid();
        generateRooms();
        createCorridors();
        createCollider();
        secondTry = false;
        transform.position = pos;
        transform.localScale = scale * Vector3.one;
        scaleLights(scale * 2);
        spawnPlayer();
    }

    private void OnDrawGizmos()
    {
        if (showMapBoundaries == true)
        {
            Vector3 mapSize3 = new Vector3(mapSize[0] * scale, mapSize[1] * scale, 0);
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
                    Vector3 pos = new Vector3(x * scale, y * scale, 0) + transform.position + Vector3.one * 0.5f * scale;
                    Gizmos.DrawCube(pos, Vector3.one * scale * 0.9f);
                }
            }
        }
    }

    private void scaleLights(float scale)
    {
        foreach(Room room in builtRooms)
        {
            room.scaleLights(scale, room.transform.localScale.x);
        }
        Hallway[] hallWays = searchChildren<Hallway>(transform).ToArray();
        foreach (Hallway hallway in hallWays){
            if(hallway.gameObject.tag == "Gate")
            {
                continue;
            }
            hallway.scaleLights(scale, hallway.transform.localScale.x * 12);
        }
    }

    private List<T> searchChildren<T>(Transform parent)
    {
        List<T> list = new List<T>();
        searchChildren<T>(parent, list);
        return list;
    }

    private void searchChildren<T>(Transform parent, List<T> list)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var component = parent.GetChild(i).GetComponent<T>();
            if (component != null)
            {
                if (component.ToString().Equals("null") == false)
                {
                    list.Add(component);
                }
            }
            searchChildren(parent.GetChild(i), list);
        }
    }

    private void spawnPlayer()
    {
        int i = UnityEngine.Random.Range(0, builtRooms.Count);
        Room spawnRoom = builtRooms[i];
        RoomBehaviour roomBehaviour = spawnRoom.gameObject.GetComponent<RoomBehaviour>();
        roomBehaviour.enabled = false;
        playerTransform.position = ((Vector3)(spawnRoom.position) * scale + transform.position);
    }

    private void createCollider()
    {
        EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
        if (edgeCollider == null)
        {
            edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
        }
        Vector2[] colliderPoints = getColliderPoints();
        if (colliderPoints == null)
        {
            Debug.Log("Error");
            if(secondTry == false)
            {
                secondTry = true;
                //generateMap();
            }
            return;
        }
        Vector2[][] corners = getConerners(colliderPoints);
        Vector2[] cornerPoints = new Vector2[corners.Length + 1];
        applyWallThickness(corners);

        for (int i = 0; i < corners.Length; i++)
        {
            cornerPoints[i] = corners[i][0];
        }
        cornerPoints[cornerPoints.Length - 1] = corners[0][0];
        
        edgeCollider.points = cornerPoints;
        if (edgeCollider.pointCount < 4)
        {
            Debug.Log("Collider was not created succesfully");
            if (secondTry == false)
            {
                secondTry = true;
                generateMap();
            }
        }
    }

    private Vector2[][] getConerners(Vector2[] colliderPoints)
    {
        // TODO fix - doesnt add first point to the corners
        // go through collider points and find corners
        List<Vector2[]> corners = new List<Vector2[]>();
        for(int i = 0; i < colliderPoints.Length; i++)
        {
            Vector2 previous;
            if(i == 0)
            {
                previous = colliderPoints[colliderPoints.Length - 1];
            }
            else
            {
                previous = colliderPoints[i - 1];
            }
            Vector2 current = colliderPoints[i];
            Vector2 next;
            if(i == colliderPoints.Length - 1)
            {
                next = colliderPoints[0];
            }
            else
            {
                next = colliderPoints[i + 1];
            }
            Vector2 prevnext = next - previous;
            if(prevnext.x != 0 && prevnext.y != 0)
            {
                Vector2 dir = (next - current) + (previous - current);
                Vector2 testDir = (dir - Vector2.one) / 2;
                Vector2 gridPos = current + testDir;
                if(grid[(int)gridPos.x, (int)gridPos.y] == false)
                {
                    dir = -dir;
                }
                corners.Add(new Vector2[] { current, dir });
            }
        }
        return corners.ToArray();
    }

    private void applyWallThickness(Vector2[][] corners)
    {
        for(int i = 0; i < corners.Length; i++)
        {
            corners[i][0] += corners[i][1] * wallThicknes;
        }
    }

    private Vector2[] getColliderPoints()
    {
        // find point next to room
        Vector2Int startpos = Vector2Int.zero;
        foreach(Room room in builtRooms)
        {
            Vector2 pos1 = room.position - (Vector2)room.size/2f + Vector2.left;
            Vector2 pos2 = room.position - (Vector2)room.size/2f + Vector2.down;
            if(pos1.x >= 0 && pos1.x < grid.GetLength(0) && pos1.y >= 0 && pos1.y < grid.GetLength(1))
            {
                if(grid[(int)pos1.x, (int)pos1.y] == false)
                {
                    startpos = new Vector2Int((int)pos1.x, (int)pos1.y);
                    break;
                }
            }
            else if (pos2.x >= 0 && pos2.x < grid.GetLength(0) && pos2.y >= 0 && pos2.y < grid.GetLength(1))
            {
                if (grid[(int)pos2.x, (int)pos2.y] == false)
                {
                    startpos = new Vector2Int((int)pos2.x, (int)pos2.y);
                    break;
                }
            }
        }
        ColliderCreator colliderCreator = new ColliderCreator(grid, startpos.x, startpos.y);
        return (colliderCreator.getColliderPoints(0));
    }

    // TODO create gate on room - boxcollider goes a bit out to corridor (in line with room)
    private void createGate(Room room, Vector2 pos, Vector2 dir)
    {
        Vector2 absDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
        Vector2 offset = ((dir - absDir) / 2f).magnitude * Vector2.one;
        GameObject gate = Instantiate(room.gate);
        Hallway sizeScaler = gate.GetComponent<Hallway>();
        sizeScaler.scaleToSize();
        Vector3 gatePos = pos + new Vector2(dir.y, dir.x) / 2f - dir * wallThicknes - adjustForDir(sizeScaler.Size / 2f, dir) + dir * room.gateThickness / 2f + offset;
        gate.transform.position = gatePos;
        gate.transform.rotation = Quaternion.Euler(0, 0, Mathf.Abs(dir.y) * 90);
        gate.transform.parent = room.transform;
    }

    private Vector2 adjustForDir(Vector2 size, Vector2 dir)
    {
        if(dir.x != 0)
        {
            return size * dir;
        }
        else
        {
            return dir * new Vector2(size.y, size.x);
        }
    }

    // create all corridors
    private void createCorridors()
    {
        // delaunay triangulation of paths beween built rooms
        Vector2[] points = new Vector2[builtRooms.Count];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = builtRooms[i].position + new Vector2(transform.position.x, transform.position.y);
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
        Vector2[] startEndCoords = getStartEndCorridorCoords(room1, room2);

        if (isInsideRect(room1.position - (Vector2)room1.size/2f - Vector2.one, room1.position + (Vector2)room1.size / 2f, startEndCoords[0]) || 
            isInsideRect(room1.position - (Vector2)room1.size / 2f, room1.position + (Vector2)room1.size / 2f, startEndCoords[0]))
        {
            startEndCoords[0] += startEndCoords[2];
        }
        if (isInsideRect(room2.position - (Vector2)room2.size/2f - Vector2.one, room2.position + (Vector2)room2.size / 2f, startEndCoords[1]) ||
            isInsideRect(room2.position - (Vector2)room2.size / 2f, room2.position + (Vector2)room2.size / 2f, startEndCoords[1]))
        {
            startEndCoords[1] += startEndCoords[3];
        }
        createGate(room1, startEndCoords[0], startEndCoords[2]);
        createGate(room2, startEndCoords[1], startEndCoords[3]);
        createCorridorChunk(chooseCorridorSprite(startEndCoords[2], startEndCoords[2], corridor), startEndCoords[0], corridorParrent);
        createCorridorChunk(chooseCorridorSprite(startEndCoords[3], startEndCoords[3], corridor), startEndCoords[1], corridorParrent);
        startEndCoords[0] += startEndCoords[2];
        startEndCoords[1] += startEndCoords[3];

        PathFinder pathFinder = new PathFinder(grid);
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
            GameObject chosenCorridorObject = chooseCorridorSprite(lastDir, nextDir, corridor);

            createCorridorChunk(chosenCorridorObject, pathTiles[i], corridorParrent);
        }
    }

    private GameObject chooseCorridorSprite(Vector2 lastDir, Vector2 nextDir, Corridor corridor)
    {
        GameObject chosenCorridorObject;
        if(lastDir.x == nextDir.x || lastDir.y == nextDir.y)
        {
            if(lastDir.y == 0)
            {
                chosenCorridorObject = Instantiate(corridor.StraightHallway);
            }
            else
            {
                chosenCorridorObject = Instantiate(corridor.StraightHallway);
                chosenCorridorObject.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }
        else
        {
            if(lastDir.y == 1)
            {
                if (nextDir.x == 1)
                {
                    // UpRight
                    chosenCorridorObject = Instantiate(corridor.TurnHallWay);
                    chosenCorridorObject.transform.rotation = Quaternion.Euler(0, 0, -90);
                }
                else
                {
                    // UpLeft
                    chosenCorridorObject = Instantiate(corridor.TurnHallWay);
                    chosenCorridorObject.transform.rotation = Quaternion.Euler(0, 0, 180);
                }
            }
            else if(lastDir.y == -1)
            {
                if (nextDir.x == 1)
                {
                    // DownRight
                    chosenCorridorObject = Instantiate(corridor.TurnHallWay);
                }
                else
                {
                    // DownLeft
                    chosenCorridorObject = Instantiate(corridor.TurnHallWay);
                    chosenCorridorObject.transform.rotation = Quaternion.Euler(0, 0, 90);
                }
            }
            else if(lastDir.x == 1)
            {
                if(nextDir.y == 1)
                {
                    // DownLeft
                    chosenCorridorObject = Instantiate(corridor.TurnHallWay);
                    chosenCorridorObject.transform.rotation = Quaternion.Euler(0, 0, 90);
                }
                else
                {
                    // UpLeft
                    chosenCorridorObject = Instantiate(corridor.TurnHallWay);
                    chosenCorridorObject.transform.rotation = Quaternion.Euler(0, 0, 180);
                }
            }
            else
            {
                if(nextDir.y == 1)
                {
                    // DownRight
                    chosenCorridorObject = Instantiate(corridor.TurnHallWay);
                }
                else
                {
                    // UpRight
                    chosenCorridorObject = Instantiate(corridor.TurnHallWay);
                    chosenCorridorObject.transform.rotation = Quaternion.Euler(0, 0, -90);
                }
            }
        }
        return chosenCorridorObject;
    }

    // return start, end positions of corridor and directions from room1 room2
    private Vector2[] getStartEndCorridorCoords(Room room1, Room room2)
    {
        Vector2 room1Center = room1.position;
        Vector2 room2Center = room2.position;
        Vector2 dirFrom1to2 = room2Center - room1Center;
        Vector2 dir1 = getBinaryDir(room1, dirFrom1to2);
        Vector2 dir2 = getBinaryDir(room2, -dirFrom1to2);
        Vector2 posOnRoom1;
        Vector2 posOnRoom2;
        
        if((dirFrom1to2 * dir1).magnitude == 0 || (dirFrom1to2 * dir2).magnitude == 0)
        {
            posOnRoom1 = dir1 * room1.size / 2f;
            posOnRoom2 = dir2 * room2.size / 2f;
        }
        else
        {
            posOnRoom1 = dirFrom1to2 * (dir1 * room1.size/2f).magnitude / (dirFrom1to2 * dir1).magnitude;
            posOnRoom2 = -dirFrom1to2 * (dir2 * room2.size/2f).magnitude / (dirFrom1to2 * dir2).magnitude;
        }

        if(Mathf.Max(room1.size.x, room1.size.y)/2 <= hallwayDistFromCorners || Mathf.Max(room2.size.x, room2.size.y) / 2 <= hallwayDistFromCorners)
        {
            Debug.Log("hallway Dist From Corners is set too high this may result in unwanted results");
        }

        if((posOnRoom1 - (dir1 * room1.size / 2f)).magnitude > (new Vector2(dir1.y, dir1.x) * room1.size / 2f).magnitude - hallwayDistFromCorners)
        {
            posOnRoom1 -= (posOnRoom1 - (dir1 * room1.size / 2f)).normalized;
        }
        if ((posOnRoom2 - (dir2 * room2.size / 2f)).magnitude > (new Vector2(dir2.y, dir2.x) * room2.size / 2f).magnitude - hallwayDistFromCorners)
        {
            posOnRoom2 -= (posOnRoom2 - (dir2 * room2.size / 2f)).normalized;
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
    // returns one of 4 basic dirs between rooms depending on which is the closest
    private Vector2 getBinaryDir(Room room, Vector2 dir)
    {
        Vector2 direction = Vector2.zero;
        Vector2 roomCenter = room.position + (Vector2)room.size / 2f;
        // if dir is perpendicular
        if(dir.x == 0 || dir.y == 0)
        {
            return new Vector2(dir.normalized.x, dir.normalized.y);
        }
        // is on horizontal wall
        if(Mathf.Abs((dir * ((Vector2)room.size/2f).y / dir.y).x) < room.size.x / 2f )
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
            deleteAllChildren();
            grid = createGrid();
            generateRooms();
        }
        sendDebugMessages = true;
        float succesPercantage = ((float)(testSize - failedAttemps) / (float)testSize) * 100f;
        string percentageString = (Mathf.Round(succesPercantage)).ToString();
        Debug.Log("Map Test Finished: " + percentageString + "% of maps generated with requiered amount of rooms");
    }

    public void testMapGeneration()
    {
        sendDebugMessages = false;
        for (int i = 0; i < testSize; i++)
        {
            generateMap();
        }
        sendDebugMessages = true;
    }

    // generates rooms on map
    private void generateRooms()
    {
        int roomCount = UnityEngine.Random.Range(minRooms, maxRooms + 1);
        Room[] roomTypeRooms = getRoomsOfType(rooms, Room.RoomTypes.Room);
        int inFailedAttemps = failedAttemps;
        for (int i = 0; i < roomCount; i++)
        {
            int randRoomIndex = UnityEngine.Random.Range(0, roomTypeRooms.Length);
            Vector2 roomPosition = getValidRoomPosition(roomTypeRooms[randRoomIndex], roomIterations);
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
    private Room[] getRoomsOfType(Room[] rooms, Room.RoomTypes roomType)
    {
        List<Room> roomsOfType = new List<Room>();
        foreach(Room room in rooms)
        {
            if(room.roomType == roomType)
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
            Destroy(transform.GetChild(i).gameObject);
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
        if(minRoomDist < 4)
        {
            minRoomDist = 4;
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
            if(room.size.x > maxSize)
            {
                maxSize = (int)room.size.x;
            }
            if (room.size.y > maxSize)
            {
                maxSize = (int)room.size.y;
            }
        }
        maxSize = maxSize * (maxRooms + 1 );
        bool[,] grid = new bool[maxSize, maxSize];
        return grid;
    }

    // instantiates room sprite into scene and adds it to grid
    private void CreateRoom(Room room, Vector2 bottomLeft)
    {
        GameObject roomObject = Instantiate(room.gameObject);
        room = roomObject.GetComponent<Room>();
        RoomBehaviour roomBehaviour = roomObject.AddComponent<RoomBehaviour>();
        roomObject.tag = "Room";

        room.scaleToSize();

        // room trigger cretion
        BoxCollider2D roomTrigger = roomObject.AddComponent<BoxCollider2D>();
        roomTrigger.size = (Vector2)room.size / room.gameObject.transform.localScale.x;
        roomTrigger.isTrigger = true;

        room.position = bottomLeft + (Vector2)room.size/2f;
        room.updatePosition();

        roomObject.transform.parent = transform;

        if (room.size == Vector2.zero)
        {
            Debug.Log("room size of " + room.roomName + " sprite not set");
            DestroyImmediate(roomObject);
            return;
        }
        else
        {
            builtRooms.Add(room);
            fillRectInGrid(room, (int)bottomLeft.x, (int)bottomLeft.y);
            roomBehaviour.room = room;
            roomBehaviour.mapCreator = this;
        }
    }

    private void createCorridorChunk(GameObject corridorObject, Vector2 bottomLeft, GameObject parrent, string name = "corridor chunk")
    {
        Vector3 corridorPos = (Vector3)bottomLeft + new Vector3(0.5f, 0.5f, 0) + transform.position;
        Hallway hallwayScript = corridorObject.GetComponent<Hallway>();
        hallwayScript.scaleToSize();
        corridorObject.transform.position = corridorPos;
        corridorObject.name = name;
        corridorObject.transform.parent = parrent.transform;
        fillRectInGrid(Vector2.one, bottomLeft);
    }

    // fills 2D bool array acording to room size and position - LB = left bottom corner coords of rect
    private void fillRectInGrid(Room room, int LBx, int LBy)
    {
        int rectXSize = room.size.y;
        int rectYSize = room.size.x;

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
        for(int y = LBy - minRoomDist; y < LBy + room.size.y + minRoomDist; y++)
        {
            for(int x = LBx - minRoomDist; x < LBx + room.size.x + minRoomDist; x++)
            {
                if(x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1))
                {
                    // grid[x, y] = true if ocupied
                    if(grid[x, y])
                    {
                        return false;
                    }
                }
                else
                {
                    return false; 
                }
            }
        }
        return true;
    }

    // return a valid position where selected room fits
    private Vector2 getValidRoomPosition(Room room, int maxIterations = 100)
    {
        int iterations = 0;
        int mapWidth = grid.GetLength(0);
        int mapHeight = grid.GetLength(1);
        bool fits = false;
        Vector2 possibleRoomPos = Vector2.zero;

        while (!fits && iterations < maxIterations)
        {
            float x = UnityEngine.Random.Range(0, mapWidth);
            float y = UnityEngine.Random.Range(0, mapHeight);
            //possibleRoomPos = getRandomRoomPosition(room, 0, 0, grid.GetLength(0), grid.GetLength(1));
            possibleRoomPos = new Vector2(x, y);
            fits = roomFits(room, (int)possibleRoomPos.x, (int)possibleRoomPos.y);
            iterations++;
        }
        if (!fits)
        {
            return Vector2.left;
        }
        if(attractRooms == false)
        {
            return possibleRoomPos;
        }
        int its = 0;
        Vector2 dirToBLCorner = -possibleRoomPos.normalized;
        Vector2 floatPos = possibleRoomPos;
        Vector2 lastViablePos = Vector2.zero;
        while(fits && its<1000)
        {
            its++;
            lastViablePos = floatPos;
            floatPos += dirToBLCorner;
            fits = roomFits(room, (int)floatPos.x, (int)floatPos.y);
        }
        return new Vector2((int)(lastViablePos.x), (int)(lastViablePos.y));
    }

    // return random room position where room could but doesnt have to fit
    private Vector2 getRandomRoomPosition(Room room, int startX, int startY, int endX, int endY)
    {
        Vector2 newPos = new Vector2(startX, startY);
        if(room.size.x * 2 <= endX - startX && room.size.y * 2 <= endY - startY)
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
