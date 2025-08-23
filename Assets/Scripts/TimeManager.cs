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
    


    [Header("UI References")]
    [Tooltip("Text component to display remaining time")]
    [SerializeField] private TMP_Text timeDisplayText;
    [SerializeField] private GameObject ingameMenu;

    [Header("Debug")]
    [Tooltip("Show time info in console")]
    [SerializeField] private bool debugMode = false;

    public float currentTime;
    public bool isGamePaused = false;
    public bool isGameRunning = false;
    public bool hasGameEnded = false;

    private bool _ingameMenuShown = false;


    // Events
    public System.Action OnTimeUp;  // TODO: remove
    public System.Action OnEarlyWarningReached; // TODO: remove
    public System.Action OnFinalCountdownReached;  // TODO: remove
    public System.Action OnFinalCountdownTick;  // TODO: remove
    public System.Action OnGamePaused;
    public System.Action OnGameUnpaused;

    void Start()
    {
        GameEvents.Instance.TimeManagerInstantiated?.Invoke(this);
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
            ToggleIngameMenu();
        }
    }

    public void ToggleIngameMenu()
    {
        if (!_ingameMenuShown)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ingameMenu.SetActive(true);
            _ingameMenuShown = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            ingameMenu.SetActive(false);
            _ingameMenuShown = false;
            UnpauseGame();
        }
    }


    private void UpdateTimer()
    {
        // Only countdown if game is running and not paused
        if (!isGameRunning || isGamePaused || hasGameEnded)
            return;

        float previousTime = currentTime;
        currentTime += Time.deltaTime;


        // Fire event every time we cross a whole second boundary
        int previousSecond = Mathf.CeilToInt(previousTime);
        int currentSecond = Mathf.CeilToInt(currentTime);
            

        // Check for crossing seconds boundaries and update UI
        if (previousSecond != currentSecond)    
        {
            timeDisplayText.text = GetFormattedTime();
        }

    }


    public void StartTimer()
    {
         isGameRunning = true;
        hasGameEnded = false;

        timeDisplayText.text = GetFormattedTime();
    }

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
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}