using System.Collections;
using System.Collections.Generic;
using ThreadedPathfinding;
using UnityEngine;

public class Map : MonoBehaviour
{
    /*
     * Holds information about a 2D map. Also spawns sprites to visually represent the map.
     */

    // I call the individual squares in the map 'tiles'
    public GameObject TilePrefab;
    public GameObject CharacterPrefab;
    public SpriteRenderer Background;
    public SpriteRenderer Background2;

    [Header("Character Spawning")]
    [Range(0, 5000)]
    public int CharacterCount = 10;

    [Header("Map Settings")]

    // Width and height of the map.
    public int Width = 50;
    public int Height = 50;

    // The data structure to reprensent the tiles. True means wall, false means air.
    public bool[,] Tiles;

    private List<GameObject> walls = new List<GameObject>();

    public void Awake()
    {
        // Get a reference to the pathfinding manager...
        PathfindingManager manager = PathfindingManager.Instance;

        // Give the manager our custom tile provider. That's it!
        manager.Provider = new CustomTileProvider(this);

        // Camera work, to center the camera on the world. Demo purposes only.
        Camera.main.transform.position = new Vector3(Width / 2, Height / 2, -10f);
        Camera.main.orthographicSize = Height / 2f + 1;

        // Position the background sprite.
        Vector3 center = new Vector3(Width / 2 - 0.5f, Height / 2 - 0.5f);
        Background.transform.position = center;
        Background2.transform.position = center;
        Background.size = new Vector2(Width + 1, Height + 1);
        Background2.size = new Vector2(Width + 0, Height + 0);

        // Initialize tile array.
        Tiles = new bool[Width, Height];  
    }

    private void OnGUI()
    {
        GUILayout.Label("Click in the gray area to place walls. Avoid creating closed regions.");
        bool clear = GUILayout.Button("Clear map");
        if (clear)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Tiles[x, y] = false;
                }
            }
            foreach (var item in walls)
            {
                Destroy(item);
            }
            walls.Clear();
        }
        
        bool spawn = GUILayout.Button("Spawn " + CharacterCount + " characters");
        if (spawn)
            SpawnCharacters();
    }

    private bool GetSolidRelative(PNode current, int rx, int ry)
    {
        // Return true to indicate that the tile is air.

        int nx = rx + current.X;
        int ny = ry + current.Y;

        if(nx >= 0 && ny >= 0 && nx < Width && ny < Height)
        {
            return !Tiles[nx, ny];
        }
        else
        {
            return false;
        }
    }

    private void Update()
    {
        bool mouseClicked = Input.GetMouseButton(0);

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);

        int x = (int)mousePos.x;
        int y = (int)mousePos.y;

        if(x >= 0 && y >= 0 && x < Width && y < Height)
        {
            if (mouseClicked)
            {
                if(!Tiles[x, y])
                {
                    Tiles[x, y] = true;
                    SpawnTile(x, y);
                }
            }
        }
    }

    private void SpawnTile(int x, int y)
    {
        Vector3 position = new Vector3(x, y, 0f);
        walls.Add(Instantiate(TilePrefab, position, Quaternion.identity, this.transform));
    }

    private void SpawnAllTiles()
    {
        // Places a tile at all of the solid positions.
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (Tiles[x, y])
                {
                    Vector3 position = new Vector3(x, y, 0f);
                    Instantiate(TilePrefab, position, Quaternion.identity, this.transform);
                }
            }
        }
    }

    private void SpawnCharacters()
    {
        for (int i = 0; i < CharacterCount; i++)
        {
            Character spawned = Instantiate(CharacterPrefab).GetComponent<Character>();

            // Find a position that is not a wall...
            // This sometimes makes characters be stuck in very small rooms that they cannot pathfind out of, but this is a demo so it doesn't really matter.
            // TODO fix what I just described.

            int x = 0, y = 0;
            while (true)
            {
                x = Random.Range(0, Width);
                y = Random.Range(0, Height);

                if (Tiles[x, y] == true) // If it is a wall...
                    continue; // Try again.
                else
                    break; // Done, found position.
            }

            spawned.X = x;
            spawned.Y = y;
            spawned.transform.position = new Vector3(x, y, 0f);
            spawned.Map = this;
        }
    }
}
