using System.Collections;
using Assets.Algorithm.BlackBoard;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    private EnemyManager _enemy;

    public Coroutine FindTargetCoroutine;

    private float _viewRadius;
    private float _viewAngle;

    private Transform _cachedPlayerTransform;
    public void Initialize(EnemyManager core)
    {
        this._enemy = core;
        this._viewAngle = core.Stats.viewAngle;
        this._viewRadius = core.Stats.viewRadius;

        float randomOffset = Random.Range(0f, 0.15f);
        FindTargetCoroutine = StartCoroutine(VisionTick(0.15f, randomOffset));
    }

    //resume the vision coroutine
    private void OnEnable()
    {
        if (_enemy != null)
        {
            float randomOffset = Random.Range(0f, 0.15f);
            FindTargetCoroutine = StartCoroutine(VisionTick(0.15f, randomOffset));
        }
    }

    // stop the vision coroutine
    private void OnDisable()
    {
        if (FindTargetCoroutine != null)
        {
            StopCoroutine(FindTargetCoroutine);
            FindTargetCoroutine = null;
        }
    }

    IEnumerator VisionTick(float delay, float startOffset)
    {
        // Wait for the random offset before starting the loop
        yield return new WaitForSeconds(startOffset);

        while (true)
        {
            // Calculate Tactical Exposure is the player position is known
            if (GameBlackboard.Instance.ReadData<bool>(EnvironmentKey.PlayerDetected))
            {
                if (_cachedPlayerTransform == null)
                {
                    _cachedPlayerTransform = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition);
                }
                CalculateTacticalPlayerExposure();
            }

            // find the visible targets
            FindVisibleTargets();

            // Wait for the next tick
            yield return new WaitForSeconds(delay);
        }
    }
    private void CalculateTacticalPlayerExposure()
    {

        Transform playerTransform = _cachedPlayerTransform;
        if (playerTransform == null) return;

        Vector2 playerPos = playerTransform.position;
        Vector2 enemyPos = this.transform.position;
        Vector2 dirToPlayer = (playerPos - enemyPos).normalized;
        float distance = Vector2.Distance(enemyPos, playerPos);

        Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();

        float playerWidth = playerCollider != null ? playerCollider.bounds.extents.x : 0.5f;

        Vector2 perpendicularDir = new Vector2(-dirToPlayer.y, dirToPlayer.x);

        Vector2 centerPoint = playerPos;
        Vector2 leftPoint = playerPos + (perpendicularDir * playerWidth);
        Vector2 rightPoint = playerPos - (perpendicularDir * playerWidth);

        int visiblePoints = 0;
        int totalPoints = 3;

        if (!Physics2D.Raycast(enemyPos, (centerPoint - enemyPos).normalized, distance, _enemy.ObstacleLayer))
            visiblePoints++;

        if (!Physics2D.Raycast(enemyPos, (leftPoint - enemyPos).normalized, Vector2.Distance(enemyPos, leftPoint), _enemy.ObstacleLayer))
            visiblePoints++;

        if (!Physics2D.Raycast(enemyPos, (rightPoint - enemyPos).normalized, Vector2.Distance(enemyPos, rightPoint), _enemy.ObstacleLayer))
            visiblePoints++;

        _enemy.GetBrain().CurrentPlayerExposure = (float)visiblePoints / totalPoints;
    }
    void FindVisibleTargets()
    {
        // check for targets within radius
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, this._viewRadius, _enemy.PlayerLayer);


        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector2 dirToTarget = (target.position - transform.position).normalized;

            // check if target is within the vision angle
            if (Vector2.Angle(this._enemy.Movment.FacingDirection, dirToTarget) < this._viewAngle / 2)
            {
                Collider2D playerCollider = target.GetComponent<Collider2D>();
                float playerWidth = playerCollider.bounds.extents.x; // Half the width of the player

                // Calculate the perpendicular vector to find the left and right edges
                Vector2 perpendicularDir = new Vector2(-dirToTarget.y, dirToTarget.x);

                Vector2 centerPoint = target.position;
                Vector2 leftPoint = (Vector2)target.position + (perpendicularDir * playerWidth);
                Vector2 rightPoint = (Vector2)target.position - (perpendicularDir * playerWidth);

                int visiblePoints = 0;
                int totalPoints = 3;

                // cast rays to all 3 points
                if (!Physics2D.Raycast(transform.position, (centerPoint - (Vector2)transform.position).normalized, Vector2.Distance(transform.position, centerPoint), _enemy.ObstacleLayer))
                    visiblePoints++;

                if (!Physics2D.Raycast(transform.position, (leftPoint - (Vector2)transform.position).normalized, Vector2.Distance(transform.position, leftPoint), _enemy.ObstacleLayer))
                    visiblePoints++;

                if (!Physics2D.Raycast(transform.position, (rightPoint - (Vector2)transform.position).normalized, Vector2.Distance(transform.position, rightPoint), _enemy.ObstacleLayer))
                    visiblePoints++;

                float exposureLevel = (float)visiblePoints / totalPoints;

                _enemy.GetBrain().CurrentPlayerExposure = exposureLevel;

                // if at least one point is visible, the player is seen 
                if (visiblePoints > 0)
                {
                    PlayerManager player = target.GetComponent<PlayerManager>();
                    GameBlackboard.Instance.PlayerDetected(player);
                }
            }
        }



    }
    private void OnDrawGizmos()
    {
        if (_enemy == null || _enemy.Movment == null) return;

        Vector2 facing = _enemy.Movment.FacingDirection;

        // Convert Vector2 direction to an angle in degrees for the Gizmo
        // Mathf.Atan2 returns radians; we convert to degrees. 
        // We subtract 90 because Unity's 0 degrees is "Up" (North) in your Trig logic.
        float facingAngle = Mathf.Atan2(facing.x, facing.y) * Mathf.Rad2Deg;

        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, this._viewRadius);

        Vector3 leftBoundary = DirFromAngle(facingAngle - this._viewAngle / 2, true);
        Vector3 rightBoundary = DirFromAngle(facingAngle + this._viewAngle / 2, true);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * this._viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * this._viewRadius);
    }

    // Simplified helper for standard vector conversion
    public Vector3 DirFromAngle(float angleInDegrees, bool isGlobalAngle)
    {
        // convert to Radians
        float rad = angleInDegrees * Mathf.Deg2Rad;
        // Standard 2D coordinate system where 0 is Up
        return new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0);
    }
    private void OnDestroy()
    {
        if (FindTargetCoroutine != null)
        {
            StopCoroutine(FindTargetCoroutine);

        }
    }
}

