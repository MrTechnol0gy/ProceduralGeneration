using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject[] floorTiles; // Array of floor tile prefabs
    public GameObject[] roomFloorTiles; // Array of room floor tile prefabs
    public GameObject[] enviroTiles; // Dirt tile prefab
    public GameObject[] wallTiles; // Array of wall tile prefabs
    public GameObject[] doorTiles; // Array of door tile prefabs
    public GameObject[] floorProps; // Array of props to spawn in the dungeon
    public GameObject[] pillarTiles; // Array of pillars to spawn in the dungeon
    public int dungeonWidth = 10; // Width of the floor
    public int dungeonLength = 10; // Length of the floor
    private int numberOfRooms; // Number of rooms to generate; based on size of dungeon
    public float tileSize = 1.0f; // Size of each tile
    public float gridSpacing = 1.0f; // Spacing between tiles
    private float timeElapsed = 0.0f; // Time elapsed since the start of the game

    private int[,] dungeonFloorMap;
    private int[,] dungeonWallMap;
    private int[,] dungeonPropsMap;
    private int[,] dungeonPillarsMap;
    private bool addDoors = true;

    public enum TileType
    {
        Empty,
        StandardTile,
        RoomTile,
        Door,
        EnviroTile,
        FloorProps,
        Pillars
    }
    private class RoomParameters
    {
        public int minSize = 3;
        public int maxSize = 5;
    }

    private RoomParameters roomParams = new RoomParameters();
    private List<Room> rooms = new List<Room>();

    private class Room
    {
        public int x;
        public int z;
        public int width;
        public int length;

        public Room(int x, int z, int width, int length)
        {
            this.x = x;
            this.z = z;
            this.width = width;
            this.length = length;
        }
    }

    void Awake()
    {
        numberOfRooms = dungeonWidth * dungeonLength / 30;
        dungeonFloorMap = new int[dungeonWidth, dungeonLength];
        dungeonWallMap = new int[dungeonWidth, dungeonLength];
        dungeonPropsMap = new int[dungeonWidth, dungeonLength];
        dungeonPillarsMap = new int[dungeonWidth, dungeonLength];
    }

    void Start()
    {
        GenerateDungeon();
    }

    void Update()
    {
        // Increment the time elapsed
        timeElapsed += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearDungeon();
            GenerateDungeon();
            timeElapsed = 0.0f;
        }
        // else if three seconds have passed, clear the dungeon and generate a new one
        else if (timeElapsed > 5.0f)
        {
            timeElapsed = 0.0f;
            ClearDungeon();
            GenerateDungeon();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void GenerateDungeon()
    {
        // Initialize the dungeon maps
        InitializeMaps();
        // Generate the rooms
        GenerateRooms();
        // Smooth the dungeon
        SmoothDungeon();
        // Fills in all areas with floors
        GenerateFloor();
        // Fills in all walls and doors
        GenerateWalls();
        // Fills in ceilings
        GenerateCeilings();
        // Fills in pillars
        GeneratePillars();
        // Fills in all props
        GenerateProps();
    }

    void ClearDungeon()
    {
        // Destroy all child objects
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        // Clear the list of rooms
        rooms.Clear();
    }

    void InitializeMaps()
    {
        // Initialize the dungeon floor map to all 0s
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int z = 0; z < dungeonLength; z++)
            {
                dungeonFloorMap[x, z] = 0;
            }
        }

        // Initialize the dungeon wall map to all 0s
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int z = 0; z < dungeonLength; z++)
            {
                dungeonWallMap[x, z] = 0;
            }
        }

        // Initialize the dungeon props map to all 0s
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int z = 0; z < dungeonLength; z++)
            {
                dungeonPropsMap[x, z] = 0;
            }
        }

        // Initialize the dungeon pillars map to all 0s
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int z = 0; z < dungeonLength; z++)
            {
                dungeonPillarsMap[x, z] = 0;
            }
        }
    }
    void GenerateFloor()
    {
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int z = 0; z < dungeonLength; z++)
            {
                GameObject tilePrefab = null;
                string tileNamePrefix = "";

                switch ((TileType)dungeonFloorMap[x, z])
                {
                    case TileType.Empty:
                        tilePrefab = floorTiles[Random.Range(0, floorTiles.Length)];
                        tileNamePrefix = "FloorTile";
                        break;
                    case TileType.StandardTile:
                        tilePrefab = floorTiles[Random.Range(0, floorTiles.Length)];
                        tileNamePrefix = "FloorTile";
                        break;
                    case TileType.RoomTile:
                        tilePrefab = roomFloorTiles[Random.Range(0, roomFloorTiles.Length)];
                        tileNamePrefix = "RoomFloorTile";
                        break;
                    case TileType.EnviroTile:
                        tilePrefab = enviroTiles[Random.Range(0, enviroTiles.Length)];
                        tileNamePrefix = "EnviroTile";
                        break;
                    default:
                        break;
                }

                if (tilePrefab != null)
                {
                    Vector3 position = new Vector3(
                    x * (tileSize + gridSpacing),
                    0,
                    z * (tileSize + gridSpacing)
                    );

                    if (tileNamePrefix != "")
                    {
                        // Correct the position for room floor tiles due to offset
                        if (tileNamePrefix == "RoomFloorTile")
                        {
                            position.z += tileSize;
                            // If a RoomTile, a random chance to add a prop to the Props map
                            int randomChance = Random.Range(0, 100);
                            if (randomChance < 10)
                            {
                                dungeonPropsMap[x, z] = TileType.FloorProps.GetHashCode();
                            }
                        }

                        GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                        tile.name = tileNamePrefix + "_" + x + "_" + z;                                               
                    }
                }
            }
        }
    }
    void GenerateCeilings()
    {
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int z = 0; z < dungeonLength; z++)
            {
                GameObject tilePrefab = null;
                string tileNamePrefix = "";

                switch ((TileType)dungeonFloorMap[x, z])
                {
                    case TileType.EnviroTile:
                        tilePrefab = enviroTiles[Random.Range(0, enviroTiles.Length)];
                        tileNamePrefix = "EnviroTile";
                        break;
                    default:
                        break;

                }
                if (tilePrefab != null)
                {
                    Vector3 position = new Vector3(
                    x * (tileSize + gridSpacing),
                    0 + tileSize,
                    z * (tileSize + gridSpacing)
                    );
                    GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                    tile.name = tileNamePrefix + "_" + x + "_" + z; 
                }
            }
        }
    }
    void GenerateWalls()
    {
        // Generate wall tiles on the outer edge of the map.
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int z = 0; z < dungeonLength; z++)
            {
                if (x == 0 || x == dungeonWidth - 1)
                {
                    // Randomly select a wall tile from your array.
                    int randomTileIndex = Random.Range(0, wallTiles.Length);
                    GameObject wallTile = wallTiles[randomTileIndex];

                    // Calculate the position for the wall tile.
                    Vector3 position = new Vector3(
                        x * (tileSize + gridSpacing),
                        0,
                        z * (tileSize + gridSpacing)
                    );

                    // Update the dungeon layout map to indicate a wall tile.
                    dungeonWallMap[x, z] = TileType.StandardTile.GetHashCode();

                    // Instantiate the wall tile at the calculated position, as a child of this transform.
                    GameObject wallTileInstance = Instantiate(wallTile, position, Quaternion.identity, transform);

                    // Determine the rotation based on position
                    if (x == 0)
                    {
                        // Rotate wall tiles along the left edge and move to the opposite side.
                        wallTileInstance.transform.rotation = Quaternion.Euler(0, 90, 0);
                        wallTileInstance.transform.position = new Vector3(
                            wallTileInstance.transform.position.x - tileSize, // Move to the opposite side
                            wallTileInstance.transform.position.y,
                            wallTileInstance.transform.position.z
                        );
                    }
                    else if (x == dungeonWidth - 1)
                    {
                        // Rotate wall tiles along the right edge.
                        wallTileInstance.transform.rotation = Quaternion.Euler(0, 90, 0);
                    }
                }
            
                if (z == 0 || z == dungeonLength - 1)
                {
                    // Calculate the position for the wall tile.
                    Vector3 position = new Vector3(
                        x * (tileSize + gridSpacing),
                        0,
                        z * (tileSize + gridSpacing)
                    );
                    // Randomly select a wall tile from your array.
                    int randomTileIndex = Random.Range(0, wallTiles.Length);
                    GameObject wallTile = wallTiles[randomTileIndex];

                    // Update the dungeon layout map to indicate a wall tile.
                    dungeonWallMap[x, z] = TileType.StandardTile.GetHashCode();

                    // Instantiate the wall tile at the calculated position, as a child of this transform.
                    GameObject wallTileInstance = Instantiate(wallTile, position, Quaternion.identity, transform);

                    // Determine the rotation based on position
                    if (z == 0)
                    {
                        // Rotate wall tiles along the bottom edge.
                        wallTileInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                    else if (z == dungeonLength - 1)
                    {
                        // Rotate wall tiles along the top edge and move to the opposite side.
                        wallTileInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
                        wallTileInstance.transform.position = new Vector3(
                            wallTileInstance.transform.position.x,
                            wallTileInstance.transform.position.y,
                            wallTileInstance.transform.position.z + tileSize // Move to the opposite side
                        );
                    }
                }
            }
        }
        // Generate walls for rooms
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int z = 0; z < dungeonLength; z++)
            {
                if (dungeonFloorMap[x, z] == TileType.RoomTile.GetHashCode())
                {
                    // Check if the tile to the left is not a room tile
                    if (x > 0 && dungeonFloorMap[x - 1, z] != TileType.RoomTile.GetHashCode())
                    {
                        // Randomly select a wall tile from your array.
                        int randomTileIndex = Random.Range(0, wallTiles.Length);
                        GameObject wallTile = wallTiles[randomTileIndex];

                        // Calculate the position for the wall tile.
                        Vector3 position = new Vector3(
                            x * (tileSize + gridSpacing),
                            0,
                            z * (tileSize + gridSpacing)
                        );
                        // if the wallMap indicates a door, create a door
                        if (dungeonWallMap[x, z] == TileType.Door.GetHashCode())
                        {
                            // Randomly select a door tile from your array.
                            randomTileIndex = Random.Range(0, doorTiles.Length);
                            wallTile = doorTiles[randomTileIndex];
                        }
                        // Instantiate the wall tile at the calculated position, as a child of this transform.
                        GameObject wallTileInstance = Instantiate(wallTile, position, Quaternion.identity, transform);

                        // Rotate wall tiles along the left edge and move to the opposite side.
                        wallTileInstance.transform.rotation = Quaternion.Euler(0, 90, 0);
                        wallTileInstance.transform.position = new Vector3(
                            wallTileInstance.transform.position.x - tileSize, // Move to the opposite side
                            wallTileInstance.transform.position.y,
                            wallTileInstance.transform.position.z
                        );
                    }
                    // Check if the tile to the right is not a room tile
                    if (x < dungeonWidth - 1 && dungeonFloorMap[x + 1, z] != TileType.RoomTile.GetHashCode())
                    {
                        // Randomly select a wall tile from your array.
                        int randomTileIndex = Random.Range(0, wallTiles.Length);
                        GameObject wallTile = wallTiles[randomTileIndex];

                        // Calculate the position for the wall tile.
                        Vector3 position = new Vector3(
                            x * (tileSize + gridSpacing),
                            0,
                            z * (tileSize + gridSpacing)
                        );

                        // if the wallMap indicates a door, create a door
                        if (dungeonWallMap[x, z] == TileType.Door.GetHashCode())
                        {
                            // Randomly select a door tile from your array.
                            randomTileIndex = Random.Range(0, doorTiles.Length);
                            wallTile = doorTiles[randomTileIndex];
                        }

                        // Instantiate the wall tile at the calculated position, as a child of this transform.
                        GameObject wallTileInstance = Instantiate(wallTile, position, Quaternion.identity, transform);

                        // Rotate wall tiles along the right edge.
                        wallTileInstance.transform.rotation = Quaternion.Euler(0, 90, 0);
                    }
                    // Check if the tile to the bottom is not a room tile
                    if (z > 0 && dungeonFloorMap[x, z - 1] != TileType.RoomTile.GetHashCode())
                    {
                        // Randomly select a wall tile from your array.
                        int randomTileIndex = Random.Range(0, wallTiles.Length);
                        GameObject wallTile = wallTiles[randomTileIndex];

                        // Calculate the position for the wall tile.
                        Vector3 position = new Vector3(
                            x * (tileSize + gridSpacing),
                            0,
                            z * (tileSize + gridSpacing)
                        );

                        // if the wallMap indicates a door, create a door
                        if (dungeonWallMap[x, z] == TileType.Door.GetHashCode())
                        {
                            // Randomly select a door tile from your array.
                            randomTileIndex = Random.Range(0, doorTiles.Length);
                            wallTile = doorTiles[randomTileIndex];
                        }

                        // Instantiate the wall tile at the calculated position, as a child of this transform.
                        GameObject wallTileInstance = Instantiate(wallTile, position, Quaternion.identity, transform);

                        // Rotate wall tiles along the bottom edge.
                        wallTileInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                    // Check if the tile to the top is not a room tile
                    if (z < dungeonLength - 1 && dungeonFloorMap[x, z + 1] != TileType.RoomTile.GetHashCode())
                    {
                        // Randomly select a wall tile from your array.
                        int randomTileIndex = Random.Range(0, wallTiles.Length);
                        GameObject wallTile = wallTiles[randomTileIndex];

                        // Calculate the position for the wall tile.
                        Vector3 position = new Vector3(
                            x * (tileSize + gridSpacing),
                            0,
                            z * (tileSize + gridSpacing)
                        );

                        // if the wallMap indicates a door, create a door
                        if (dungeonWallMap[x, z] == TileType.Door.GetHashCode())
                        {
                            // Randomly select a door tile from your array.
                            randomTileIndex = Random.Range(0, doorTiles.Length);
                            wallTile = doorTiles[randomTileIndex];
                        }

                        // Instantiate the wall tile at the calculated position, as a child of this transform.
                        GameObject wallTileInstance = Instantiate(wallTile, position, Quaternion.identity, transform);

                        // Rotate wall tiles along the top edge and move to the opposite side.
                        wallTileInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
                        wallTileInstance.transform.position = new Vector3(
                            wallTileInstance.transform.position.x,
                            wallTileInstance.transform.position.y,
                            wallTileInstance.transform.position.z + tileSize // Move to the opposite side
                        );
                    }
                }
            }
        }
            // Generate walls for enviro tiles
            for (int x = 0; x < dungeonWidth; x++)
            {
                for (int z = 0; z < dungeonLength; z++)
                {
                    if (dungeonFloorMap[x, z] == TileType.EnviroTile.GetHashCode())
                    {
                        // Check if the tile to the left is not an enviro tile
                        if (x > 0 && dungeonFloorMap[x - 1, z] != TileType.EnviroTile.GetHashCode())
                        {
                            // Randomly select a wall tile from your array.
                            int randomTileIndex = Random.Range(0, wallTiles.Length);
                            GameObject wallTile = wallTiles[randomTileIndex];

                            // Calculate the position for the wall tile.
                            Vector3 position = new Vector3(
                                x * (tileSize + gridSpacing),
                                0,
                                z * (tileSize + gridSpacing)
                            );

                            // Instantiate the wall tile at the calculated position, as a child of this transform.
                            GameObject wallTileInstance = Instantiate(wallTile, position, Quaternion.identity, transform);

                            // Rotate wall tiles along the left edge and move to the opposite side.
                            wallTileInstance.transform.rotation = Quaternion.Euler(0, 90, 0);
                            wallTileInstance.transform.position = new Vector3(
                                wallTileInstance.transform.position.x - tileSize, // Move to the opposite side
                                wallTileInstance.transform.position.y,
                                wallTileInstance.transform.position.z
                            );
                        }
                        // Check if the tile to the right is not an enviro tile
                        if (x < dungeonWidth - 1 && dungeonFloorMap[x + 1, z] != TileType.EnviroTile.GetHashCode())
                        {
                            // Randomly select a wall tile from your array.
                            int randomTileIndex = Random.Range(0, wallTiles.Length);
                            GameObject wallTile = wallTiles[randomTileIndex];

                            // Calculate the position for the wall tile.
                            Vector3 position = new Vector3(
                                x * (tileSize + gridSpacing),
                                0,
                                z * (tileSize + gridSpacing)
                            );

                            // Instantiate the wall tile at the calculated position, as a child of this transform.
                            GameObject wallTileInstance = Instantiate(wallTile, position, Quaternion.identity, transform);

                            // Rotate wall tiles along the right edge.
                            wallTileInstance.transform.rotation = Quaternion.Euler(0, 90, 0);
                        }
                        // Check if the tile to the bottom is not an enviro tile
                        if (z > 0 && dungeonFloorMap[x, z - 1] != TileType.EnviroTile.GetHashCode())
                        {
                            // Randomly select a wall tile from your array.
                            int randomTileIndex = Random.Range(0, wallTiles.Length);
                            GameObject wallTile = wallTiles[randomTileIndex];

                            // Calculate the position for the wall tile.
                            Vector3 position = new Vector3(
                                x * (tileSize + gridSpacing),
                                0,
                                z * (tileSize + gridSpacing)
                            );

                            // Instantiate the wall tile at the calculated position, as a child of this transform.
                            GameObject wallTileInstance = Instantiate(wallTile, position, Quaternion.identity, transform);

                            // Rotate wall tiles along the bottom edge.
                            wallTileInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
                        }
                        // Check if the tile to the top is not an enviro tile
                        if (z < dungeonLength - 1 && dungeonFloorMap[x, z + 1] != TileType.EnviroTile.GetHashCode())
                        {
                            // Randomly select a wall tile from your array.
                            int randomTileIndex = Random.Range(0, wallTiles.Length);
                            GameObject wallTile = wallTiles[randomTileIndex];

                            // Calculate the position for the wall tile.
                            Vector3 position = new Vector3(
                                x * (tileSize + gridSpacing),
                                0,
                                z * (tileSize + gridSpacing)
                            );

                            // Instantiate the wall tile at the calculated position, as a child of this transform.
                            GameObject wallTileInstance = Instantiate(wallTile, position, Quaternion.identity, transform);

                            // Rotate wall tiles along the top edge and move to the opposite side.
                            wallTileInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
                            wallTileInstance.transform.position = new Vector3(
                                wallTileInstance.transform.position.x,
                                wallTileInstance.transform.position.y,
                                wallTileInstance.transform.position.z + tileSize // Move to the opposite side
                            );
                        }
                    }
                }
            }
    }

    void GenerateRooms()
    {
        // Generate the specified number of rooms
        for (int i = 0; i < numberOfRooms; i++)
        {            
            // Define the room's size (random within the specified range)
            int roomWidth = Random.Range(roomParams.minSize, roomParams.maxSize);
            int roomLength = Random.Range(roomParams.minSize, roomParams.maxSize);

            // Randomly position the room within the dungeon, one tile away from the edges
            int roomX = Random.Range(1, dungeonWidth - roomWidth - 1);
            int roomZ = Random.Range(1, dungeonLength - roomLength - 1);

            // Check if the room overlaps with other rooms or dungeon walls
            if (IsRoomPositionValid(roomX, roomZ, roomWidth, roomLength))
            {
                addDoors = true;

                // Add the room to the list
                Room newRoom = new Room(roomX, roomZ, roomWidth, roomLength);
                rooms.Add(newRoom);

                // Generate the room's floor and walls
                GenerateRoom(newRoom, addDoors); 
            } 
            else
            {
                addDoors = false;

                // Add the room to the list
                Room newRoom = new Room(roomX, roomZ, roomWidth, roomLength);
                rooms.Add(newRoom);

                // Generate the room's floor and walls
                GenerateRoom(newRoom, addDoors); 
            } 
        }
    }

    bool IsRoomPositionValid(int x, int z, int width, int length)
    {
        // Check if the room overlaps with existing rooms or dungeon walls
        foreach (Room existingRoom in rooms)
        {
            if (x < existingRoom.x + existingRoom.width + 1 &&
                x + width + 1 > existingRoom.x &&
                z < existingRoom.z + existingRoom.length + 1 &&
                z + length + 1 > existingRoom.z)
            {
                return false; // Overlaps with an existing room
            }
        }
        // Check if the room is within the dungeon bounds
        if (x < 0 || x + width >= dungeonWidth || z < 0 || z + length >= dungeonLength)
        {
            return false; // Room is out of bounds
        }
        return true;
    }

    void GenerateRoom(Room room, bool addDoors)
    {
        // Create a list to store potential door positions
        List<Vector2Int> potentialDoorPositions = new List<Vector2Int>();

        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int z = room.z; z < room.z + room.length; z++)
            {
                // Set values in dungeonFloorMap and dungeonWallMap to indicate floors and walls
                dungeonFloorMap[x, z] = TileType.RoomTile.GetHashCode(); 
                // Check if this tile is not a door
                if (dungeonWallMap[x, z] != TileType.Door.GetHashCode())
                {
                    dungeonWallMap[x, z] = TileType.RoomTile.GetHashCode();
                }

                if (addDoors == true)
                {
                    // Check if this tile is at the edge of the room and isn't in the corner of the room
                    if (x == room.x || x == room.x + room.width - 1 || z == room.z || z == room.z + room.length - 1 && !(x == room.x && z == room.z) && !(x == room.x + room.width - 1 && z == room.z) && !(x == room.x && z == room.z + room.length - 1) && !(x == room.x + room.width - 1 && z == room.z + room.length - 1))
                    {
                        // This tile is at the edge of the room, add it to potential door positions
                        potentialDoorPositions.Add(new Vector2Int(x, z));
                    }
                }
            }
        }
        // Check if there are potential door positions
        if (potentialDoorPositions.Count > 0)
        {
            // Randomly select a position from the potential door positions
            Vector2Int doorPosition = potentialDoorPositions[Random.Range(0, potentialDoorPositions.Count)];

            // Set the door tile at the chosen position
            dungeonWallMap[doorPosition.x, doorPosition.y] = TileType.Door.GetHashCode();
        }
    }
    void SmoothDungeon()
    {
        int smoothIterations = 5;
        // Smooth the dungeon floor map
        for (int i = 0; i < smoothIterations; i++)
        {
            // Create a copy of the dungeon floor map for this iteration
            // This is to prevent the smoothing from affecting the results of previous iterations
            int[,] newDungeonFloorMap = (int[,])dungeonFloorMap.Clone();
            int pillarsPlaced = 0;

            Debug.Log("Loop + " + i);
            // Increase the size of rooms to create corridors one tile wide
            for (int x = 0; x < dungeonWidth; x++)
            {
                for (int z = 0; z < dungeonLength; z++)
                {
                    if (dungeonFloorMap[x, z] == TileType.Empty.GetHashCode())
                    {
                        // Handle empty tiles
                        // Check all conditions and modify newDungeonFloorMap, not dungeonFloorMap
                        // Check if the tile is adjacent to the border of the map
                        if (x == 0 || x == dungeonWidth - 1 || z == 0 || z == dungeonLength - 1)
                        {
                            // Set the tile to a standard tile
                            newDungeonFloorMap[x, z] = TileType.StandardTile.GetHashCode();
                        }
                        // Check if the tile is adjacent to a room tile
                        // This creates borders around each room, which act as hallways.
                        else if (GetAdjacentRoomFloorCount(x, z) > 0)
                        {
                            // Set the tile to a standard tile
                            newDungeonFloorMap[x, z] = TileType.StandardTile.GetHashCode();
                        }
                        // Check if the tile is still empty, and set it to be a standard tile
                        else if (newDungeonFloorMap[x, z] == TileType.Empty.GetHashCode())
                        {
                            // Set the tile to a standard tile
                            newDungeonFloorMap[x, z] = TileType.StandardTile.GetHashCode();
                        }
                    }
                    else if (dungeonFloorMap[x, z] == TileType.StandardTile.GetHashCode())
                    {
                        // Handle standard tiles
                        // Check conditions and modify newDungeonFloorMap if necessary
                        // Check if the tile is not adjacent to any other standard tiles
                        if (GetAdjacentStandardFloorCount(x, z) == 0)
                        {
                            // Set the tile to a room tile
                            newDungeonFloorMap[x, z] = TileType.RoomTile.GetHashCode();
                        }
                        // Check if the tile is only adjacent to one other standard tile
                        else if (GetAdjacentStandardFloorCount(x, z) == 1)
                        {
                            // Set the tile to a room tile
                            newDungeonFloorMap[x, z] = TileType.RoomTile.GetHashCode();
                        }
                        // Check if the tile is surrounded by room tiles
                        else if (GetAdjacentRoomFloorCount(x, z) == 4 && GetDiagonalRoomFloorCount(x, z) == 4)
                        {
                            // Set the tile to a room tile
                            newDungeonFloorMap[x, z] = TileType.RoomTile.GetHashCode();
                        }
                        // Check if the tile is surrounded by standard tiles
                        else if (GetAdjacentStandardFloorCount(x, z) == 4 && GetDiagonalStandardFloorCount(x, z) == 4)
                        {
                            // Set the tile to an enviro tile
                            newDungeonFloorMap[x, z] = TileType.EnviroTile.GetHashCode();

                        }
                    }
                    else if (dungeonFloorMap[x, z] == TileType.RoomTile.GetHashCode())
                    {
                        // Handle room tiles
                        // Check conditions and modify newDungeonFloorMap if necessary
                        // Check if the tile is not adjacent to any other room tiles
                        if (GetAdjacentRoomFloorCount(x, z) == 0)
                        {
                            // Set the tile to a standard tile
                            newDungeonFloorMap[x, z] = TileType.StandardTile.GetHashCode();
                        }
                        // Check if the tile is only adjacent to one other room tile
                        else if (GetAdjacentRoomFloorCount(x, z) == 1)
                        {
                            // Set the tile to a standard tile
                            newDungeonFloorMap[x, z] = TileType.StandardTile.GetHashCode();
                        }
                        // Check if the tile is surrounded by standard tiles
                        else if (GetAdjacentStandardFloorCount(x, z) == 4 && GetDiagonalStandardFloorCount(x, z) == 4)
                        {
                            // Set the tile to a standard tile
                            newDungeonFloorMap[x, z] = TileType.StandardTile.GetHashCode();
                        }                        
                        else if (GetAdjacentRoomFloorCount(x, z) == 4 && GetDiagonalRoomFloorCount(x, z) == 4)
                        {
                            if (pillarsPlaced == 0)
                            {
                                // Set the tile to a pillar tile
                                dungeonPillarsMap[x, z] = TileType.Pillars.GetHashCode();
                                pillarsPlaced += 3;
                            }
                            else
                            {
                                pillarsPlaced--;
                            }
                        }
                    }
                }
            }     
            // Replace the dungeon floor map with the modified map
            dungeonFloorMap = newDungeonFloorMap;           
        } 
    }
    int GetAdjacentEmptyFloorCount(int x, int z)
    {
        int adjacentFloorCount = 0;

        // Check if the tile to the left is an empty tile
        if (x > 0 && dungeonFloorMap[x - 1, z] == TileType.Empty.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the right is an empty tile
        if (x < dungeonWidth - 1 && dungeonFloorMap[x + 1, z] == TileType.Empty.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the bottom is an empty tile
        if (z > 0 && dungeonFloorMap[x, z - 1] == TileType.Empty.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the top is an empty tile
        if (z < dungeonLength - 1 && dungeonFloorMap[x, z + 1] == TileType.Empty.GetHashCode())
        {
            adjacentFloorCount++;
        }
        return adjacentFloorCount;
    }
    int GetAdjacentRoomFloorCount(int x, int z)
    {
        int adjacentFloorCount = 0;

        // Check if the tile to the left is a room tile
        if (x > 0 && dungeonFloorMap[x - 1, z] == TileType.RoomTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the right is a room tile
        if (x < dungeonWidth - 1 && dungeonFloorMap[x + 1, z] == TileType.RoomTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the bottom is a room tile
        if (z > 0 && dungeonFloorMap[x, z - 1] == TileType.RoomTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the top is a room tile
        if (z < dungeonLength - 1 && dungeonFloorMap[x, z + 1] == TileType.RoomTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        return adjacentFloorCount;
    }
    int GetDiagonalEmptyFloorCount(int x, int z)
    {
        int adjacentFloorCount = 0;

        // Check if the tile to the top left is an empty tile
        if (x > 0 && z < dungeonLength - 1 && dungeonFloorMap[x - 1, z + 1] == TileType.Empty.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the top right is an empty tile
        if (x < dungeonWidth - 1 && z < dungeonLength - 1 && dungeonFloorMap[x + 1, z + 1] == TileType.Empty.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the bottom left is an empty tile
        if (x > 0 && z > 0 && dungeonFloorMap[x - 1, z - 1] == TileType.Empty.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the bottom right is an empty tile
        if (x < dungeonWidth - 1 && z > 0 && dungeonFloorMap[x + 1, z - 1] == TileType.Empty.GetHashCode())
        {
            adjacentFloorCount++;
        }
        return adjacentFloorCount;
    }
    int GetDiagonalRoomFloorCount(int x, int z)
    {
        int adjacentFloorCount = 0;

        // Check if the tile to the top left is a room tile
        if (x > 0 && z < dungeonLength - 1 && dungeonFloorMap[x - 1, z + 1] == TileType.RoomTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the top right is a room tile
        if (x < dungeonWidth - 1 && z < dungeonLength - 1 && dungeonFloorMap[x + 1, z + 1] == TileType.RoomTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the bottom left is a room tile
        if (x > 0 && z > 0 && dungeonFloorMap[x - 1, z - 1] == TileType.RoomTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the bottom right is a room tile
        if (x < dungeonWidth - 1 && z > 0 && dungeonFloorMap[x + 1, z - 1] == TileType.RoomTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        return adjacentFloorCount;
    }
    int GetAdjacentStandardFloorCount(int x, int z)
    {
        int adjacentFloorCount = 0;

        // Check if the tile to the left is a standard tile
        if (x > 0 && dungeonFloorMap[x - 1, z] == TileType.StandardTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the right is a standard tile
        if (x < dungeonWidth - 1 && dungeonFloorMap[x + 1, z] == TileType.StandardTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the bottom is a standard tile
        if (z > 0 && dungeonFloorMap[x, z - 1] == TileType.StandardTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the top is a standard tile
        if (z < dungeonLength - 1 && dungeonFloorMap[x, z + 1] == TileType.StandardTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        return adjacentFloorCount;
    }
    int GetDiagonalStandardFloorCount(int x, int z)
    {
        int adjacentFloorCount = 0;

        // Check if the tile to the top left is a standard tile
        if (x > 0 && z < dungeonLength - 1 && dungeonFloorMap[x - 1, z + 1] == TileType.StandardTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the top right is a standard tile
        if (x < dungeonWidth - 1 && z < dungeonLength - 1 && dungeonFloorMap[x + 1, z + 1] == TileType.StandardTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the bottom left is a standard tile
        if (x > 0 && z > 0 && dungeonFloorMap[x - 1, z - 1] == TileType.StandardTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        // Check if the tile to the bottom right is a standard tile
        if (x < dungeonWidth - 1 && z > 0 && dungeonFloorMap[x + 1, z - 1] == TileType.StandardTile.GetHashCode())
        {
            adjacentFloorCount++;
        }
        return adjacentFloorCount;
    }

    void GenerateProps()
    {
        // Generate props based on the dungeonPropsMap
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int z = 0; z < dungeonLength; z++)
            {
                if (dungeonPropsMap[x, z] == TileType.FloorProps.GetHashCode() && dungeonPillarsMap[x, z] != TileType.Pillars.GetHashCode())
                {
                    // Randomly select a prop tile from your array.
                    int randomTileIndex = Random.Range(0, floorProps.Length);
                    GameObject propsTile = floorProps[randomTileIndex];

                    // Calculate the position for the prop tile.
                    Vector3 position = new Vector3(
                        x * (tileSize + gridSpacing),
                        0,
                        z * (tileSize + gridSpacing)
                    );

                    // Instantiate the pillar tile at the calculated position, as a child of this transform.
                    GameObject propsTileInstance = Instantiate(propsTile, position, Quaternion.identity, transform);
                    // Move the prop half a tile to the left and forward
                    propsTileInstance.transform.position = new Vector3(
                        propsTileInstance.transform.position.x - tileSize / 2,
                        propsTileInstance.transform.position.y,
                        propsTileInstance.transform.position.z + tileSize / 2
                    );
                    // Rotate the prop randomly
                    propsTileInstance.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    // Scale the prop up three times
                    propsTileInstance.transform.localScale = new Vector3(3, 3, 3);
                }
            }
        }
    }
    void GeneratePillars()
    {
        // Generate pillars based on the dungeonPillarsMap
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int z = 0; z < dungeonLength; z++)
            {
                if (dungeonPillarsMap[x, z] == TileType.Pillars.GetHashCode())
                {
                    // Randomly select a pillar tile from your array.
                    int randomTileIndex = Random.Range(0, pillarTiles.Length);
                    GameObject pillarTile = pillarTiles[randomTileIndex];

                    // Calculate the position for the pillar tile.
                    Vector3 position = new Vector3(
                        x * (tileSize + gridSpacing),
                        0,
                        z * (tileSize + gridSpacing)
                    );

                    // Instantiate the pillar tile at the calculated position, as a child of this transform.
                    GameObject pillarTileInstance = Instantiate(pillarTile, position, Quaternion.identity, transform);
                    // Move the pillar half a tile to the left and forward
                    pillarTileInstance.transform.position = new Vector3(
                        pillarTileInstance.transform.position.x - tileSize / 2,
                        pillarTileInstance.transform.position.y,
                        pillarTileInstance.transform.position.z + tileSize / 2
                    );
                    // Rotate the pillar randomly
                    pillarTileInstance.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                }
            }
        }
    }
}
