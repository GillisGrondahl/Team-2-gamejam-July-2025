using UnityEngine;

public class EnemyShipMovement : MonoBehaviour
{
    [Header("Pitching Motion (Bow/Stern lifting/falling)")]
    [Tooltip("Maximum pitch angle in degrees")]
    [SerializeField] private float _pitchAmplitude = 8f;
    [Tooltip("How often the pitching motion occurs (every X seconds)")]
    [SerializeField] private float _pitchPeriod = 10f;

    [Header("Overall Motion Intensity")]
    [SerializeField] private float _motionIntensity = 1f;

    [Header("References")]
    [SerializeField] private ParticleSystem _sprayParticles;

    private Vector3 _initialRotation;
    private float _pitchOffset;


    // Track when splash was last played
    private float _nextSplashTime = 0f;

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
}
