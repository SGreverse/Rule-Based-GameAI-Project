using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDebugScript : MonoBehaviour
{
    [Tooltip("Drag the Player GameObject here")]
    [SerializeField] private PlayerManager _player;

    [SerializeField] private Slider HealthBar;
    [SerializeField] private Slider ShieldBar;
    private void Start()
    {
        HealthBar.maxValue = 100;
        ShieldBar.maxValue = 100;
    }
    void Update()
    {
       

        if (HealthBar.value != _player.GetBrain().CurrentHealth)
        {
            HealthBar.value = _player.GetBrain().CurrentHealth;
        }

        if (ShieldBar.value != _player.GetBrain().ShieldStamina)
        {
            ShieldBar.value = _player.GetBrain().ShieldStamina;
        }
    }
}
