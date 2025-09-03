using System.Collections.Generic;
using UnityEngine;

public class Traffic : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;
    public float rotationSpeed = 5f;

    private List<Vector3> worldPath = new List<Vector3>();
    private int currentIndex = 0;
    private bool hasPath = false;

    // Called from MapMaker
    public void SetPath(List<Vector2Int> path)
    {
        worldPath.Clear();

        foreach (var point in path)
            worldPath.Add(new Vector3(point.x * 2 + 1, 0.5f, point.y * 2 + 1));

        if (worldPath.Count > 0)
        {
            currentIndex = 0;
            hasPath = true;
        }
    }

    private void Update()
    {
        if (!hasPath || worldPath.Count == 0) return;

        Vector3 target = worldPath[currentIndex];
        target.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        Vector3 direction = (target - transform.position).normalized;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            currentIndex++;
            if (currentIndex >= worldPath.Count)
                hasPath = false;
        }
    }
}
