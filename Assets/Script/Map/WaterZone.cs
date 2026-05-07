using UnityEngine;

public class WaterZone : MonoBehaviour
{
    private const float WATER_SPEED_PENALTY = 0.5f; // Half speed

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Slow down the player
        PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SpeedModifier = WATER_SPEED_PENALTY;
        }

        // Slow down the enemies
        EnemyMovement enemyMovement = collision.GetComponent<EnemyMovement>();
        if (enemyMovement != null)
        {
            enemyMovement.SpeedModifier = WATER_SPEED_PENALTY;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Restore player speed
        PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SpeedModifier = 1.0f;
        }

        // Restore enemy speed
        EnemyMovement enemyMovement = collision.GetComponent<EnemyMovement>();
        if (enemyMovement != null)
        {
            enemyMovement.SpeedModifier = 1.0f;
        }
    }
}
