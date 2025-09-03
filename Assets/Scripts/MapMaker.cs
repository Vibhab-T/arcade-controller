using System.Collections.Generic;
using UnityEngine;

public class MapMaker : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 20;
    public int height = 20;
    public float roadChance = 0.3f;

    [Header("Prefabs")]
    public GameObject grassPrefab;
    public GameObject roadPrefab;
    public GameObject carPrefab;

    [Header("Cars")]
    public int carCount = 10;

    private int[,] grid;
    private Dictionary<Vector2Int, GameObject> tileNodes = new Dictionary<Vector2Int, GameObject>();

    private void Start()
    {
        // Init grid
        grid = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = 0;

        // Random horizontal roads
        for (int y = 0; y < height; y++)
        {
            if (Random.value < roadChance)
            {
                for (int x = 0; x < width; x++)
                    grid[x, y] = 1;
            }
        }

        // Random vertical roads
        for (int x = 0; x < width; x++)
        {
            if (Random.value < roadChance)
            {
                for (int y = 0; y < height; y++)
                    grid[x, y] = 1;
            }
        }

        PaintGrid();

        // Spawn cars after grid is ready
        for (int i = 0; i < carCount; i++)
            SpawnCarWithMinSteps(15);
    }

    void PaintGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * 2 + 1, 0, y * 2 + 1);
                GameObject instance = grid[x, y] == 1 ? Instantiate(roadPrefab, pos, Quaternion.identity)
                                                      : Instantiate(grassPrefab, pos, Quaternion.identity);
                instance.transform.parent = this.transform;
                tileNodes[new Vector2Int(x, y)] = instance;
            }
        }
    }

    void SpawnCarWithMinSteps(int minSteps = 15)
    {
        List<Vector2Int> roads = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (grid[x, y] == 1) roads.Add(new Vector2Int(x, y));

        if (roads.Count < 2)
        {
            Debug.LogError("Not enough roads to spawn a car!");
            return;
        }

        Vector2Int start = roads[Random.Range(0, roads.Count)];

        List<Vector2Int> candidates = new List<Vector2Int>();
        foreach (var road in roads)
            if (Vector2Int.Distance(start, road) >= minSteps)
                candidates.Add(road);

        if (candidates.Count == 0)
        {
            Debug.LogError("No valid end positions satisfy minimum distance!");
            return;
        }

        Vector2Int end = candidates[Random.Range(0, candidates.Count)];

        List<Vector2Int> path = FindPath(start, end);
        if (path.Count < minSteps)
        {
            Debug.LogError("Generated path is shorter than minimum steps. Consider increasing road connectivity.");
            return;
        }

        Vector3 spawnPos = new Vector3(start.x * 2 + 1, 0.5f, start.y * 2 + 1);
        GameObject car = Instantiate(carPrefab, spawnPos, Quaternion.identity);
        car.transform.parent = this.transform;
        car.SendMessage("SetPath", path, SendMessageOptions.DontRequireReceiver);

        Debug.Log($"Car spawned from {start} to {end} | Path length: {path.Count}");
    }

    // -----------------------------
    // A* Pathfinding
    // -----------------------------
    List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> openSet = new List<Vector2Int> { start };
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int> { [start] = 0 };
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float> { [start] = Vector2Int.Distance(start, goal) };

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet[0];
            foreach (var node in openSet)
                if (fScore.ContainsKey(node) && fScore[node] < fScore[current])
                    current = node;

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);

            foreach (var neighbor in GetNeighbours(current))
            {
                int tentativeG = gScore.ContainsKey(current) ? gScore[current] + 1 : int.MaxValue;
                int neighborG = gScore.ContainsKey(neighbor) ? gScore[neighbor] : int.MaxValue;

                if (tentativeG < neighborG)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Vector2Int.Distance(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new List<Vector2Int>();
    }

    List<Vector2Int> GetNeighbours(Vector2Int cell)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
        foreach (var dir in directions)
        {
            int nx = cell.x + dir.x;
            int ny = cell.y + dir.y;
            if (nx >= 0 && nx < width && ny >= 0 && ny < height && grid[nx, ny] == 1)
                neighbours.Add(new Vector2Int(nx, ny));
        }
        return neighbours;
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> totalPath = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }
}
