using UnityEngine;
using UnityEngine.UI;

public class EnemyShieldController : MonoBehaviour
{
    [Header("Enemy Refrence")]
    [SerializeField] private EnemyManager enemy;

    [Header("View Reference")]
    [SerializeField] private Image ShieldFillImage;



    private void Update()
    {
        ShieldFillImage.fillAmount = enemy.GetBrain().ShieldStamina / enemy.Stats.MaxShieldStamina;
    }
}
