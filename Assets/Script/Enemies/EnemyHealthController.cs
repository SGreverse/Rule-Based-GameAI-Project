using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthController : MonoBehaviour
{
    [Header("Enemy Refrence")]
    [SerializeField] private EnemyManager enemy;

    [Header("View Reference")]
    [SerializeField] private Image healthFillImage;



    private void Update()
    {
         healthFillImage.fillAmount = enemy.GetBrain().CurrentHealth / enemy.Stats.maxHealth;
    }
}
