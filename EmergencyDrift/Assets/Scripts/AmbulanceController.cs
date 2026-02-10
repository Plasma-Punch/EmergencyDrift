using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using System.Collections.Generic;

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

    private Vector3 _moveForce;
    private Slider _healthBar;
    private Rigidbody _rb;

    private bool _canDrift;

    private void Start()
    {
        _healthBar = _HealthBarObject.GetComponent<Slider>();
        _healthBar.value = _health.value;
        _enginesound.pitch = 0.5f;
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        GasInput();
        DragAndTraction();
    }

    private void FixedUpdate()
    {
        Steering();
        _rb.Move(transform.position + _moveForce * Time.deltaTime, transform.rotation);
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
        _moveForce += transform.forward * (_moveSpeed.value * (_gasInput.value - _breakInput.value));

        _moveForce = Vector3.ClampMagnitude(_moveForce, _maxSpeed.value);

        float enginePitch;
        if (_gasInput.value == 0 && _breakInput.value == 0) enginePitch = 0.5f;
        else enginePitch = 0.5f + (_gasInput.value / 2) + (_breakInput.value / 2);
        enginePitch = Mathf.Clamp(enginePitch, 0.5f, 1f);
        _enginesound.pitch = enginePitch;
    }

    private void Steering()
    {
        float steerInput = _steeringInput.value;
        if(_breakInput.value != 0) transform.Rotate(Vector3.up * -steerInput * _moveForce.magnitude * _steerAngle * Time.deltaTime);
        else transform.Rotate(Vector3.up * steerInput * _moveForce.magnitude * _steerAngle * Time.deltaTime);

        // Drifting
        if (steerInput != 0 && _gasInput.value != 0 && _canDrift)
        {
            float driftAngle = Mathf.Clamp(steerInput * 45, -45, 45) * Mathf.Clamp01(_gasInput.value);
            Quaternion targetRotation = Quaternion.Euler(0, driftAngle, 0);

            _model.transform.localRotation = Quaternion.Slerp(_model.transform.localRotation, targetRotation, Time.deltaTime * _driftSpeed);
        }
        else
        {
            // If there's no input, gradually reset the car's tilt
            Quaternion straightRotation = Quaternion.Euler(0, 0, 0);
            _model.transform.localRotation = Quaternion.Slerp(_model.transform.localRotation, straightRotation, Time.deltaTime * _driftSpeed);
        }

        HandleDriftEffects(steerInput);
    }

    private void DragAndTraction()
    {
        // Drag and max speed limit
        _moveForce *= _drag;

        // Traction
        Debug.DrawRay(transform.position, _moveForce.normalized * 3);
        Debug.DrawRay(transform.position, transform.forward * 3, Color.blue);
        _moveForce = Vector3.Lerp(_moveForce.normalized, transform.forward, _traction * Time.deltaTime) * _moveForce.magnitude;
    }

    private void HandleDriftEffects(float steerInput)
    {
        if (steerInput >= 0.35f && _gasInput.value != 0 && _canDrift || steerInput <= -0.35f && _gasInput.value != 0 && _canDrift)
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
