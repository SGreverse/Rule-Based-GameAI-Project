using Assets.Algorithm.BlackBoard;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    private EnemyManager _enemyManager;

    [Header("Melee Settings")]
    public Transform AttackPoint;
    public GameObject MeleeVisualPrefab;
    public float MeleeWidth = 1.5f;

    [Header("Ranged Settings")]
    public GameObject ProjectilePrefab;
    public float ProjectileSpeed = 10f;

    public void Initialize(EnemyManager core)
    {
        this._enemyManager = core;
    }
    public bool MeleeAttack()
    {
        Vector2 direction = _enemyManager.Movment.FacingDirection;
        float attackRange = _enemyManager.Stats.attackRange;

        AttackPoint.localPosition = direction * (0.5f + (attackRange / 2f));

        if (MeleeVisualPrefab != null)
        {
            GameObject visual = Instantiate(MeleeVisualPrefab, AttackPoint.position, Quaternion.identity);
            visual.GetComponent<MeleeHitboxVisualizer>().PlayVisual(attackRange, MeleeWidth, direction);
        }

        Vector2 boxSize = new Vector2(MeleeWidth, attackRange);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        Collider2D[] enemies = Physics2D.OverlapBoxAll(AttackPoint.position, boxSize, angle, _enemyManager.PlayerLayer);
        if (enemies.Length > 0)
        {

            PlayerManager playerManager = enemies[0].GetComponent<PlayerManager>();

            Vector2 playerPos = playerManager.transform.position;
            Vector2 enemyPos = _enemyManager.transform.position;

            Vector2 dirToEnemy = (enemyPos - playerPos).normalized;
            Vector2 playerFacing = playerManager.Movement.FacingDirection.normalized;

            angle = Vector2.Angle(playerFacing, dirToEnemy);
            EnemyLogic brain = _enemyManager.GetBrain();
            float finalDamage = (angle > 90 && angle < 270) ? brain.CalculateStubDamage() : brain.CalculateMeleeDamage();
            float RemainingHealth = playerManager.TakeDamage(finalDamage,_enemyManager);
            Update_Player_Health(RemainingHealth);

            return true;
        }
        else
        {
            return false;
        }
    }
    public void RangedAttack()
    {
        if (ProjectilePrefab == null)
        {
            Debug.LogError("No Projectile Prefab assigned to Enemy!");
            return;
        }


        Transform playerTransform = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition);

        Vector2 shootDirection = (playerTransform.position - this.transform.position).normalized;

        // Calculate the angle, and SUBTRACT 45 degrees because the original sprite is drawn diagonally
        float angle = (Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg) - 45f;
        Quaternion ProejctileRotation = Quaternion.Euler(0, 0, angle);

        // Spawn the arrow with the corrected rotation
        GameObject Proejctile = Instantiate(ProjectilePrefab, this.transform.position, ProejctileRotation);

        //Give the ball physical speed using its Rigidbody2D
        Rigidbody2D rb = Proejctile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * ProjectileSpeed;
        }
        // Pass the Enemy's Damage stat to the ball
        ProjectileScript projScript = Proejctile.GetComponent<ProjectileScript>();
        if (projScript != null)
        {
            // We use the stats as defined in your project to determine how much HP to deduct 
            projScript.SetupDamage(_enemyManager.GetBrain().CalculateProjectileDamage());
            projScript.SetupLayers(_enemyManager.PlayerLayer, _enemyManager.ObstacleLayer,GameManager.Instance.EnemyMask);
            projScript.SetUpSender(_enemyManager);
            this._enemyManager.GetBrain().Shoot();

        }

    }
    public void RaiseShield()
    {
        _enemyManager.GetBrain().SetShieldState(true);
        _enemyManager.SetState(EntityState.Defending);
    }
    public void LowerShield()
    {
        _enemyManager.GetBrain().SetShieldState(false);
        _enemyManager.SetState(EntityState.Free);
    }
    public void Update_Player_Health(float RemainingHealth)
    {
        GameBlackboard.Instance.WriteData(EnvironmentKey.PlayerHealth, RemainingHealth);
    }

}
