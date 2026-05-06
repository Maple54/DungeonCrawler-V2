using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Level Configuration")]
    [SerializeField] private LevelData levelData;

    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;

    [Header("Entities")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject enemyPrefab;

    private bool[,] grid;
    private Transform dungeonParent;

    private Vector2Int spawnCell;
    private Vector2Int bossCell;

    private void Start()
    {
        if (levelData == null)
        {
            Debug.LogError("DungeonGenerator: No LevelData assigned!");
            return;
        }

        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        GameObject parentGO = new GameObject($"Dungeon ({levelData.levelName})");
        dungeonParent = parentGO.transform;

        grid = new bool[levelData.gridWidth, levelData.gridHeight];

        Vector2Int walker = new Vector2Int(levelData.gridWidth / 2, levelData.gridHeight / 2);
        grid[walker.x, walker.y] = true;

        for (int i = 0; i < levelData.walkSteps; i++)
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

            if (newPos.x < 1 || newPos.x >= levelData.gridWidth - 1 ||
                newPos.y < 1 || newPos.y >= levelData.gridHeight - 1)
            {
                continue;
            }

            walker = newPos;
            grid[walker.x, walker.y] = true;

            if (levelData.roomCount > 0 && i % (levelData.walkSteps / levelData.roomCount) == 0)
            {
                StampRoom(walker);
            }
        }

        if (levelData.widenCorridors)
        {
            WidenCorridors();
        }

        int floorCount = 0;
        for (int x = 0; x < levelData.gridWidth; x++)
            for (int y = 0; y < levelData.gridHeight; y++)
                if (grid[x, y]) floorCount++;

        Debug.Log($"[{levelData.levelName}] Floor cells: {floorCount} / {levelData.gridWidth * levelData.gridHeight}");

        SpawnFloors();
        SpawnWalls();
        FindSpawnAndBoss();
        SpawnEntities();
    }

    private void StampRoom(Vector2Int centre)
    {
        int width = Random.Range(levelData.minRoomSize, levelData.maxRoomSize + 1);
        int height = Random.Range(levelData.minRoomSize, levelData.maxRoomSize + 1);

        int startX = centre.x - width / 2;
        int startY = centre.y - height / 2;

        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (x < 1 || x >= levelData.gridWidth - 1 || y < 1 || y >= levelData.gridHeight - 1) continue;
                grid[x, y] = true;
            }
        }
    }

    private void WidenCorridors()
    {
        bool[,] snapshot = new bool[levelData.gridWidth, levelData.gridHeight];
        for (int x = 0; x < levelData.gridWidth; x++)
            for (int y = 0; y < levelData.gridHeight; y++)
                snapshot[x, y] = grid[x, y];

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0)
        };

        for (int x = 1; x < levelData.gridWidth - 1; x++)
        {
            for (int y = 1; y < levelData.gridHeight - 1; y++)
            {
                if (!snapshot[x, y]) continue;
                foreach (Vector2Int dir in directions)
                {
                    grid[x + dir.x, y + dir.y] = true;
                }
            }
        }
    }

    private void SpawnFloors()
    {
        for (int x = 0; x < levelData.gridWidth; x++)
        {
            for (int y = 0; y < levelData.gridHeight; y++)
            {
                if (grid[x, y])
                {
                    Vector3 worldPos = new Vector3(x, 0, y);
                    Instantiate(floorPrefab, worldPos, Quaternion.identity, dungeonParent);
                }
            }
        }
    }

    private void SpawnWalls()
    {
        for (int x = 0; x < levelData.gridWidth; x++)
        {
            for (int y = 0; y < levelData.gridHeight; y++)
            {
                if (grid[x, y]) continue;

                if (HasFloorNeighbour(x, y))
                {
                    Vector3 worldPos = new Vector3(x, levelData.wallHeight / 2f, y);
                    GameObject wall = Instantiate(wallPrefab, worldPos, Quaternion.identity, dungeonParent);
                    wall.transform.localScale = new Vector3(1, levelData.wallHeight, 1);
                }
            }
        }
    }

    private bool HasFloorNeighbour(int x, int y)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0)
        };

        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;
            if (nx < 0 || nx >= levelData.gridWidth || ny < 0 || ny >= levelData.gridHeight) continue;
            if (grid[nx, ny]) return true;
        }
        return false;
    }

    private int[,] BFS(Vector2Int start)
    {
        int[,] distances = new int[levelData.gridWidth, levelData.gridHeight];
        for (int x = 0; x < levelData.gridWidth; x++)
            for (int y = 0; y < levelData.gridHeight; y++)
                distances[x, y] = -1;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        distances[start.x, start.y] = 0;
        queue.Enqueue(start);

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDist = distances[current.x, current.y];

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbour = current + dir;
                if (neighbour.x < 0 || neighbour.x >= levelData.gridWidth ||
                    neighbour.y < 0 || neighbour.y >= levelData.gridHeight) continue;
                if (!grid[neighbour.x, neighbour.y]) continue;
                if (distances[neighbour.x, neighbour.y] != -1) continue;

                distances[neighbour.x, neighbour.y] = currentDist + 1;
                queue.Enqueue(neighbour);
            }
        }
        return distances;
    }

    private void FindSpawnAndBoss()
    {
        Vector2Int spawn = new Vector2Int(levelData.gridWidth / 2, levelData.gridHeight / 2);
        int[,] distances = BFS(spawn);

        Vector2Int boss = spawn;
        int maxDist = 0;
        for (int x = 0; x < levelData.gridWidth; x++)
        {
            for (int y = 0; y < levelData.gridHeight; y++)
            {
                if (distances[x, y] > maxDist)
                {
                    maxDist = distances[x, y];
                    boss = new Vector2Int(x, y);
                }
            }
        }

        Debug.Log($"Spawn at {spawn}, Boss at {boss}, distance: {maxDist}");
        spawnCell = spawn;
        bossCell = boss;
    }

    private void SpawnEntities()
    {
        Vector3 spawnWorldPos = new Vector3(spawnCell.x, 1f, spawnCell.y);
        playerObject.transform.position = spawnWorldPos;

        Vector3 bossWorldPos = new Vector3(bossCell.x, 1f, bossCell.y);
        Instantiate(bossPrefab, bossWorldPos, Quaternion.identity, dungeonParent);

        int[,] distances = BFS(spawnCell);

        List<Vector2Int> validCells = new List<Vector2Int>();
        for (int x = 0; x < levelData.gridWidth; x++)
        {
            for (int y = 0; y < levelData.gridHeight; y++)
            {
                if (!grid[x, y]) continue;
                if (distances[x, y] < levelData.safeRadius) continue;
                if (x == bossCell.x && y == bossCell.y) continue;
                validCells.Add(new Vector2Int(x, y));
            }
        }

        int spawned = 0;
        while (spawned < levelData.enemyCount && validCells.Count > 0)
        {
            int index = Random.Range(0, validCells.Count);
            Vector2Int cell = validCells[index];
            validCells.RemoveAt(index);

            Vector3 enemyWorldPos = new Vector3(cell.x, 1f, cell.y);
            Instantiate(enemyPrefab, enemyWorldPos, Quaternion.identity, dungeonParent);
            spawned++;
        }

        Debug.Log($"Spawned player at {spawnCell}, boss at {bossCell}, {spawned} enemies.");
    }
}