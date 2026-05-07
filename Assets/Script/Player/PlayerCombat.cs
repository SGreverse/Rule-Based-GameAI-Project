using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerManager _playerManager;

    [Header("Melee Combat")]
    public Transform AttackPoint;
    public LayerMask enemyLayers;
    public GameObject MeleeVisualPrefab; 

    [Header("Ranged Combat")]
    public GameObject arrowPrefab;
    public LayerMask obstacleLayer;

    public void Initialize(PlayerManager core) => this._playerManager = core;


    public void ExecuteAttack()
    {
        StartCoroutine(AttackCoroutine());
    }
    IEnumerator AttackCoroutine()
    {
        _playerManager.SetState(EntityState.Attacking);
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            //put the attack position in the same direction the player is facing
            Vector2 direction = _playerManager.Movement.FacingDirection;
            AttackPoint.localPosition = direction;

            if (MeleeVisualPrefab != null)
            {
                GameObject visual = Instantiate(MeleeVisualPrefab, AttackPoint.position, Quaternion.identity);
                visual.GetComponent<MeleeHitboxVisualizer>().PlayVisual(_playerManager.Stats.attackRange, _playerManager.Stats.attackWidth, direction);
            }

            yield return new WaitForSeconds(1f);


            Vector2 boxSize = new Vector2(_playerManager.Stats.attackWidth, _playerManager.Stats.attackRange);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            Collider2D[] enemies = Physics2D.OverlapBoxAll(AttackPoint.position, boxSize, angle, enemyLayers);

            foreach (Collider2D enemy in enemies)
            {
                // if he's not hitting him through the wall
                if (!Physics2D.Linecast(transform.position, enemy.transform.position, obstacleLayer))
                {
                    EnemyManager enemyController = enemy.GetComponent<EnemyManager>();
                    if (enemyController != null)
                    {
                        Vector2 playerPos = this.transform.position;
                        Vector2 enemyPos = enemyController.transform.position;

                        Vector2 dirToEnemy = (enemyPos - playerPos).normalized;

                        angle = Vector2.Angle(_playerManager.Movement.FacingDirection, dirToEnemy);
                        EnemyLogic brain = enemyController.GetBrain();
                        float finalDamage = (angle > 90 && angle < 270) ? brain.CalculateStubDamage() : brain.CalculateMeleeDamage();
                        enemyController.TakeDamage(finalDamage, _playerManager);
                    }
                }
            }
        }
        _playerManager.SetState(EntityState.Free);
    }
    public void ShootArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("No Arrow Prefab assigned to Player!");
            return;
        }

        //  Get the direction the player is currently facing
        Vector2 shootDirection = _playerManager.Movement.FacingDirection;

        // Calculate the angle and subtract 45 for your specific arrow sprite
        float angle = (Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg) - 45f;
        Quaternion arrowRotation = Quaternion.Euler(0, 0, angle);

        // Spawn the arrow at the AttackPoint (so it comes out of the player's hands)
        GameObject arrow = Instantiate(arrowPrefab, transform.position, arrowRotation);

        // Add velocity
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * _playerManager.Stats.arrowSpeed;
        }

        // Setup the damage and layers
        ProjectileScript projScript = arrow.GetComponent<ProjectileScript>();
        if (projScript != null)
        {
            _playerManager.GetBrain().Shoot();

            projScript.SetupDamage(_playerManager.GetBrain().CalculateProjectileDamage());
            projScript.SetUpSender(_playerManager);
            projScript.SetupLayers(enemyLayers, obstacleLayer);
        }

    }
    public void RaiseShield()
    {
        Debug.Log("Raising Shield");
        _playerManager.GetBrain().SetShieldState(true);
        _playerManager.SetState(EntityState.Defending);
    }
    public void LowerShield()
    {
        Debug.Log("Lowering Shield");
        _playerManager.GetBrain().SetShieldState(false);
        _playerManager.SetState(EntityState.Free);
    }
    public void Heal()
    {
        this._playerManager.GetBrain().Heal();
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (_playerManager != null)
        {
            Vector2 boxSize = new Vector2(_playerManager.Stats.attackWidth, _playerManager.Stats.attackRange);
            if (AttackPoint != null) Gizmos.DrawCube(AttackPoint.position, boxSize);
        }
    }

}

