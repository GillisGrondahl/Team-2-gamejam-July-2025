using Unity.VisualScripting;
using UnityEngine;
using VContainer;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private float levelDurationInSeconds = 60f;
    [SerializeField] private float earlyWarningInSeconds = 10f;
    [SerializeField] private int finalCountdownInSeconds = 5;

    public float LevelDurationInSeconds => levelDurationInSeconds;
    public float EarlyWarningInSeconds => earlyWarningInSeconds;
    public int FinalCountdownInSeconds => finalCountdownInSeconds;

    private bool _earlyWarningTriggered = false;
    private int _lastAnnouncedSecond = int.MinValue;


    private AudioManager _audio;
    private ITimerService _timer;
    private ITimerUI _timerUI;
    private IInputService _input;


    //TODO: Move this to Controller, LevelManager doing to many things
    [Inject]
    private void Construct(ITimerService timerService, AudioManager audioService, ITimerUI timerUI, IInputService input)
    {
        _timer = timerService;
        _audio = audioService;
        _timerUI = timerUI;
        _input = input;
    }

    private void _input_Escape(bool obj)
    {
        throw new System.NotImplementedException();
    }

    //Add check for the lvl1 info
    private void Start()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        _earlyWarningTriggered = false;
        _timer.Start(levelDurationInSeconds);
    }

    private void OnEnable()
    {
        _input.Escape += _input_Escape;
        _timer.Tick += OnTimerTick;
        _timer.Completed += OnTimerCompleted;
    }

    private void OnDisable()
    {
        _timer.Tick -= OnTimerTick;
        _timer.Completed -= OnTimerCompleted;
    }

    private void OnTimerTick(float time) 
    {
        _timerUI.ShowTime(time);

        if (!_earlyWarningTriggered && time <= earlyWarningInSeconds)
        { 
            _earlyWarningTriggered = true;
            _timerUI.ShowWarning();
            _audio.SetTempo(0.3f);
        }


        if (finalCountdownInSeconds > 0)
        {
            int wholeSeconds = Mathf.CeilToInt(time);
            if(wholeSeconds != _lastAnnouncedSecond && wholeSeconds <= finalCountdownInSeconds && wholeSeconds > 0)
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
}

