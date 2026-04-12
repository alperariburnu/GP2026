using UnityEngine;

/// <summary>
/// Spawns points within a Rect area using a SimplePool for efficiency.
/// </summary>
public class PointSpawner : MonoBehaviour
{
    [Header("Prefab & Pool")]
    [SerializeField]
    [Tooltip("Point prefab to spawn")]
    private Point pointPrefab;

    [SerializeField]
    [Tooltip("Number of pooled items to pre-warm")]
    private int poolPreWarm = 20;

    [Header("Spawn Area")]
    [SerializeField]
    [Tooltip("Area (world-space) where points may spawn")]
    private Rect spawnArea = new Rect(-5f, -3f, 10f, 6f);

    [Header("Spawn Settings")]
    [SerializeField]
    [Tooltip("Maximum concurrently active points")]
    private int maxPoints = 15;

    [SerializeField]
    [Tooltip("Time between spawn attempts in seconds")]
    private float spawnInterval = 0.5f;

    private SimplePool<Point> pool;
    private float timer;
    private int activeCount;

    private void Awake()
    {
        if (pointPrefab == null)
        {
            Debug.LogError("PointSpawner: pointPrefab is not assigned.");
            enabled = false;
            return;
        }

        pool = new SimplePool<Point>(pointPrefab, poolPreWarm);
    }

    private void Start()
    {
        // Subscribe to game end if GameManager is available so we can stop spawning
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameEnd += HandleGameEnd;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameEnd -= HandleGameEnd;
        }
    }

    private void Update()
    {
        // Respect GameManager active state if available
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
            return;

        if (activeCount >= maxPoints)
            return;

        timer += Time.deltaTime;
        if (timer < spawnInterval)
            return;

        timer = 0f;
        SpawnOne();
    }

    private void SpawnOne()
    {
        Point p = pool.Get();
        if (p == null)
        {
            // Fallback to Instantiate if pool is empty
            p = Instantiate(pointPrefab);
        }

        Vector2 spawnPos = GetRandomPositionInRect(spawnArea);
        
        // Demonstrate method with ref parameter - clamps position if needed
        ClampPositionToRect(ref spawnPos, spawnArea);
        
        p.transform.position = spawnPos;
        p.SetOwnerPool(pool);

        // Hook up release event to decrement active counter
        p.OnReleased += HandlePointReleased;

        p.gameObject.SetActive(true);
        activeCount++;
    }

    private void HandlePointReleased(Point p)
    {
        p.OnReleased -= HandlePointReleased;
        activeCount = Mathf.Max(0, activeCount - 1);
    }

    /// <summary>
    /// Returns a random position within the specified rectangle.
    /// </summary>
    private Vector2 GetRandomPositionInRect(Rect r)
    {
        return new Vector2(Random.Range(r.xMin, r.xMax), Random.Range(r.yMin, r.yMax));
    }

    /// <summary>
    /// Demonstrates a method with a ref parameter.
    /// Clamps the given position to stay within the rectangle bounds.
    /// </summary>
    private void ClampPositionToRect(ref Vector2 position, Rect bounds)
    {
        position.x = Mathf.Clamp(position.x, bounds.xMin, bounds.xMax);
        position.y = Mathf.Clamp(position.y, bounds.yMin, bounds.yMax);
    }

    private void HandleGameEnd()
    {
        // Stop spawning when game ends
        enabled = false;
    }
}
