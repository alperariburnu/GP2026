using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Centralized input handling for mouse clicks. Performs Physics2D.Raycast at the input position.
/// Supports both the legacy Input API and the new Input System (mouse only).
/// </summary>
public class ClickController : MonoBehaviour
{
    private Camera mainCamera;
    private int clickCount = 0;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("ClickController: Camera.main not found. Attempting to find any camera.");
            mainCamera = FindObjectOfType<Camera>();
        }

        if (mainCamera == null)
            Debug.LogError("ClickController: No camera found in the scene. Input will not work.");
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        HandleNewInputSystem();
#else
        HandleLegacyInput();
#endif
    }

    private void HandleLegacyInput()
    {
        // Mouse left button only
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = ScreenToWorld(Input.mousePosition);
            TryHit(worldPos);
        }
    }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    private void HandleNewInputSystem()
    {
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = mouse.position.ReadValue();
            Vector2 worldPos = ScreenToWorld(screenPos);
            TryHit(worldPos);
        }
    }
#endif

    private Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        if (mainCamera == null)
            return Vector2.zero;

        Vector3 world = mainCamera.ScreenToWorldPoint(screenPosition);
        return new Vector2(world.x, world.y);
    }

    private void TryHit(Vector2 worldPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
        if (hit.collider == null)
            return;

        Point point = hit.collider.GetComponent<Point>();
        if (point != null)
        {
            clickCount++;
            point.Collect();
            
            // Finding and using data example - find UI Canvas
            FindAndDisplayClickCount();
        }
    }

    /// <summary>
    /// Demonstrates finding game objects and using their data.
    /// Searches for Canvas and updates debug info.
    /// </summary>
    private void FindAndDisplayClickCount()
    {
        // Example of finding objects in scene and using their data
        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"ClickController: Canvas found with name: {canvas.name}");
        }

        if (GameManager.Instance != null)
        {
            int currentScore = GameManager.Instance.GetScore();
            Debug.Log($"ClickController: Points clicked this session: {clickCount}, Current score: {currentScore}");
        }
    }

    /// <summary>
    /// Returns the number of points clicked in this session.
    /// </summary>
    public int GetClickCount()
    {
        return clickCount;
    }
}
