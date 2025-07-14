using UnityEngine;

public class ShipMotion : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Debug toggle: freeze motion")]
    [SerializeField] private bool _freeze = false;

    [Header("Heaving Motion (Up/Down)")]
    [Tooltip("How high the ship moves up and down")]
    [SerializeField] private float _heaveAmplitude = 1f;
    [Tooltip("How often the heaving motion occurs (every X seconds)")]
    [SerializeField] private float _heavePeriod = 10f;
    

    [Header("Pitching Motion (Bow/Stern lifting/falling)")]
    [Tooltip("Maximum pitch angle in degrees")]
    [SerializeField] private float _pitchAmplitude = 8f;
    [Tooltip("How often the pitching motion occurs (every X seconds)")]
    [SerializeField] private float _pitchPeriod = 10f;

    [Header("Wave Variation")]
    [Tooltip("Adds randomness to wave timing for more natural motion")]
    [SerializeField] private float _waveVariation = 0.1f;

    [Tooltip("Speed multiplier for overall motion intensity")]
    [Range(0f, 3f)]
    [SerializeField] private float _motionIntensity = 1f;

    private Vector3 _initialPosition;
    private Vector3 _initialRotation;
    private float _heaveOffset;
    private float _pitchOffset;

    void Start()
    {
        // Store the initial position and rotation
        _initialPosition = transform.position;
        _initialRotation = transform.eulerAngles;

        // Add random offsets to make motion feel less predictable
        _heaveOffset = Random.Range(0f, 2f * Mathf.PI);
        _pitchOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {

        // scale overall intensity with time 
        float _time = Time.time * _motionIntensity;


        if (_freeze != true)    // debug toggle
        {
            CalcHeaving(_time);

            CalcPitching(_time);
        }

    }

    public void CalcHeaving(float _time)
    {
        float _heaveFrequency = 1f / _heavePeriod;

        // Calculate heaving (up/down)
        float _heaveMotion = Mathf.Sin(_time * _heaveFrequency * 2f * Mathf.PI + _heaveOffset) * _heaveAmplitude;

        // wave variation for more chatic waves
        float _heaveVariation = Mathf.Sin(_time * _heaveFrequency * 1.3f * 2f * Mathf.PI + _heaveOffset + 1f) * (_heaveAmplitude * _waveVariation);


        // Apply the motion to position (heaving)
        Vector3 newPosition = _initialPosition;
        newPosition.y += _heaveMotion + _heaveVariation;
        transform.position = newPosition;
    }

    public void CalcPitching(float _time)
    {
        float _pitchFrequency = 1f / _pitchPeriod;

        // Calculate pitching
        float _pitchMotion = Mathf.Sin(_time * _pitchFrequency * 2f * Mathf.PI + _pitchOffset) * _pitchAmplitude;

        // wave variation for more chaotic waves
        float _pitchVariation = Mathf.Sin(_time * _pitchFrequency * 0.8f * 2f * Mathf.PI + _pitchOffset + 2f) * (_pitchAmplitude * _waveVariation);

        // Apply the motion to rotation (pitching) - z-axis, because we're viewing the ship sideways!
        Vector3 newRotation = _initialRotation;
        newRotation.z += _pitchMotion + _pitchVariation;
        transform.eulerAngles = newRotation;

    }

    // adjust motion intensity at runtime
    public void SetMotionIntensity(float intensity)
    {
        _motionIntensity = Mathf.Clamp(intensity, 0f, 3f);
    }
}