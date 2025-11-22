using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelLifetimeScope : LifetimeScope
{
   // [SerializeField] LevelData levelData;

    protected override void Configure(IContainerBuilder builder)
    {
        //builder.RegisterInstance(levelData);
        builder.RegisterEntryPoint<TimerTickDriver>();
        builder.RegisterEntryPoint<RecipeSystem>().AsSelf();
        builder.RegisterEntryPoint<LevelManager>().AsSelf();
        builder.Register<Countdown>(Lifetime.Scoped).As<ITimerService>();
        //builder.RegisterComponentInHierarchy<CountdownUI>().As<ITimerUI>();
    }
}
