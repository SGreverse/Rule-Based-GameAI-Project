using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    public PlayerManager playerModel; 
    public Slider healthSlider;     
    public Slider shieldSlider;     
    public TextMeshProUGUI ArrowsCount;
    public TextMeshProUGUI PotionCount;
    public Slider aimChargeSlider;

    public TextMeshProUGUI ArmorName;
    public TextMeshProUGUI SwordName;
    public TextMeshProUGUI BowName;
    void Start()
    {
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        healthSlider.value = playerModel.GetBrain().CurrentHealth;
        shieldSlider.value = playerModel.GetBrain().ShieldStamina;
        ArrowsCount.text = $"{playerModel.GetBrain().ProjectileAmount}";
        PotionCount.text= $"{playerModel.GetBrain().PotionAmount}";
        BowName.text = playerModel.GetBrain().Equipment.EquippedBow.name;
        SwordName.text = playerModel.GetBrain().Equipment.EquippedSword.name;
        ArmorName.text = playerModel.GetBrain().Equipment.EquippedArmor.name;
        if (aimChargeSlider != null)
        {
            // Only show the UI bar if the player is actively aiming
            bool isAiming = playerModel.CurrentState == EntityState.Aiming;
            aimChargeSlider.gameObject.SetActive(isAiming);

            if (isAiming)
            {
                // Fills the bar from 0 to 1 based on how long they've held the button
                aimChargeSlider.value = playerModel.CurrentAimTime / playerModel.GetBrain().GetBowChargeTime();
            }
        }
    }
}
