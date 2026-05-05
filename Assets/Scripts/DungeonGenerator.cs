using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 30;
    [SerializeField] private int gridHeight = 30;

    [Header("Random Walk Settings")]
    [SerializeField] private int walkSteps = 200;

    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;

    // The grid: true = floor, false = empty (will become wall)
    private bool[,] grid;

    // Parent transform to keep the Hierarchy tidy
    private Transform dungeonParent;

    private void Start()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        // Create a parent object so spawned tiles are nested tidily
        GameObject parentGO = new GameObject("Dungeon");
        dungeonParent = parentGO.transform;

        // Step 1: Create the empty grid
        grid = new bool[gridWidth, gridHeight];

        // Step 2: Start the walker at the centre
        Vector2Int walker = new Vector2Int(gridWidth / 2, gridHeight / 2);
        grid[walker.x, walker.y] = true;

        // Step 3: Take walkSteps random steps
        for (int i = 0; i < walkSteps; i++)
        {
            int direction = Random.Range(0, 4);
            Vector2Int step = Vector2Int.zero;

            switch (direction)
            {
                case 0: step = new Vector2Int(0, 1); break;
                case 1: step = new Vector2Int(0, -1); break;
                case 2: step = new Vector2Int(1, 0); break;
                case 3: step = new Vector2Int(-1, 0); break;
            }

            Vector2Int newPos = walker + step;

            if (newPos.x < 1 || newPos.x >= gridWidth - 1 ||
                newPos.y < 1 || newPos.y >= gridHeight - 1)
            {
                continue;
            }

            walker = newPos;
            grid[walker.x, walker.y] = true;
        }

        // Step 4: Spawn a floor tile for every cell marked true
        SpawnFloors();
        SpawnWalls();
    }

    private void SpawnFloors()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y])
                {
                    // Convert grid coordinates to world position.
                    // Grid Y becomes world Z (because we're top-down).
                    Vector3 worldPos = new Vector3(x, 0, y);
                    Instantiate(floorPrefab, worldPos, Quaternion.identity, dungeonParent);
                }
            }
        }
    }
    private void SpawnWalls()
{
    // Loop through every cell in the grid
    for (int x = 0; x < gridWidth; x++)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            // Skip floor cells - we only place walls in empty cells
            if (grid[x, y]) continue;

            // Check if this empty cell has any floor neighbour
            if (HasFloorNeighbour(x, y))
            {
                Vector3 worldPos = new Vector3(x, 0.5f, y);
                Instantiate(wallPrefab, worldPos, Quaternion.identity, dungeonParent);
            }
        }
    }
}

private bool HasFloorNeighbour(int x, int y)
    {
        // Check the 4 cardinal neighbours: N, S, E, W
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            // Make sure the neighbour is inside the grid
            if (nx < 0 || nx >= gridWidth || ny < 0 || ny >= gridHeight) continue;

            // If the neighbour is a floor, this empty cell should be a wall
            if (grid[nx, ny]) return true;
        }

        return false;
    }
}