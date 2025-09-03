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

    [Header("Cars")]
    public GameObject playerCar;   // Assign the existing car in the scene
    public GameObject aiCarPrefab;
    public int aiCarCount = 10;

    private int[,] grid;
    private Dictionary<Vector2Int, GameObject> tileNodes = new Dictionary<Vector2Int, GameObject>();

    private void Start()
    {
        InitializeGrid();
        PaintGrid();

        // Move the existing player car to a valid road tile
        PlacePlayerCar();

        // Spawn AI cars
        for (int i = 0; i < aiCarCount; i++)
            SpawnCarWithMinSteps(aiCarPrefab, 15);
    }

    void InitializeGrid()
    {
        grid = new int[width, height];

        // Horizontal roads
        for (int y = 0; y < height; y++)
            if (Random.value < roadChance)
                for (int x = 0; x < width; x++)
                    grid[x, y] = 1;

        // Vertical roads
        for (int x = 0; x < width; x++)
            if (Random.value < roadChance)
                for (int y = 0; y < height; y++)
                    grid[x, y] = 1;
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

    void PlacePlayerCar()
    {
        if (playerCar == null) return;

        // Find a random road tile
        List<Vector2Int> roads = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (grid[x, y] == 1) roads.Add(new Vector2Int(x, y));

        if (roads.Count == 0) return;

        Vector2Int chosenTile = roads[Random.Range(0, roads.Count)];
        Vector3 newPos = new Vector3(chosenTile.x * 2 + 1, 0.5f, chosenTile.y * 2 + 1);

        // Move the car
        playerCar.transform.position = newPos;

        // Reset rotation
        playerCar.transform.rotation = Quaternion.identity;

        // Optionally, if playerCar has a Traffic script, assign a path
        var traffic = playerCar.GetComponent<Traffic>();
        if (traffic != null)
        {
            // Create a simple path for demo purposes (can be expanded)
            List<Vector2Int> path = new List<Vector2Int> { chosenTile };
            traffic.SetPath(path);
        }
    }

    void SpawnCarWithMinSteps(GameObject carPrefab, int minSteps = 15)
    {
        List<Vector2Int> roads = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (grid[x, y] == 1) roads.Add(new Vector2Int(x, y));

        if (roads.Count < 2) return;

        Vector2Int start = roads[Random.Range(0, roads.Count)];
        List<Vector2Int> candidates = roads.FindAll(r => Vector2Int.Distance(start, r) >= minSteps);
        if (candidates.Count == 0) return;

        Vector2Int end = candidates[Random.Range(0, candidates.Count)];
        List<Vector2Int> path = FindPath(start, end);
        if (path.Count < minSteps) return;

        Vector3 spawnPos = new Vector3(start.x * 2 + 1, 0.5f, start.y * 2 + 1);
        GameObject car = Instantiate(carPrefab, spawnPos, Quaternion.identity);
        car.transform.parent = this.transform;

        var traffic = car.GetComponent<Traffic>();
        if (traffic != null)
            traffic.SetPath(path);
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
                int tentativeG = gScore[current] + 1;
                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
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
