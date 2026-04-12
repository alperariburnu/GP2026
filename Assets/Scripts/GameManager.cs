/*
README:
- Point prefab requirements:
  - SpriteRenderer (visual)
  - Collider2D (CircleCollider2D recommended)
  - Point (script)
  - Optionally an AudioSource or an AudioClip assigned to Point

- Scene setup:
  1. Create a Canvas with TextMeshProUGUI to show score.
  2. Add an empty GameObject, attach this GameManager script and assign the TextMeshProUGUI.
  3. Add ClickController (handles input) to an empty GameObject.
  4. Add PointSpawner and assign a Point prefab and tuning values.
  5. Ensure main Camera is tagged "MainCamera".

- TextMeshPro dependency: This script requires TextMeshPro (TMP) package. If TMP is not present, add it via Package Manager.
*/

using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Central game manager (singleton) responsible for global state such as score and game timer.
/// Exposes AddScore, GetScore and updates UI. Controls game start/end state.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("UI")] 
    [SerializeField]
    [Tooltip("TextMeshProUGUI used to display the current score. Assign in the Inspector.")]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    [Tooltip("Optional TextMeshProUGUI used to display remaining time. If left empty, GameManager will try to find a GameObject named 'TimerText'.")]
    private TextMeshProUGUI timerText;

    [Header("Settings")]
    [SerializeField]
    [Tooltip("Initial score on game start (useful for testing).")]
    private int initialScore = 0;

    [SerializeField]
    [Tooltip("Duration of the game in seconds. Set to 0 to disable timer.")]
    private float gameDuration = 5f;

    [SerializeField]
    [Tooltip("Automatically start the game timer on Start.")]
    private bool autoStart = true;

    private int score;
    private float remainingTime;
    private bool isGameActive;

    /// <summary>
    /// Singleton instance (read-only). Use GameManager.Instance to access.
    /// </summary>
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// Raised when the game ends.
    /// </summary>
    public event Action OnGameEnd;

    /// <summary>
    /// Whether the game is currently active (accepting input and spawning).
    /// </summary>
    public bool IsGameActive => isGameActive;

    private void Awake()
    {
        // Singleton pattern with basic safety
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        score = initialScore;
        remainingTime = Mathf.Max(0f, gameDuration);
        isGameActive = false;
    }

    private void Start()
    {
        // Attempt to auto-assign timerText if not set and a GameObject named 'TimerText' exists
        if (timerText == null && gameDuration > 0f)
        {
            var go = GameObject.Find("TimerText");
            if (go != null)
            {
                timerText = go.GetComponent<TextMeshProUGUI>();
                if (timerText == null)
                    Debug.LogWarning("GameManager: Found 'TimerText' GameObject but it has no TextMeshProUGUI component.");
            }
        }

        if (scoreText == null)
        {
            Debug.LogWarning("GameManager: scoreText is not assigned in the Inspector. Score UI will be unavailable.");
        }

        if (timerText == null && gameDuration > 0f)
        {
            Debug.LogWarning("GameManager: timerText is not assigned though gameDuration > 0. Timer will not be visible.");
        }

        UpdateScoreDisplay();
        UpdateTimerDisplay();

        if (autoStart && gameDuration > 0f)
            StartGame();
        else if (gameDuration <= 0f)
            // If no timer, set active so game runs until manually ended
            isGameActive = true;
    }

    private void Update()
    {
        if (!isGameActive)
            return;

        if (gameDuration <= 0f)
            return; // no timer

        remainingTime -= Time.deltaTime;
        UpdateTimerDisplay();

        if (remainingTime <= 0f)
        {
            EndGame();
        }
    }

    /// <summary>
    /// Allows external code to set the timer text component at runtime.
    /// </summary>
    public void SetTimerText(TextMeshProUGUI tmpro)
    {
        timerText = tmpro;
        UpdateTimerDisplay();
    }

    /// <summary>
    /// Starts the game timer and activates gameplay.
    /// </summary>
    public void StartGame()
    {
        remainingTime = Mathf.Max(0f, gameDuration);
        isGameActive = true;
        UpdateTimerDisplay();
    }

    /// <summary>
    /// Ends the game, stops gameplay and displays final score.
    /// </summary>
    public void EndGame()
    {
        if (!isGameActive)
            return;

        isGameActive = false;
        remainingTime = 0f;
        UpdateTimerDisplay();

        // Show final score prominently
        if (scoreText != null)
        {
            scoreText.text = $"Final Score: {score}";
        }

        OnGameEnd?.Invoke();
    }

    /// <summary>
    /// Safely adds points to the score and updates the UI. Negative amounts are ignored.
    /// </summary>
    /// <param name="amount">Amount of score to add (expected >= 0).</param>
    public void AddScore(int amount)
    {
        if (!isGameActive)
            return; // ignore scoring when game not active

        if (amount <= 0)
        {
            if (amount < 0)
                Debug.LogWarning("GameManager.AddScore called with negative amount. Ignoring.");
            return;
        }

        // Safe addition; checked to catch overflow during development
        try
        {
            checked { score += amount; }
        }
        catch (System.OverflowException)
        {
            Debug.LogError("GameManager.AddScore: Score overflow detected. Resetting to int.MaxValue.");
            score = int.MaxValue;
        }

        UpdateScoreDisplay();
    }

    /// <summary>
    /// Returns the current score value.
    /// </summary>
    public int GetScore()
    {
        return score;
    }

    /// <summary>
    /// Sets the score directly to a specific value. Useful for resetting or testing.
    /// </summary>
    public void SetScore(int newScore)
    {
        if (newScore < 0)
        {
            Debug.LogWarning("GameManager.SetScore: Score cannot be negative. Setting to 0.");
            score = 0;
        }
        else
        {
            score = newScore;
        }
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        if (!isGameActive && gameDuration > 0f && remainingTime <= 0f)
        {
            timerText.text = "Time: 0.00";
            return;
        }

        timerText.text = $"Time: {Mathf.Max(0f, remainingTime):0.00}";
    }
}
