using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    

    [Header("Time Settings")]
    [Tooltip("Level duration in seconds")]
    [SerializeField] private float levelDurationSeconds = 30f;
    [SerializeField] private float earlyWarning = 10f;
    [SerializeField] private int finalCountdownTicks = 5;

    [Header("UI References")]
    [Tooltip("Text component to display remaining time")]
    [SerializeField] private TMP_Text timeDisplayText;

    [Header("Debug")]
    [Tooltip("Show time info in console")]
    [SerializeField] private bool debugMode = false;

    public float remainingTime;
    public bool isInEarlyWarning = false;
    public bool isGamePaused = false;
    public bool isGameRunning = false;
    public bool hasGameEnded = false;

    // Events
    public System.Action OnTimeUp;
    public System.Action OnEarlyWarningReached;
    public System.Action OnFinalCountdownReached;
    public System.Action OnFinalCountdownTick; 
    public System.Action OnGamePaused;
    public System.Action OnGameUnpaused;

    void Start()
    {
        StartTimer();
    }

    void Update()
    {
        HandleInput();
        UpdateTimer();
    }


    private void HandleInput() 
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }


    private void UpdateTimer()
    {
        // Only countdown if game is running and not paused
        if (!isGameRunning || isGamePaused || hasGameEnded)
            return;

        float previousTime = remainingTime;
        remainingTime -= Time.deltaTime;

        // Check for stages (early warning & final countdown)
        if (remainingTime > 0f)
        {
            // Fire event every time we cross a whole second boundary
            int previousSecond = Mathf.CeilToInt(previousTime);
            int currentSecond = Mathf.CeilToInt(remainingTime);
            

            // Check for crossing seconds boundaries
            if (previousSecond != currentSecond)    
            {
                // check for early warning
                if (currentSecond <= earlyWarning)
                {
                    if (!isInEarlyWarning)
                    {
                        OnEarlyWarningReached?.Invoke(); //fire event for reaching early warning - but only the first time
                    }
                    isInEarlyWarning = true;
                }

                // check for final countdown
                if (currentSecond <= finalCountdownTicks && currentSecond > 0) //check for last few seconds for updating timer TMP's color & playing sound
                {
                    if (currentSecond == finalCountdownTicks)
                    {
                        OnFinalCountdownReached?.Invoke(); //fire event for reaching final countdown

                        if (debugMode)
                            Debug.Log("Final countdown reached!");
                    }
                    OnFinalCountdownTick?.Invoke(); // fire event for every seconds tick during final countdown

                    timeDisplayText.color = Color.red;
                    timeDisplayText.text = GetFormattedTime();


                    if (debugMode)
                        Debug.Log($"Final countdown: {currentSecond} seconds remaining!");
                }
                else
                {
                    timeDisplayText.text = GetFormattedTime();


                    if (debugMode)
                        Debug.Log($"Timer decreased by one second: {currentSecond} seconds remaining!");
                }

            }
        }

        // Check if time is up
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            hasGameEnded = true;
            isGameRunning = false;

            // Fire the time up event
            OnTimeUp?.Invoke();

            if (debugMode)
                Debug.Log("Countdown reached zero");
        }
    }



    /// <summary>
    /// Starts the timer
    /// </summary>
    public void StartTimer()
    {
        remainingTime = levelDurationSeconds;
        isGameRunning = true;
        hasGameEnded = false;

        if (debugMode)
            Debug.Log($"Timer started: {levelDurationSeconds} seconds");

        timeDisplayText.text = GetFormattedTime();
    }

    /// <summary>
    /// Toggles pause/unpause state
    /// </summary>
    public void TogglePause()
    {
        if (hasGameEnded) return; // Can't pause if game has ended

        isGamePaused = !isGamePaused;

        // Pause/unpause Unity's time scale
        Time.timeScale = isGamePaused ? 0f : 1f;

        // Fire appropriate event
        if (isGamePaused)
        {
            OnGamePaused?.Invoke();
        }
        else
        {
            OnGameUnpaused?.Invoke();
        }

        if (debugMode)
            Debug.Log($"Game {(isGamePaused ? "paused" : "unpaused")}");
    }

    /// <summary>
    /// Manually pause the game
    /// </summary>
    public void PauseGame()
    {
        if (hasGameEnded || isGamePaused) return;

        isGamePaused = true;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke();

        if (debugMode)
            Debug.Log("Game paused");
    }

    /// <summary>
    /// Manually unpause the game
    /// </summary>
    public void UnpauseGame()
    {
        if (hasGameEnded || !isGamePaused) return;

        isGamePaused = false;
        Time.timeScale = 1f;
        OnGameUnpaused?.Invoke();

        if (debugMode)
            Debug.Log("Game unpaused");
    }




    /// <summary>
    /// Gets formatted time string (MM:SS)
    /// </summary>
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnDestroy()
    {
        // Reset time scale when TimeManager is destroyed
        Time.timeScale = 1f;
    }
}