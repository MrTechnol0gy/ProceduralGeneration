using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject[] floorTiles; // Array of floor tile prefabs
    public GameObject[] roomFloorTiles; // Array of room floor tile prefabs
    public GameObject[] wallTiles; // Array of wall tile prefabs
    public GameObject[] doorTiles; // Array of door tile prefabs
    public int dungeonWidth = 10; // Width of the floor
    public int dungeonLength = 10; // Length of the floor
    public float tileSize = 1.0f; // Size of each tile
    public float gridSpacing = 1.0f; // Spacing between tiles

    private int[,] dungeonFloorMap;
    private int[,] dungeonWallMap;

    private class RoomParameters
    {
        public int minSize = 2;
        public int maxSize = 6;
        public int minDoors = 1;
        public int maxDoors = 4;
        public int minDistanceBetweenRooms = 1;
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
        dungeonFloorMap = new int[dungeonWidth, dungeonLength];
        dungeonWallMap = new int[dungeonWidth, dungeonLength];
    }

    void Start()
    {
        GenerateDungeon();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearDungeon();
            GenerateDungeon();
        }
    }

    void GenerateDungeon()
    {
        // Initialize the dungeon maps
        InitializeMaps();
        // Generate the outer walls
        GenerateWalls();
        // Generate a single room
        GenerateSingleRoom();
        // Fills in all non-room areas with floors
        GenerateFloor();
        // for (int z = 0; z < dungeonLength; z++)
        // {
        //     for (int x = 0; x < dungeonWidth; x++)
        //     {
        //         Debug.Log("dungeonFloorMap[" + x + ", " + z + "] = " + dungeonFloorMap[x, z]);
        //     }
        // }
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
    }
    void GenerateFloor()
    {
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int z = 0; z < dungeonLength; z++)
            {
                if (dungeonFloorMap[x, z] == 0)
                {
                    // Randomly select a floor tile from your array
                    int randomTileIndex = Random.Range(0, floorTiles.Length);
                    GameObject tilePrefab = floorTiles[randomTileIndex];

                    // Calculate the position based on the grid spacing and tile size
                    Vector3 position = new Vector3(
                        x * (tileSize + gridSpacing),
                        0,
                        z * (tileSize + gridSpacing)
                    );

                    // Update the dungeon layout map to indicate a floor tile.
                    dungeonFloorMap[x, z] = 1;

                    // Instantiate the floor tile at the calculated position, as a child of this transform
                    GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                    // Rename the tile based on its position in the grid
                    tile.name = "FloorTile_" + x + "_" + z;
                }
                else if (dungeonFloorMap[x, z] == 2)
                {
                    // Randomly select a floor tile from your array
                    int randomTileIndex = Random.Range(0, roomFloorTiles.Length);
                    GameObject tilePrefab = roomFloorTiles[randomTileIndex];

                    // Calculate the position based on the grid spacing and tile size
                    Vector3 position = new Vector3(
                        x * (tileSize + gridSpacing),
                        0,
                        z * (tileSize + gridSpacing) + tileSize // To correct an offset in the room floor tiles
                    );

                    // Update the dungeon layout map to indicate a floor tile.
                    dungeonFloorMap[x, z] = 1;

                    // Instantiate the floor tile at the calculated position, as a child of this transform
                    GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                    // Rename the tile based on its position in the grid
                    tile.name = "RoomFloorTile_" + x + "_" + z;
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
                    dungeonWallMap[x, z] = 1;

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
                    dungeonWallMap[x, z] = 1;

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
    }

    void GenerateSingleRoom()
    {
        // Define the room's size (random within the specified range)
        int roomWidth = Random.Range(roomParams.minSize, roomParams.maxSize);
        int roomLength = Random.Range(roomParams.minSize, roomParams.maxSize);

        // Randomly position the room within the dungeon
        int roomX = Random.Range(0, dungeonWidth - roomWidth);
        int roomZ = Random.Range(0, dungeonLength - roomLength);

        // Check if the room overlaps with other rooms or dungeon walls
        if (IsRoomPositionValid(roomX, roomZ, roomWidth, roomLength))
        {
             // Add the room to the list
            Room newRoom = new Room(roomX, roomZ, roomWidth, roomLength);
            rooms.Add(newRoom);

            // Generate the room's floor and walls
            GenerateRoom(newRoom);
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

    void GenerateRoom(Room room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int z = room.z; z < room.z + room.length; z++)
            {
                // Set values in dungeonFloorMap and dungeonWallMap to indicate floors and walls
                dungeonFloorMap[x, z] = 2;  // You might want to use different values to represent floors and walls
                dungeonWallMap[x, z] = 2;  // Adjust this value based on your logic
            }
        }
    }
}
