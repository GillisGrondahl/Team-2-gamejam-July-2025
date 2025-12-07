using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;



public class FMODAudioManager : IAudioService, IStartable, IDisposable
{
    private readonly FMODTrackLookup _fmodLookup;
    private readonly ISettingsService _settingsService;

    private float _masterVolume = 0.9f;
    private float _bgmVolume = 0.7f;
    private float _ambVolume = 0.7f;
    private float _sfxVolume = 1.0f;

    private bool _bgmEnabled = true;

    private Bus masterBus;
    private Bus BGMBus;
    private Bus AMBBus;
    private Bus SFXBus;

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

    private EventInstance _bgmInstance;
    private EventInstance _ambInstance;

    private List<EventInstance> _eventInstances = new List<EventInstance>();

    [Inject]
    public FMODAudioManager(FMODTrackLookup fmodLookup, ISettingsService settingsService)
    {
        _fmodLookup = fmodLookup;
        _settingsService = settingsService;
    }

    public void Start()
    {

        // Ensure banks are loaded (redundant if auto-loading works, but safe)
        if (!RuntimeManager.HasBankLoaded("Master"))
        {
            RuntimeManager.LoadBank("BGM");
            RuntimeManager.LoadBank("Ambience");
            RuntimeManager.LoadBank("SFX");
            RuntimeManager.LoadBank("Master");
        }

        // Wait for all banks to finish loading
        RuntimeManager.WaitForAllSampleLoading();

        // Assign busses
        masterBus = RuntimeManager.GetBus("bus:/");
        BGMBus = RuntimeManager.GetBus("bus:/BGM");
        AMBBus = RuntimeManager.GetBus("bus:/Ambience");
        SFXBus = RuntimeManager.GetBus("bus:/SFX");

        //InitializeAmbience();

        var audio = _settingsService.Current.Audio;

        //Load saved volume settings
        _masterVolume = audio.MasterVolume;
        _bgmVolume = audio.MusicVolume;
        _ambVolume = audio.AmbienceVolume;
        _sfxVolume = audio.SfxVolume;

        // Set initial Volume
        SetVolume(masterBus, _masterVolume);
        SetVolume(BGMBus, _bgmVolume);
        SetVolume(AMBBus, _ambVolume);
        SetVolume(SFXBus, _sfxVolume);
    }

    private void SetVolume(Bus volumeControlBus, float volume)
    {
        volumeControlBus.setVolume(volume);
    }

    public void StartTrack(AudioTrackData track)
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

        if (!_fmodLookup.TryGet(track.ProviderKey, out var eventRef))
        {
            Debug.LogWarning($"No FMOD event for ProviderKey '{track.ProviderKey}'");
            return;
        }

        PlayPersistent(eventRef, track.Channel);
    }

    public void StopTrack(TrackChannel channel)
    {
        ref var inst = ref GetInstanceRef(channel);
        if (!inst.isValid()) return;

        inst.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        inst.release();
        _eventInstances.Remove(inst);
        inst.clearHandle();
    }

    private void PlayPersistent(EventReference eventRef, TrackChannel channel)
    {
        ref var inst = ref GetInstanceRef(channel);

        if (inst.isValid())
        {
            inst.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
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

    public void SetTempo(TrackChannel channel, float newValue)
    {

        ref var inst = ref GetInstanceRef(channel);
        if (!inst.isValid()) return;

        inst.setPitch(newValue);
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public void Dispose()
    {
        // Clean up FMOD instances
        foreach (EventInstance instance in _eventInstances)
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }
    }
}
