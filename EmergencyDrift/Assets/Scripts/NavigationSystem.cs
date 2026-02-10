using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NavigationSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject _navigationUI;
    [SerializeField]
    private Image _arrowImage;
    [SerializeField]
    private Image _timerImage;

    private GameObject _target;
    private float _bleedOutTimer;
    private float _maxBleedTimer;
    private GameObject _playerCam;

    public void PatientSpawned(Component sender, object obj)
    {
        SpawnPatientEventArgs args = obj as SpawnPatientEventArgs;
        if (args == null) return;

        _navigationUI.SetActive(true);
        _target = args.SpawnLocation;
        _bleedOutTimer = args.BleedOutRate;
        _maxBleedTimer = args.BleedOutRate;

        _playerCam = Camera.main.gameObject;

        StartCoroutine(UpdateNavigation());
    }

    private IEnumerator UpdateNavigation()
    {
        while(_target != null)
        {
            _bleedOutTimer -= Time.deltaTime;
            _timerImage.fillAmount = _bleedOutTimer / _maxBleedTimer;

            // NEW: Rotate arrow to point at target
            if (_playerCam != null)
            {
                Vector3 directionToTarget = Camera.main.WorldToScreenPoint(_target.transform.position) - Camera.main.WorldToScreenPoint(_playerCam.transform.position);
                float angleToTarget = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

                _arrowImage.transform.rotation = Quaternion.AngleAxis(angleToTarget - 90f, Vector3.forward);
            }

            yield return null;
        }
    }

    public void DisableUI(Component sender, object obj)
    {
        _navigationUI.SetActive(false);
    }
}
