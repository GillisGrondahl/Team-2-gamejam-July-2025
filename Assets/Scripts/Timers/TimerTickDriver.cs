using UnityEngine;
using VContainer.Unity;

public class TimerTickDriver : ITickable
{
    private readonly ITimerService _timer;

    public TimerTickDriver(ITimerService timer)
    {
        _timer = timer;
    }

    public void Tick()
    {
        _timer.Update(Time.deltaTime);
    }

}
