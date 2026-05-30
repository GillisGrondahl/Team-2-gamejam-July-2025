using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using VContainer;
using VContainer.Unity;

// A minimal MonoBehaviour whose only job is to run coroutines for non-MonoBehaviour classes
public class CoroutineRunner : MonoBehaviour { }

public class FMODAudioManager : IAudioService, IStartable, IDisposable
{
    readonly FMODTrackLookup _fmodLookup;
    readonly ISettingsService _settingsService;

    float _masterVolume = 0.9f;
    float _bgmVolume = 0.7f;
    float _ambVolume = 0.7f;
    float _sfxVolume = 1.0f;

    Bus masterBus;
    Bus BGMBus;
    Bus AMBBus;
    Bus SFXBus;

    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = Mathf.Clamp01(value);
            SetVolume(masterBus, _masterVolume);
            _settingsService.Apply(settings => settings.Audio.MasterVolume = _masterVolume);
        }
    }

    public float BgmVolume
    {
        get => _bgmVolume;
        set
        {
            _bgmVolume = Mathf.Clamp01(value);
            SetVolume(BGMBus, _bgmVolume);
            _settingsService.Apply(settings => settings.Audio.MusicVolume = _bgmVolume);
        }
    }

    public float AmbVolume
    {
        get => _ambVolume;
        set
        {
            _ambVolume = Mathf.Clamp01(value);
            SetVolume(AMBBus, _ambVolume);
            _settingsService.Apply(settings => settings.Audio.AmbienceVolume = _ambVolume);
        }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = Mathf.Clamp01(value);
            SetVolume(SFXBus, _sfxVolume);
            _settingsService.Apply(settings => settings.Audio.SfxVolume = _sfxVolume);
        }
    }                                                                                                          

    EventInstance _bgmInstance;
    EventInstance _ambInstance;

    List<EventInstance> _eventInstances = new ();
    readonly Dictionary<TrackChannel, string> _currentProviderKeyByChannel = new();


    [Inject]
    [UnityEngine.Scripting.Preserve]
    public FMODAudioManager(FMODTrackLookup fmodLookup, ISettingsService settingsService)
    {
        _fmodLookup = fmodLookup;
        _settingsService = settingsService;
    }

    public void Start()
    {
        // Create a persistent coroutine runner so we can yield without blocking the main thread
        var go = new GameObject("FMODAudioCoroutineRunner");
        GameObject.DontDestroyOnLoad(go);
        var runner = go.AddComponent<CoroutineRunner>();

        // Load banks and trigger sample data preloading (fixes first-play stutter on WebGL)
        RuntimeManager.LoadBank("BGM");
        RuntimeManager.StudioSystem.getBank("bank:/BGM", out Bank bgmBank);
        bgmBank.loadSampleData();

        RuntimeManager.LoadBank("Ambience");
        RuntimeManager.StudioSystem.getBank("bank:/Ambience", out Bank ambBank);
        ambBank.loadSampleData();

        RuntimeManager.LoadBank("SFX");
        RuntimeManager.StudioSystem.getBank("bank:/SFX", out Bank sfxBank);
        sfxBank.loadSampleData();

        RuntimeManager.LoadBank("Master");
        RuntimeManager.StudioSystem.getBank("bank:/Master", out Bank masterBank);
        masterBank.loadSampleData();

        RuntimeManager.CoreSystem.mixerSuspend();
        RuntimeManager.CoreSystem.mixerResume();

        // Assign busses
        masterBus = RuntimeManager.GetBus("bus:/");
        BGMBus = RuntimeManager.GetBus("bus:/BGM");
        AMBBus = RuntimeManager.GetBus("bus:/Ambience");
        SFXBus = RuntimeManager.GetBus("bus:/SFX");

        // Load saved volume settings
        var audio = _settingsService.Current.Audio;
        _masterVolume = audio.MasterVolume;
        _bgmVolume = audio.MusicVolume;
        _ambVolume = audio.AmbienceVolume;
        _sfxVolume = audio.SfxVolume;

        // Set initial volumes
        SetVolume(masterBus, _masterVolume);
        SetVolume(BGMBus, _bgmVolume);
        SetVolume(AMBBus, _ambVolume);
        SetVolume(SFXBus, _sfxVolume);

        // Defer the wait for banks and sample loading to a coroutine to avoid blocking the main thread
        runner.StartCoroutine(WaitForAudioReady());
    }

    private IEnumerator WaitForAudioReady()
    {
        // Wait for master banks to be loaded before proceeding
        while (!RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
        }

        RuntimeManager.WaitForAllSampleLoading();

        Debug.Log("FMODAudioManager: audio ready");
    }

    private void SetVolume(Bus volumeControlBus, float volume)
    {
        volumeControlBus.setVolume(volume);
    }

    public void StartTrack(AudioTrackData track, bool fadeout = true, float fadeInSeconds = 0.5f)
    {
        if (track == null)
        {
            Debug.LogWarning("StartTrack called with null track");
            return;
        }

        if (string.IsNullOrEmpty(track.ProviderKey))
        {
            Debug.LogWarning($"Track '{track.name}' has empty ProviderKey");
            return;
        }

        if (_currentProviderKeyByChannel.TryGetValue(track.Channel, out var currentKey) &&
            currentKey == track.ProviderKey)
        {
            return;
        }

        if (!_fmodLookup.TryGet(track.ProviderKey, out var eventRef))
        {
            Debug.LogWarning($"No FMOD event for ProviderKey '{track.ProviderKey}'");
            return;
        }

        StopTrack(track.Channel, fadeout);
        PlayPersistent(eventRef, track.Channel, fadeout, fadeInSeconds);

        _currentProviderKeyByChannel[track.Channel] = track.ProviderKey;
    }


    public void StopTrack(TrackChannel channel, bool fadeout=true)
    {
        ref var inst = ref GetInstanceRef(channel);
        if (!inst.isValid()) return;

        if (fadeout)
        {
            inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        else
        {
            inst.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }

        inst.release();
        _eventInstances.Remove(inst);
        inst.clearHandle();

        _currentProviderKeyByChannel.Remove(channel);
    }

    private void PlayPersistent(EventReference eventRef, TrackChannel channel, bool fadeOut = true, float fadeIn = 0f)
    {
        ref var inst = ref GetInstanceRef(channel);

        if (inst.isValid())
        {
            if(fadeOut)
            {
                inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
            else
            {
                inst.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
            inst.release();
            _eventInstances.Remove(inst);
            inst.clearHandle();
        }

        if (eventRef.IsNull)
        {
            Debug.LogWarning($"Tried to play null event on channel {channel}");
            return;
        }

        inst = RuntimeManager.CreateInstance(eventRef);
        _eventInstances.Add(inst);

        inst.start();
    }

    private ref EventInstance GetInstanceRef(TrackChannel channel)
    {
        switch (channel)
        {
            case TrackChannel.BGM:
                return ref _bgmInstance;
            case TrackChannel.Ambience:
                return ref _ambInstance;
            default:
                throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
        }
    }

    public void SetParameter(TrackChannel channel, string paramName, float newValue)
    {

        ref var inst = ref GetInstanceRef(channel);
        if (!inst.isValid()) return;

        inst.setParameterByName(paramName, newValue);
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public void ResetAudio()
    {
        ResetTrack(TrackChannel.BGM);
        ResetTrack(TrackChannel.Ambience);
    }

    private void ResetTrack(TrackChannel channel)
    {
        ref var inst = ref GetInstanceRef(channel);
        if (!inst.isValid()) return;

        inst.setParameterByName("Pitch", 0f);
        inst.setParameterByName("Fade", 1f);
    }

    public void Dispose()
    {
        // Clean up FMOD instances
        foreach (EventInstance instance in _eventInstances)
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
        }
    }
}
