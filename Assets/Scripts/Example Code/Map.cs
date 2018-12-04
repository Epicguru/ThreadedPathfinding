using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    /*
     * Holds information about a 2D map. Also spawns sprites to visually represent the map.
     */

    // I call the individual squares in the map 'tiles'
    public GameObject TilePrefab;

    // Width and height of the map.
    public int Width = 50;
    public int Height = 50;

    // The data structure to reprensent the tiles. True means wall, false means air.
    public bool[,] Tiles;

    public void Awake()
    {
        // Get a reference to the pathfinding manager...
        PathfindingManager manager = PathfindingManager.Instance;

        // Give the manager our custom tile provider. That's it!
        manager.Provider = new CustomTileProvider(this);

        // Generate the 2D map, and then spawn the tiles to represent it.
        Tiles = new bool[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                bool solid = Random.Range(0f, 100f) <= 30f; // 30% chance to be solid.
                Tiles[x, y] = solid;
            }
        }

        // Spawn tiles.
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if(Tiles[x, y])
                {
                    Vector3 position = new Vector3(x, y, 0f);
                    Instantiate(TilePrefab, position, Quaternion.identity, this.transform);
                }
            }
        }
    }
}
