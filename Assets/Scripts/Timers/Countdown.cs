using System;
using UnityEngine;
using VContainer.Unity;

public class Countdown : ITimerService, ITickable
{
    private float _timeScale = 1f;

    public event Action<float> Tick = delegate { };
    public event Action Completed = delegate { };

    public float Duration { get; private set; }

    public float Remaining { get; private set; }

    public float Elapsed { get; private set; }

    public float Progress => Duration <= 0f ? 1f : Math.Clamp(Elapsed / Duration, 0f, 1f);

    public float TimeScale
    {
        get => _timeScale;
        set => _timeScale = value < 0f ? 0f : value;
    }

    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }

    public void Start(float durationSeconds, bool restart = true)
    {
        if (durationSeconds < 0f) durationSeconds = 0f;

        if (IsRunning && !restart)
            return;

        Duration = durationSeconds;
        Elapsed = 0f;
        Remaining = Duration;
        IsRunning = true;
        IsPaused = false;

        // Emit an immediate tick so listeners can sync UI on start
        Tick(Remaining);

        // Edge case: zero duration → complete immediately
        if (Duration <= 0f)
            CompleteNow();
    }

    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        IsPaused = false;
        Elapsed = 0f;
        Remaining = 0f;
    }

    public void Pause()
    {
        if (!IsRunning || IsPaused) return;
        IsPaused = true;
    }

    public void Resume()
    {
        if (!IsRunning || !IsPaused) return;
        IsPaused = false;
    }


    public void Update(float deltaTime)
    {
        if (!IsRunning || IsPaused || Duration <= 0f || TimeScale <= 0f) return;

        var dt = deltaTime * TimeScale;
        if (dt <= 0f) return;

        Elapsed += dt;
        Remaining = Math.Max(0f, Duration - Elapsed);

        Tick(Remaining);

        if (Remaining <= 0f)
            CompleteNow();
    }

    public void AddTime(float time)
    {
        Elapsed -= time;
    }


    private void CompleteNow()
    {
        Elapsed = Duration;
        Remaining = 0f;

        IsRunning = false;
        IsPaused = false;

        Completed();
    }

    void ITickable.Tick()
    {
        Update(Time.deltaTime);
    }
}
