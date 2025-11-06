using FMOD;
using UnityEngine;
using UnityEngine.Events;
using VContainer;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelDifficultyData levelDifficultyData;

    private bool _earlyWarningTriggered = false;
    private int _lastAnnouncedSecond = int.MinValue;

    private bool _isGamePaused = false;


    private AudioManager _audio;
    private ITimerService _timer;
    private ITimerUI _timerUI;
    private IInputService _input;

    public event UnityAction GamePaused;
    public event UnityAction GameResumed;


    //TODO: Move this to Controller, LevelManager doing to many things
    [Inject]
    private void Construct(ITimerService timerService, AudioManager audioService, ITimerUI timerUI, IInputService input)
    {
        _timer = timerService;
        _audio = audioService;
        _timerUI = timerUI;
        _input = input;
    }


    //Add check for the lvl1 info
    private void Start()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        _earlyWarningTriggered = false;
        _timer.Start(levelDifficultyData.levelDurationInSeconds);
        ResumeGame();
    }

    private void OnEnable()
    {
        _input.Escape += TogglePause;
        _timer.Tick += OnTimerTick;
        _timer.Completed += OnTimerCompleted;
    }

    private void OnDisable()
    {
        _input.Escape -= TogglePause;
        _timer.Tick -= OnTimerTick;
        _timer.Completed -= OnTimerCompleted;
    }

    private void OnTimerTick(float time) 
    {
        _timerUI.ShowTime(time);

        if (!_earlyWarningTriggered && time <= levelDifficultyData.earlyWarningInSeconds)
        { 
            _earlyWarningTriggered = true;
            _timerUI.ShowWarning();
            _audio.SetTempo(0.3f);
        }


        if (levelDifficultyData.finalCountdownInSeconds > 0)
        {
            int wholeSeconds = Mathf.CeilToInt(time);
            if(wholeSeconds != _lastAnnouncedSecond && wholeSeconds <= levelDifficultyData.finalCountdownInSeconds && wholeSeconds > 0)
            {
                _lastAnnouncedSecond = wholeSeconds;
                _timerUI.ShowTimeEnding();
                _audio.SetTempo(0.5f);
            }
        }
    }

    private void OnTimerCompleted()
    {
        //Audio?
    }

    public void TogglePause()
    {
        if (_isGamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    private void PauseGame()
    {
        Time.timeScale = 0f;
        _isGamePaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GamePaused?.Invoke();
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        _isGamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameResumed?.Invoke();
    }
}

