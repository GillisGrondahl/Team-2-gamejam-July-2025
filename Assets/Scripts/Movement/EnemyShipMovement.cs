using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class EnemyShipMovement : MonoBehaviour
{
    [Header("Pitching Motion (Bow/Stern lifting/falling)")]
    [Tooltip("Maximum pitch angle in degrees")]
    [SerializeField] private float _pitchAmplitude = 8f;
    [Tooltip("How often the pitching motion occurs (every X seconds)")]
    [SerializeField] private float _pitchPeriod = 10f;

    [Header("Overall Motion Intensity")]
    [SerializeField] private float _motionIntensity = 1f;

    [Header("Closing Distance")]
    [SerializeField] private Transform _startPosition;
    [SerializeField] private Transform _endPosition;

    [Header("Gunshot Schedule")]
    [Tooltip("List of distances (0-1 based on timer progress) at which gunshots fire")]
    [SerializeField] private List<float> _gunshotDistances = new List<float>();

    [Header("References")]
    [SerializeField] private ParticleSystem _sprayParticles;
    [SerializeField] private ParticleSystem _gunshotParticles;
    [SerializeField] private MMF_Player _MMFGunshot;

    private Vector3 _initialRotation;
    private float _pitchOffset;
    private float _nextSplashTime = 0f;

    private float distanceToPlayer = 0f;

    // Track which gunshots have already been fired
    private HashSet<int> _firedGunshotIndices = new HashSet<int>();

    private ITimerService _timerService;

    [Inject]
    private void Construct(ITimerService timerService)
    {
        _timerService = timerService;
    }

    private void OnEnable()
    {
        _timerService.Tick += OnTimerTick;
    }

    private void OnDisable()
    {
        _timerService.Tick -= OnTimerTick;
    }

    private void Awake()
    {
        // Store the initial position and rotation
        _initialRotation = transform.eulerAngles;

        // Add random offsets to make motion feel less predictable
        _pitchOffset = UnityEngine.Random.Range(0f, 2f * Mathf.PI);

        // Start at half period so splash happens at lowest point first
        _nextSplashTime = (_pitchPeriod / _motionIntensity) * 1.4f;

    }

    void Update()
    {
        // scale overall intensity with time 
        float _time = Time.time * _motionIntensity;

        CalcPitching(_time);
    }

    public void CalcPitching(float _time)
    {
        float _pitchFrequency = 1f / _pitchPeriod;

        // Calculate pitching
        float _pitchMotion = Mathf.Sin(_time * _pitchFrequency * 2f * Mathf.PI + _pitchOffset) * (_pitchAmplitude * _motionIntensity);

        // Apply the motion to rotation (pitching) - z-axis, because we're viewing the ship sideways!
        Vector3 newRotation = _initialRotation;
        newRotation.z += _pitchMotion;
        transform.eulerAngles = newRotation;

        // Play splash particles when bow reaches lowest point (once every pitch period)
        if (Time.time >= _nextSplashTime)
        {
            if (_sprayParticles != null)
            {
                _sprayParticles.Play();
            }
            _nextSplashTime = Time.time + (_pitchPeriod / _motionIntensity);
        }
    }

    private void OnTimerTick(float time)
    {
        distanceToPlayer = _timerService.Progress;

        // Update ship position
        transform.position = new Vector3(
            Vector3.Lerp(_startPosition.position, _endPosition.position, distanceToPlayer).x,
            transform.position.y,
            Vector3.Lerp(_startPosition.position, _endPosition.position, distanceToPlayer).z
        );

        // Check each gunshot distance
        for (int i = 0; i < _gunshotDistances.Count; i++)
        {
            // Skip if already fired
            if (_firedGunshotIndices.Contains(i))
                continue;

            // Check if we've reached or passed the trigger distance
            if (distanceToPlayer >= _gunshotDistances[i])
            {
                FireGunshot();
                _firedGunshotIndices.Add(i); // Mark this gunshot as fired
            }
        }
    }

    public void FireGunshot()
    {
        // we could calculate the delay between the soundwave arriving and the gunshot being fired based on the distance here,
        // but I think that would be overengineered for now. So instead, we're using a MMF Player with a static pause of 1s between them.
        /*
        if (_gunshotParticles != null)
        {
            _gunshotParticles.Play();
        }*/

        _MMFGunshot.PlayFeedbacks();
    }
}