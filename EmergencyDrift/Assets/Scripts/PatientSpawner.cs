using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _patientPrefab;
    [SerializeField]
    private int _minBleedOutTime = 10;
    [SerializeField]
    private int _maxBleedOutTime = 25;
    [SerializeField]
    private List<Transform> _spawnLocations = new List<Transform>();
    [SerializeField]
    private GameEvent _patientSpawned;
    [SerializeField]
    private float _spawnDelay = 10;

    private Transform _previousSpawnPoint;

    private int _allowedPatients = 1;
    private int _activePatients = 0;
    private int _spawnedPatients = 0;

    private void Start()
    {
        SpawnPatient();
    }
    private void SpawnPatient()
    {
        Transform spawnPoint = SelectRandomSpawnPoint();
        int bleedTime = GetRandomBleedOutRate();

        GameObject patient = Instantiate(_patientPrefab, spawnPoint);
        patient.GetComponent<Patient>().BleedTime = bleedTime;
        patient.transform.parent = gameObject.transform;
        _patientSpawned.Raise(this, new SpawnPatientEventArgs { BleedOutRate = bleedTime, SpawnLocation = patient });
        _previousSpawnPoint = spawnPoint;
        _activePatients += 1;
        _spawnedPatients += 1;

        if(_spawnedPatients > 5 && _allowedPatients < _spawnLocations.Count)
        {
            _spawnedPatients = 0;
            _allowedPatients += 1;
        }

        StartCoroutine(SpawnCooldown());
    }

    private Transform SelectRandomSpawnPoint()
    {
        int randomIndex = Random.Range(0, _spawnLocations.Count);
        Transform spawnPoint = _spawnLocations[randomIndex];

        while(spawnPoint == _previousSpawnPoint)
        {
            randomIndex = Random.Range(0, _spawnLocations.Count);
            spawnPoint = _spawnLocations[randomIndex];
        }

        return spawnPoint;
    }

    private int GetRandomBleedOutRate()
    {
        int randomBleedTime = Random.Range(_minBleedOutTime, _maxBleedOutTime + 1);
        return randomBleedTime;
    }

    private IEnumerator SpawnCooldown()
    {
        yield return new WaitForSeconds(_spawnDelay);
        if(_activePatients < _allowedPatients) SpawnPatient();
        else StartCoroutine(SpawnCooldown());
    }

    public void PatientPickedUp(Component sender, object obj)
    {
        _activePatients--;
    }
}
