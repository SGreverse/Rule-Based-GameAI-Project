using Assets.Algorithm.BlackBoard;
using Assets.Script;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProjectileScript : MonoBehaviour
{
    private IDamagable _projectileSender;

    private float _damage;

    [Header("Settings")]
    public float Lifetime = 5f; // Destroys the bullet after 5 seconds so they don't lag the game

    private LayerMask TargetLayer;
    private LayerMask ObstacleLayer;

    //optional
    private LayerMask TeammateLayer;
    private void Start()
    {
        // Always destroy instantiated objects after a set time!
        Destroy(gameObject, Lifetime);
    }

    public void SetupDamage(float damageAmount)
    {
        _damage = damageAmount;
    }
    public void SetupLayers(LayerMask target, LayerMask obstacle, LayerMask teammates = default)
    {
        this.ObstacleLayer = obstacle;
        this.TargetLayer = target;

        this.TeammateLayer = teammates;
    }
    public void SetUpSender(IDamagable DamagingEntity)
    {
        this._projectileSender = DamagingEntity;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Bitwise check: Is the collision layer part of the TargetLayer mask
        if ((TargetLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            IDamagable target = collision.GetComponent<IDamagable>();
            if (target != null)
            {
                float remainingHp = target.TakeDamage(_damage, _projectileSender);

                if (_projectileSender is EnemyManager)
                {
                    GameBlackboard.Instance.WriteData(EnvironmentKey.PlayerHealth, remainingHp);
                }
            }
            Destroy(gameObject);
        }
        // Bitwise check: Is the collision layer part of the ObstacleLayer mask
        else if ((ObstacleLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            // Destroy the projectile if it hits the environment
            Destroy(gameObject);
        }
        // Bitwise check: Is the collision layer part of the EnemyLayer Teammate Mask.
        else if (TeammateLayer!=default && (TeammateLayer.value & (1 << collision.gameObject.layer)) > 0 )
        {
            IDamagable target = collision.GetComponent<IDamagable>();
            if (target != _projectileSender)
            {
                target.TakeDamage(_damage, _projectileSender);

                Destroy(gameObject);
            }

        }
    }
}
