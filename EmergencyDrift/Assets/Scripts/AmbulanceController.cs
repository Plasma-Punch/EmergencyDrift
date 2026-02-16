using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using System.Collections.Generic;
using TMPro;

public class AmbulanceController : MonoBehaviour
{
    [Header("Model")]
    [SerializeField] private GameObject _model;

    [Header("Moving")]
    [SerializeField] private FloatReference _gasInput;
    [SerializeField] private FloatReference _breakInput;
    [SerializeField] private FloatReference _moveSpeed;
    [SerializeField] private FloatReference _maxSpeed;
    [SerializeField] private float _drag = 0.98f;
    [SerializeField] private AudioSource _enginesound;

    [Header("Steering")]
    [SerializeField] private FloatReference _steeringInput;
    [SerializeField] private float _steerAngle = 30f;
    [SerializeField] private float _traction = 2f;
    [SerializeField] private float _driftSpeed = 2f;
    [SerializeField, Range(0,1)] private float _minDriftInput;
    [SerializeField] private AudioSource _driftSound;
    [SerializeField] private List<VisualEffect> _tireSmoke = new List<VisualEffect>();
    [SerializeField] private List<TrailRenderer> _tireMarks = new List<TrailRenderer>();

    [Header("Collision")]
    [SerializeField] private float _bounciness = 5f;
    [SerializeField] private float _collisionMultiplier = 10f;
    [SerializeField] private AudioSource _hitSound;

    [Header("Health")]
    [SerializeField] private FloatReference _health;
    [SerializeField] private GameObject _HealthBarObject;
    [SerializeField] private GameEvent _died;

    [SerializeField] private TextMeshProUGUI _speeedText;

    private Vector3 _moveForce;
    private Slider _healthBar;
    private Rigidbody _rb;

    private bool _canDrift;
    private Quaternion _targetModelLocalRotation = Quaternion.identity;
    private float _lastSteerInput = 0f;

    private void Start()
    {
        _healthBar = _HealthBarObject.GetComponent<Slider>();
        _healthBar.value = _health.value;
        _enginesound.pitch = 0.5f;
        _rb = GetComponent<Rigidbody>();
        // Ensure Rigidbody interpolation is enabled for smooth visuals when physics runs in FixedUpdate
        if (_rb != null)
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        // Keep audio and other non-physics updates on Update for smoothness
        UpdateEngineSound();

        // Smooth visual-only model rotation in Update (interpolated between physics frames)
        if (_model != null)
        {
            _model.transform.localRotation = Quaternion.Slerp(_model.transform.localRotation, _targetModelLocalRotation, Time.deltaTime * _driftSpeed);
        }

        // Handle visual/audio drift effects in Update for responsiveness
        HandleDriftEffects(_lastSteerInput);
    }

    private void FixedUpdate()
    {
        // Perform all physics and movement updates in FixedUpdate to avoid jitter
        // Apply steering first so movement uses the updated rotation
        Steering();
        GasInput();
        DragAndTraction();

        // Apply movement via Rigidbody.velocity so the physics solver integrates motion smoothly
        if (_rb != null)
        {
            Vector3 newVel = _moveForce;
            // Preserve existing vertical velocity (gravity/jumps/collisions)
            newVel.y = _rb.linearVelocity.y;
            _rb.linearVelocity = newVel;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != 7) return;
        // Calculate the current speed magnitude (this is the speed of the ambulance)
        float speed = _moveForce.magnitude;

        // Calculate damage based on speed and the collision multiplier
        float damage = speed * _collisionMultiplier;

        // push player away from collision
        //Vector3 direction = transform.position - collision.transform.position;
        //direction.y = 0;
        //_rb.AddForce(direction * _bounciness, ForceMode.Impulse);


        //Debug.Log(damage);
        // Apply the damage to health
        _health.variable.value -= damage;
        _healthBar.value = _health.value;
        _hitSound.Play();

        //Debug.Log(_health);

        // Check if health falls below zero and handle accordingly
        if (_health.value <= 0)
        {
            Debug.Log("Ambulance Destroyed!");
            _died.Raise();
        }
        else
        {
           Debug.Log("Damage Taken: " + damage + ", Remaining Health: " + _health);
        }
    }

    private void GasInput()
    {
        // Use fixedDeltaTime so movement is framerate-independent
        Vector3 forward = _rb != null ? _rb.rotation * Vector3.forward : transform.forward;
        _moveForce += forward * (_moveSpeed.value * (_gasInput.value - _breakInput.value)) * Time.fixedDeltaTime;

        _speeedText.text = $"{_moveForce.magnitude}";
        _moveForce = Vector3.ClampMagnitude(_moveForce, _maxSpeed.value);
    }

    private void UpdateEngineSound()
    {
        float enginePitch;
        if (_gasInput.value == 0 && _breakInput.value == 0) enginePitch = 0.5f;
        else enginePitch = 0.5f + (_gasInput.value / 2) + (_breakInput.value / 2);
        enginePitch = Mathf.Clamp(enginePitch, 0.5f, 1f);
        _enginesound.pitch = enginePitch;
    }

    private void Steering()
    {
        float steerInput = _steeringInput.value;
        _lastSteerInput = steerInput;
        // Rotate using Rigidbody to keep physics consistent
        float rotationDirection = (_breakInput.value != 0) ? -steerInput : steerInput;
        float yDelta = rotationDirection * _moveForce.magnitude * _steerAngle * Time.fixedDeltaTime;
        _rb.MoveRotation(_rb.rotation * Quaternion.Euler(0f, yDelta, 0f));

        // Drifting
        if (steerInput >= _minDriftInput && _gasInput.value != 0 && _canDrift)
        {
            float driftAngle = Mathf.Clamp(steerInput * 35, -35, 35) * Mathf.Clamp01(_gasInput.value);
            _targetModelLocalRotation = Quaternion.Euler(0, driftAngle, 0);
        }
        else
        {
            // If there's no input, gradually reset the car's tilt
            _targetModelLocalRotation = Quaternion.identity;
        }

    }

    private void DragAndTraction()
    {
        // Drag and max speed limit
        _moveForce *= _drag;

        // Traction
        Vector3 pos = _rb != null ? _rb.position : transform.position;
        Vector3 forward = _rb != null ? _rb.rotation * Vector3.forward : transform.forward;
        Debug.DrawRay(pos, _moveForce.normalized * 3);
        Debug.DrawRay(pos, forward * 3, Color.blue);
        // Use fixedDeltaTime to keep traction calculation in the physics timestep
        _moveForce = Vector3.Lerp(_moveForce.normalized, forward, _traction * Time.fixedDeltaTime) * _moveForce.magnitude;
    }

    private void HandleDriftEffects(float steerInput)
    {
        if (steerInput >= _minDriftInput && _gasInput.value != 0 && _canDrift || steerInput <= -_minDriftInput && _gasInput.value != 0 && _canDrift)
        {
            foreach(VisualEffect smoke in _tireSmoke)
            {
                smoke.Play();
            }
            foreach(TrailRenderer skidMark in _tireMarks)
            {
                skidMark.emitting = true;
            }
            if (!_driftSound.isPlaying) _driftSound.Play();
            _maxSpeed.variable.value = _moveSpeed.value + 5;
        }
        else
        {
            foreach (VisualEffect smoke in _tireSmoke)
            {
                smoke.Stop();
            }
            foreach (TrailRenderer skidMark in _tireMarks)
            {
                skidMark.emitting = false;
            }
            if (_driftSound.isPlaying) _driftSound.Stop();
            _maxSpeed.variable.value = _moveSpeed.value;
        }
    }

    public void EngageDrift(Component sender, object obj)
    {
        _canDrift = true;
    }

    public void DisengageDrift(Component sender, object obj)
    {
        _canDrift = false;
    }
}
