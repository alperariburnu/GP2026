using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Clickable point that awards score when collected.
/// Required components on prefab: SpriteRenderer, Collider2D (CircleCollider2D recommended).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Point : CollectibleBase, ICollectible
{
    [Header("Value")]
    [SerializeField]
    [Tooltip("Score awarded when collected")]
    private int pointValue = 10;

    [Header("Pop Animation")]
    [SerializeField]
    [Tooltip("Target scale multiplier for pop animation")]
    private float popScale = 1.2f;

    [SerializeField]
    [Tooltip("Duration of the pop animation in seconds")]
    private float popDuration = 0.12f;

    [Header("Audio")]
    [SerializeField]
    [Tooltip("Optional clip to play when collected")]
    private AudioClip collectClip;

    // Pool that owns this instance (nullable)
    private SimplePool<Point> ownerPool;

    // Event invoked when this point is returned to the pool
    public Action<Point> OnReleased;

    private Collider2D cachedCollider;
    private bool collected;
    private Vector3 initialScale;

    private void Awake()
    {
        cachedCollider = GetComponent<Collider2D>();
        initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        // Reset state for reuse
        collected = false;
        if (cachedCollider != null)
            cachedCollider.enabled = true;

        transform.localScale = initialScale;
    }

    /// <summary>
    /// Assigns the owning pool for later release. Call this when spawning from a pool.
    /// </summary>
    public void SetOwnerPool(SimplePool<Point> pool)
    {
        ownerPool = pool;
    }

    /// <summary>
    /// Public API to collect this point. Safe against multiple calls.
    /// Overrides base Collect to add score, sound and pooling behavior.
    /// </summary>
    public override void Collect()
    {
        if (collected)
            return;

        collected = true;

        if (cachedCollider != null)
            cachedCollider.enabled = false;

        // Award score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(pointValue);
        }
        else
        {
            Debug.LogWarning("Point.Collect: GameManager instance not found.");
        }

        // Play sound at this position if provided
        if (collectClip != null)
        {
            AudioSource.PlayClipAtPoint(collectClip, transform.position);
        }

        StartCoroutine(PopAndRelease());
    }

    /// <summary>
    /// Returns the point value. Demonstrates a simple getter method.
    /// Implements ICollectible.GetValue and overrides base if needed.
    /// </summary>
    public override int GetValue()
    {
        return pointValue;
    }

    /// <summary>
    /// Sets the point value with validation. Demonstrates a method with parameter and validation.
    /// </summary>
    public void SetValue(int newValue)
    {
        if (newValue > 0)
        {
            pointValue = newValue;
        }
        else
        {
            Debug.LogWarning("Point.SetValue: Value must be positive");
        }
    }

    /// <summary>
    /// Modifies the value by adding an amount. Uses ref parameter for demonstration.
    /// This shows how to modify and return data through parameters.
    /// </summary>
    public void ModifyValueBy(ref int amount, out int resultingValue)
    {
        if (amount != 0)
        {
            pointValue += amount;
        }
        resultingValue = pointValue;
    }

    private IEnumerator PopAndRelease()
    {
        float elapsed = 0f;
        Vector3 start = transform.localScale;
        Vector3 target = initialScale * popScale;

        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popDuration);
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        // Return to pool if available, otherwise destroy
        if (ownerPool != null)
        {
            ownerPool.Release(this);
            OnReleased?.Invoke(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
