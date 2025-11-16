using VContainer;
using VContainer.Unity;
using UnityEngine;

public class ProjectLifetimeScope : LifetimeScope
{
    [SerializeField] AudioManager audioManager;
    [SerializeField] SceneController sceneController;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInNewPrefab(audioManager, Lifetime.Singleton).DontDestroyOnLoad();
        builder.RegisterComponentInNewPrefab(sceneController, Lifetime.Singleton).DontDestroyOnLoad().AsImplementedInterfaces();
        builder.RegisterEntryPoint<InputManager>();

        builder.RegisterBuildCallback(r => r.Resolve<AudioManager>());
        builder.RegisterBuildCallback(r => r.Resolve<SceneController>());   
    }
}
