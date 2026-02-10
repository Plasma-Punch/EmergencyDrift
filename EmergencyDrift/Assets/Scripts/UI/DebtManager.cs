using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DebtManager : MonoBehaviour
{
    [SerializeField] private float _equipmentPrice;
    [SerializeField] private float _infrastructurePrice;
    [SerializeField] private float _supliesPrice;
    [SerializeField] private float _experimentsPrice;

    [SerializeField] private FloatReference _money;
    [SerializeField] private GameObject _debtUI;
    [SerializeField] private GameEvent _updateMoney;

    [SerializeField] private Button _equipmentButton;
    [SerializeField] private Button _infrastructureButton;
    [SerializeField] private Button _supliesButton;
    [SerializeField] private Button _experimentsButton;

    [SerializeField] private GameEvent _gameWon;
    [SerializeField] private InputActionReference _OpenMenu;
    [SerializeField] private InputActionReference _CloseMenu;

    private TextMeshProUGUI _equipmentText;
    private TextMeshProUGUI _infrastructureText;
    private TextMeshProUGUI _supliesText;
    private TextMeshProUGUI _experimentsText;

    private EventSystem _eventsystem;

    private int _completedLoans;
    private bool _canOpenMenu = true;

    private void Start()
    {
        _equipmentText = _equipmentButton.GetComponentInChildren<TextMeshProUGUI>();
        _infrastructureText = _infrastructureButton.GetComponentInChildren<TextMeshProUGUI>();
        _supliesText = _supliesButton.GetComponentInChildren<TextMeshProUGUI>();
        _experimentsText = _experimentsButton.GetComponentInChildren<TextMeshProUGUI>();

        _equipmentText.text = $"{_equipmentText.text} ${_equipmentPrice}";
        _infrastructureText.text = $"{_infrastructureText.text} ${_infrastructurePrice}";
        _supliesText.text = $"{_supliesText.text} ${_supliesPrice}";
        _experimentsText.text = $"{_experimentsText.text} ${_experimentsPrice}";

        _eventsystem = GameObject.FindObjectOfType<EventSystem>();

        _OpenMenu.action.performed += Open_Menu_Performed;
        _CloseMenu.action.performed += Close_Menu_Performed;
    }

    private void Close_Menu_Performed(InputAction.CallbackContext obj)
    {
        if (!_debtUI.activeSelf) return;

        _debtUI.SetActive(false);
        Time.timeScale = 1f;
        StartCoroutine(AllowOpenMenu());
    }

    private void Open_Menu_Performed(InputAction.CallbackContext obj)
    {
        if (_debtUI.activeSelf || !_canOpenMenu) return;
        _canOpenMenu = false;
        _eventsystem.SetSelectedGameObject(_equipmentButton.gameObject);
        _debtUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void BuyEquipment()
    {
        if (_money.value < _equipmentPrice) return;

        _money.variable.value -= _equipmentPrice;

        _equipmentButton.interactable = false;
        _equipmentButton.image.color = Color.green;
        _equipmentText.text = "Loan settled";
        _updateMoney.Raise();

        _completedLoans += 1;
        if(_completedLoans == 4) _gameWon.Raise();
    }

    public void BuyInfrastructure()
    {
        if (_money.value < _infrastructurePrice) return;

        _money.variable.value -= _infrastructurePrice;

        _infrastructureButton.interactable = false;
        _infrastructureButton.image.color = Color.green;
        _infrastructureText.text = "Loan settled";
        _updateMoney.Raise();

        _completedLoans += 1;
        if (_completedLoans == 4) _gameWon.Raise();
    }

    public void BuySuplies()
    {
        if (_money.value < _supliesPrice) return;

        _money.variable.value -= _supliesPrice;

        _supliesButton.interactable = false;
        _supliesButton.image.color = Color.green;
        _supliesText.text = "Loan settled";
        _updateMoney.Raise();

        _completedLoans += 1;
        if (_completedLoans == 4) _gameWon.Raise();
    }

    public void BuyRND()
    {
        if (_money.value < _experimentsPrice) return;

        _money.variable.value -= _experimentsPrice;

        _experimentsButton.interactable = false;
        _experimentsButton.image.color = Color.green;
        _experimentsText.text = "Loan settled";
        _updateMoney.Raise();

        _completedLoans += 1;
        if (_completedLoans == 4) _gameWon.Raise();
    }

    private IEnumerator AllowOpenMenu()
    {
        yield return new WaitForEndOfFrame();
        _canOpenMenu = true;
    }
}
