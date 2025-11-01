using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelOneLifetimeScope : LifetimeScope
{
    [SerializeField] LevelManager levelManager;



    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(levelManager);
        builder.Register<Countdown>(Lifetime.Scoped).As<ITimerService>();

        builder.RegisterEntryPoint<TimerTickDriver>();
        builder.RegisterComponentInHierarchy<CountdownUI>().As<ITimerUI>();
    }
}
