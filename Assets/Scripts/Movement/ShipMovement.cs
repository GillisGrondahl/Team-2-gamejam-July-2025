using UnityEngine;
using VContainer;

[DefaultExecutionOrder(-300)]
[RequireComponent(typeof(Rigidbody))]
public sealed class ShipMovement : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool _freeze = false;

    [Header("Heaving Motion (Up/Down)")]
    [SerializeField] private float _heaveAmplitude = 1f;
    [SerializeField] private float _heavePeriod = 10f;

    [Header("Pitching Motion")]
    [SerializeField] private float _pitchAmplitude = 8f;
    [SerializeField] private float _pitchPeriod = 10f;

    [Header("Wave Variation")]
    [SerializeField] private float _waveVariation = 0.1f;

    [Header("Startup Blend")]
    [SerializeField, Min(0f)] private float _motionBlendInSeconds = 1f;

    [Header("Frame Roots")]
    [Tooltip("Root for ship deck/environment content that should sway with ship motion. Defaults to this transform.")]
    [SerializeField] private Transform shipContentRoot;

    [Tooltip("Root for actors (player/hand/interactor) that should ride ship via delta transform, not parenting side effects.")]
    [SerializeField] private Transform actorsRoot;

    [SerializeField] private bool useDecoupledActorsFrame = false;
    [SerializeField] private bool detachActorsRootAtRuntime = false;
    [SerializeField] private bool applyShipDeltaToActors = false;
    [SerializeField] private bool autoGroupPlayerAndHand = true;

    private float _motionIntensity = 1f;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private float _heaveOffset;
    private float _pitchOffset;
    private Vector3 _previousPosition;
    private Quaternion _previousRotation;
    private float _motionBlendStartFixedTime;

    private Rigidbody _rb;

    private LevelData _levelData;
    private ISettingsService _settings;

    [Inject]
    private void Construct(SceneController sceneController, ISettingsService settings)
    {
        _levelData = sceneController.CurrentLevelData;
        _settings = settings;
    }

    private void Awake()
    {
        EnsureRigidbody();
        ConfigureRigidbody();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        EnsureRigidbody();
        ConfigureRigidbody();
    }

    private void EnsureRigidbody()
    {
        if (_rb == null)
            TryGetComponent(out _rb);

        if (_rb == null)
            _rb = gameObject.AddComponent<Rigidbody>();
    }

    private void ConfigureRigidbody()
    {
        if (_rb == null)
            return;

        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    private void Start()
    {
        if (_settings != null)
            HandleLandlubberModeChanged(_settings.Current.Gameplay);

        _initialPosition = _rb.position;
        _initialRotation = _rb.rotation;

        _heaveOffset = Random.Range(0f, 2f * Mathf.PI);
        _pitchOffset = Random.Range(0f, 2f * Mathf.PI);
        _motionBlendStartFixedTime = Time.fixedTime;

        if (useDecoupledActorsFrame)
            ResolveFrameRoots();

        _previousPosition = _rb.position;
        _previousRotation = _rb.rotation;
    }

    private void OnEnable()
    {
        if (_settings != null)
            _settings.GameplaySettingsChanged += HandleLandlubberModeChanged;
    }

    private void OnDisable()
    {
        if (_settings != null)
            _settings.GameplaySettingsChanged -= HandleLandlubberModeChanged;
    }

    private void FixedUpdate()
    {
        float t = Time.fixedTime * _motionIntensity;
        float motionBlend = EvaluateMotionBlend();

        Vector3 pos = _rb.position;
        Quaternion rot = _rb.rotation;

        if (!_freeze)
        {
            pos = CalcHeavingPosition(t, motionBlend);
            rot = CalcPitchingRotation(t, motionBlend);
        }

        _rb.MovePosition(pos);
        _rb.MoveRotation(rot);

        ApplyShipDeltaToActors(pos, rot);
        _previousPosition = pos;
        _previousRotation = rot;
    }

    public void HandleLandlubberModeChanged(GameplaySettings gameplaySettings)
    {
        float configuredIntensity = _levelData != null ? _levelData.waveMotionIntensity : 1f;
        _motionIntensity = gameplaySettings.LandlubberMode ? 0f : configuredIntensity;
    }

    private float EvaluateMotionBlend()
    {
        if (_motionBlendInSeconds <= 0f)
            return 1f;

        float normalized = (Time.fixedTime - _motionBlendStartFixedTime) / _motionBlendInSeconds;
        normalized = Mathf.Clamp01(normalized);
        return normalized * normalized * (3f - (2f * normalized)); // SmoothStep
    }

    private Vector3 CalcHeavingPosition(float t, float motionBlend)
    {
        float f = 1f / _heavePeriod;

        float heave = Mathf.Sin(t * f * 2f * Mathf.PI + _heaveOffset) * _heaveAmplitude;
        float variation = Mathf.Sin(t * f * 1.3f * 2f * Mathf.PI + _heaveOffset + 1f) * (_heaveAmplitude * _waveVariation);

        Vector3 p = _initialPosition;
        p.y += (heave + variation) * motionBlend;
        return p;
    }

    private Quaternion CalcPitchingRotation(float t, float motionBlend)
    {
        float f = 1f / _pitchPeriod;

        float pitch = Mathf.Sin(t * f * 2f * Mathf.PI + _pitchOffset) * (_pitchAmplitude * _motionIntensity);
        float variation = Mathf.Sin(t * f * 0.8f * 2f * Mathf.PI + _pitchOffset + 2f) * (_pitchAmplitude * _waveVariation);

        // Your original used Z axis for pitch (side view). Keep that.
        Vector3 e = _initialRotation.eulerAngles;
        e.z += (pitch + variation) * motionBlend;
        return Quaternion.Euler(e);
    }

    private void ResolveFrameRoots()
    {
        if (shipContentRoot == null)
            shipContentRoot = transform;

        if (actorsRoot == null)
        {
            actorsRoot = ResolveDefaultActorsRoot();
        }

        if (actorsRoot == null || actorsRoot == transform)
            return;

        if (detachActorsRootAtRuntime && actorsRoot.IsChildOf(transform))
        {
            actorsRoot.SetParent(transform.parent, true);
        }

        if (actorsRoot.IsChildOf(transform))
        {
            applyShipDeltaToActors = false;
            Debug.LogWarning($"{nameof(ShipMovement)}: actorsRoot is still child of ship. Delta ride compensation disabled.", this);
        }
    }

    private Transform ResolveDefaultActorsRoot()
    {
        var playerMovement = GetComponentInChildren<PlayerMovement>(true);
        var handFollower = GetComponentInChildren<HandFollower>(true);
        var interactor = GetComponentInChildren<Interactor>(true);

        Transform playerTransform = playerMovement != null ? playerMovement.transform : null;
        Transform handTransform = handFollower != null ? handFollower.transform : null;
        Transform interactorTransform = interactor != null ? interactor.transform : null;

        if (!autoGroupPlayerAndHand)
            return playerTransform != null ? playerTransform : (handTransform != null ? handTransform : interactorTransform);

        int rootChildActorCount = 0;
        if (playerTransform != null && playerTransform.parent == transform)
            rootChildActorCount++;
        if (handTransform != null && handTransform.parent == transform && handTransform != playerTransform)
            rootChildActorCount++;
        if (interactorTransform != null && interactorTransform.parent == transform &&
            interactorTransform != playerTransform && interactorTransform != handTransform)
        {
            rootChildActorCount++;
        }

        if (rootChildActorCount < 2)
            return playerTransform != null ? playerTransform : (handTransform != null ? handTransform : interactorTransform);

        Transform anchor = playerTransform != null ? playerTransform : (handTransform != null ? handTransform : interactorTransform);
        if (anchor == null)
            return null;

        GameObject actorsRootObject = new GameObject("ActorsRoot");
        Transform newActorsRoot = actorsRootObject.transform;
        newActorsRoot.SetParent(transform.parent, false);
        newActorsRoot.SetPositionAndRotation(anchor.position, anchor.rotation);
        newActorsRoot.localScale = Vector3.one;

        if (playerTransform != null && playerTransform.parent == transform)
            playerTransform.SetParent(newActorsRoot, true);

        if (handTransform != null && handTransform.parent == transform)
            handTransform.SetParent(newActorsRoot, true);

        if (interactorTransform != null && interactorTransform.parent == transform)
            interactorTransform.SetParent(newActorsRoot, true);

        return newActorsRoot;
    }

    private void ApplyShipDeltaToActors(Vector3 shipPosition, Quaternion shipRotation)
    {
        if (!useDecoupledActorsFrame)
            return;

        if (!applyShipDeltaToActors || actorsRoot == null)
            return;

        if (actorsRoot.IsChildOf(transform))
            return;

        Quaternion deltaRotation = shipRotation * Quaternion.Inverse(_previousRotation);
        Vector3 deltaPosition = shipPosition - _previousPosition;

        if (deltaPosition.sqrMagnitude < 0.0000001f && Quaternion.Angle(Quaternion.identity, deltaRotation) < 0.001f)
            return;

        Vector3 actorPosition = actorsRoot.position;
        actorPosition = _previousPosition + deltaRotation * (actorPosition - _previousPosition) + deltaPosition;

        actorsRoot.SetPositionAndRotation(actorPosition, deltaRotation * actorsRoot.rotation);
    }

    public void SetMotionIntensity(float intensity) => _motionIntensity = Mathf.Clamp(intensity, 0f, 3f);
}
