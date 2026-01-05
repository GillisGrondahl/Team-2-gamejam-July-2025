using VContainer;
using VContainer.Unity;
using UnityEngine;

public class ProjectLifetimeScope : LifetimeScope
{
    [SerializeField] private SceneController sceneController;
    [SerializeField] private GameSettingsData gameSettings;
    [SerializeField] private FMODTrackLookup fmodTrackLookup;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInNewPrefab(sceneController, Lifetime.Singleton).DontDestroyOnLoad().AsImplementedInterfaces();
        builder.RegisterEntryPoint<FMODAudioManager>();
        builder.RegisterEntryPoint<InputManager>();

        builder.RegisterBuildCallback(r => r.Resolve<ISceneController>());

#if UNITY_WEBGL && !UNITY_EDITOR
        builder.Register<ISettingsStorage, PlayerPrefsSettingsStorage>(Lifetime.Singleton);
#else
        builder.Register<ISettingsStorage, FileSettingsStorage>(Lifetime.Singleton);
#endif

        builder.RegisterInstance(fmodTrackLookup);
        builder.RegisterInstance(gameSettings.Value);
        builder.Register<ISettingsService, SettingsService>(Lifetime.Singleton);
        builder.Register<ILeaderboardService, LocalLeaderboardService>(Lifetime.Singleton);
        builder.Register<IGameStateService, GameStateService>(Lifetime.Singleton);
        builder.RegisterEntryPoint<GameStateApplier>();
    }
}
