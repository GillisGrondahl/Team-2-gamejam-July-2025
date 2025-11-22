using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<RecipeSystem>().AsSelf();
        builder.RegisterEntryPoint<LevelManager>().AsSelf();
        builder.Register<Countdown>(Lifetime.Scoped).AsImplementedInterfaces();
    }
}
