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
        public int minSize = 3;
        public int maxSize = 6;
        public int minDoors = 1;
        public int maxDoors = 4;
        public int minDistanceBetweenRooms = 1;
    }

    private RoomParameters roomParams = new RoomParameters();
    private List<Room> rooms = new List<Room>();

    private class Room
    {
        public Vector2Int position;
        public Vector2Int size;

        public Room(Vector2Int position, Vector2Int size)
        {
            this.position = position;
            this.size = size;
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
            // Destroy all children of this transform
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            GenerateDungeon();
        }
    }

    void GenerateDungeon()
    {
        // Initialize the dungeon maps
        InitializeMaps();
        // Generate the outer walls
        GenerateOuterWalls();
        // Generate a single room
        GenerateSingleRoom();
        // Fills in all non-room areas with floors
        GenerateFloor();
        for (int z = 0; z < dungeonLength; z++)
        {
            for (int x = 0; x < dungeonWidth; x++)
            {
                Debug.Log("dungeonFloorMap[" + x + ", " + z + "] = " + dungeonFloorMap[x, z]);
            }
        }
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
            }
        }
    }
    void GenerateOuterWalls()
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
        // The 1 and -1 ensure that the room is at least 1 tile away from the edge of the dungeon
        int roomX = Random.Range(1, dungeonWidth - roomWidth - 1);
        int roomZ = Random.Range(1, dungeonLength - roomLength - 1);

        // Check if the room overlaps with other rooms or dungeon walls
        if (IsRoomPositionValid(roomX, roomZ, roomWidth, roomLength))
        {
            // Add the room to the list
            Room newRoom = new Room(new Vector2Int(roomX, roomZ), new Vector2Int(roomWidth, roomLength));
            rooms.Add(newRoom);

            // Generate the room's floor and walls
            GenerateRoom(newRoom);
        }
        Debug.Log("Room Position: (" + roomX + ", " + roomZ + ")");
        Debug.Log("Room Size: (" + roomWidth + ", " + roomLength + ")");
    }

    bool IsRoomPositionValid(int x, int z, int width, int length)
    {
        // Check if the room overlaps with existing rooms or dungeon walls
        foreach (Room existingRoom in rooms)
        {
            if (x < existingRoom.position.x + existingRoom.size.x + roomParams.minDistanceBetweenRooms &&
                x + width + roomParams.minDistanceBetweenRooms > existingRoom.position.x &&
                z < existingRoom.position.y + existingRoom.size.y + roomParams.minDistanceBetweenRooms &&
                z + length + roomParams.minDistanceBetweenRooms > existingRoom.position.y)
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
        for (int x = room.position.x; x < room.position.x + room.size.x; x++)
        {
            for (int z = room.position.y; z < room.position.y + room.size.y; z++)
            {
                // Generate the room's floor
                // Randomly select a floor tile from your array
                int randomTileIndex = Random.Range(0, roomFloorTiles.Length);
                GameObject tilePrefab = roomFloorTiles[randomTileIndex];

                Vector3 position = new Vector3(
                    x * (tileSize + gridSpacing),
                    0,
                    z * (tileSize + gridSpacing)
                );

                dungeonFloorMap[x, z] = 1;
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tile.name = "FloorTile_" + x + "_" + z;

                // Generate walls around the room 
                if (x == room.position.x || x == room.position.x + room.size.x - 1 ||
                z == room.position.y || z == room.position.y + room.size.y - 1)
            {
                int randomWallTileIndex = Random.Range(0, wallTiles.Length);
                GameObject wallTilePrefab = wallTiles[randomWallTileIndex];

                // Calculate the position for the wall tile.
                Vector3 wallPosition = new Vector3(
                    x * (tileSize + gridSpacing),
                    0,
                    z * (tileSize + gridSpacing)
                );

                // Update the dungeon wall map to indicate a wall tile.
                dungeonWallMap[x, z] = 1;

                // Instantiate the wall tile at the calculated position, as a child of this transform.
                GameObject wallTileInstance = Instantiate(wallTilePrefab, wallPosition, Quaternion.identity, transform);

                // Determine the rotation based on position
                // You may need to adjust the rotation depending on your wall tile orientation.
                // This is an example, and you should adapt it to your tile's orientation.
                wallTileInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            }
        }
    }
}
