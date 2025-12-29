using System;

public interface ITimerService
{
   
    /// Fired every tick with remaining seconds.
    event Action<float> Tick;

    /// Fired once when the countdown reaches 0.
    event Action Completed;

    /// Total duration (seconds) for the current run.
    float Duration { get; }

    /// Remaining time (seconds). 0 when completed/stopped.
    float Remaining { get; }

    /// Elapsed time (seconds) since Start().
    float Elapsed { get; }

    /// 0..1 progress (Elapsed/Duration). 1 when completed.
    float Progress { get; }

    /// Time scale multiplier (default 1). Affects Update().
    float TimeScale { get; set; }

    bool IsRunning { get; }
    bool IsPaused { get; }

    /// Start a new countdown. If already running:
    /// - restart == true ? restarts with new duration
    /// - restart == false ? ignored if running
    void Start(float durationSeconds, bool restart = true);

    void Pause();
    void Resume();

    /// Stop the timer immediately (does not fire Completed).
    void Stop();

    void AddTime(float time);

    /// Manual update: pass deltaTime (usually Time.deltaTime).
    void Update(float deltaTime);

}
