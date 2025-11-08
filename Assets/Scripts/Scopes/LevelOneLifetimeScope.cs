using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelOneLifetimeScope : LifetimeScope
{
    [SerializeField] LevelManager levelManager;
    [SerializeField] LevelData levelData;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(levelManager);
        builder.RegisterInstance(levelData);
        builder.RegisterEntryPoint<TimerTickDriver>();
        builder.RegisterEntryPoint<RecipeSystem>().AsSelf();
        builder.Register<Countdown>(Lifetime.Scoped).As<ITimerService>();
        //builder.RegisterComponentInHierarchy<CountdownUI>().As<ITimerUI>();
    }
}
