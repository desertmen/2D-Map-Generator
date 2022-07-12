# 2D-Map-Generator
2D map generator is a system for creating randomised maps from prefabs of rooms and corridors

MAPCREATOR
empty gameobject with MapCreator script attached to it

- MapCreator creates the map using room, corridor and gate prefabs based on these settings:
  
  - Rooms = array of room prefabs
  
  - Corridors = array of pairs of straich and turn corridor prefabs

  - MaxRooms = maximal count of rooms generated

  - MinRooms = minimal count of rooms generated

  - MinRoomDist = minimal distance between each room

  - RoomIterations = how many times will the script try to fit room into map

  - HallwayDistFromCorners = minimal distance between corridor attached to the room wall and corners of the rooms
  
  - Scale = scale applied evenly to all part of the map

  - ShowGrid = shows grid where red rectangles represent occupied spaces and white represents empty spaces
  
  - ShowMapBoundaries = shows the boundaries of map according to Scale

  - PlayerTransform = transform of player GameObject

  - MapSize = size of the map (for scale of 1), if input equals (0, 0) size will be automaticaly calculated, this generaly results in larger map area
  
  - Create New Map = button for creating new map (new map is also created on Start)

  - TestSize = how many times will test itterate

  - Test Room Generator Settings = will try to generate map many times (specified by TestSize), prints in console how many % of attempts succeded in generating number of rooms
  
  - Test Map Generator Button = wil generate map many times (specified by TestSize), doesnt output anything int console

ROOM PREFABS
each room prefab needs to contain SizeScaler script and Room script in the root object

  - SizeScaler is used by main MapCreator script to scale rooms to desired size, it scales room based on largest sprite found in root objects children
  
  - Room script holds folowing information about each room
    - Size = Vector2Int referencing size of the room (size to which room will be transformed for MapCreator class scale of 1)
    - RoomName = name of the room
    - Gate = prefab of gate which will be created for this room
    - GateThickness = thickness of collider attached to gate (used for aligning gate collider with map collider)
    - Room Info = Scriptable object holding references to enemy prefabs and amount of spawned enemies for each type
    - Room Type = not used by script yet
    - Room script calls on SizeScaler to scale the room its attached to
    
CORRIDOR PREFABS
each corridor prefab needs to contain SizeScaler script and Hallway script in the root object

  - Hallway = class that holds Vector2Int of size of corridor and calls SizeScaler - for best performance input (1, 1)

GATE PREFABS
each gate prefab needs to contain SizeScaler script, Hallway script and GateScript script in the root object

  - HallWay = used for same purposes as in Corridor prefabs, to hold information about size of the game and to scale is by calling SizeScaler
  - GateScript = class controlling opening and closing animation of the gate and enabling/disabling its collider
  

ColliderCreator = Class holding methods for computing points for collider of the map

ConnectionFinder = Class holding methods for finding minimal spaning tree between rooms

GateScript = Class holding methods for manipulating with Gate

HallWay = Class used for holding information and scaling of corridors and gates

Heap = heap data structure used by ConnectionFinder class

MapCreator = main script generating map

MyDelaunator = class holding simplified methods for using Delaunay Triangulation algorithm by nol1fe - https://github.com/nol1fe/delaunator-sharp

Node = class used by PathFinder (represents nodes in A* algorithm)

PathFinder = A* algorithm used for finding paths of corridors between rooms

Room = class holding methods for holding information and scaling of room prefabs

RoomBehaviour = script controlling spawning of enemies in room when enetered

RoomInfo = scriptable object holding information about what and how many enemies to be spawned by RoomBehabiour in each room

SizeScaler = class holding methods for scaling objects according to largest sprite in children of object SizeScaler is attached to

MapCreatorEditor = custom Editor for MapCreator script
  
