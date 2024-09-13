using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HUDController : Observer
{
    private float _currentHealth;
    private PlayerMove _playerMove;
    public Text HealthUI;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void Notify(Subject subject)
    {
        if (!_playerMove)
            _playerMove =
                subject.GetComponent<PlayerMove>();

        if (_playerMove)
        {
            _currentHealth =
                _playerMove.curHealth;
            HealthUI.text = "Health: " + _currentHealth;
        }
    }
}
