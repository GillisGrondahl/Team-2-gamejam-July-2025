using VContainer;
using VContainer.Unity;
using UnityEngine;

public class ProjectLifetimeScope : LifetimeScope
{
    [SerializeField]
    AudioManager audioManager;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInNewPrefab(audioManager, Lifetime.Singleton).DontDestroyOnLoad();
        builder.RegisterEntryPoint<InputManager>();

        builder.RegisterBuildCallback(r => r.Resolve<AudioManager>());
    }
}
