using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject[] floorTiles; // Array of floor tile prefabs
    public GameObject[] wallTiles; // Array of wall tile prefabs
    public int width = 10; // Width of the floor
    public int length = 10; // Length of the floor
    public float tileSize = 1.0f; // Size of each tile
    public float gridSpacing = 1.0f; // Spacing between tiles

    private int[,] dungeonMap;

    void Awake()
    {
        dungeonMap = new int[width, length];
    }

    void Start()
    {
        GenerateDungeon();
        GenerateOuterWalls();
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
        GenerateFloor();
    }

    void GenerateFloor()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
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
                dungeonMap[x, z] = 1;

                // Instantiate the floor tile at the calculated position, as a child of this transform
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                // Rename the tile based on its position in the grid
                tile.name = "FloorTile_" + x + "_" + z;
            }
        }
    }
    void GenerateOuterWalls()
    {
        // Generate wall tiles on the outer edge of the map.
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                if (x == 0 || x == width - 1)
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
                    else if (x == width - 1)
                    {
                        // Rotate wall tiles along the right edge.
                        wallTileInstance.transform.rotation = Quaternion.Euler(0, 90, 0);
                    }
                }
            
                if (z == 0 || z == length - 1)
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

                    // Instantiate the wall tile at the calculated position, as a child of this transform.
                    GameObject wallTileInstance = Instantiate(wallTile, position, Quaternion.identity, transform);

                    // Determine the rotation based on position
                    if (z == 0)
                    {
                        // Rotate wall tiles along the bottom edge.
                        wallTileInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                    else if (z == length - 1)
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
}
