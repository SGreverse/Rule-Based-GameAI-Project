using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.PathFinding;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private const float STOPPING_DISTANCE = 0.1f;

    public float SpeedModifier { get; set; } = 1.0f;
    public Vector2 FacingDirection { get; private set; } = Vector2.down;

    public Vector2 TargetPosition;
    private Stack<PathWaypoint> _pathStack;
    private PathWaypoint _currentWaypoint;
    public bool HasTarget;

    private EnemyManager _manager;

    private Rigidbody2D _rb;
    private Animator _animator;

    private float _movmentSpeed;

    //for Debugging
    private EnemyPathVisualizer _pathVisualizer;
    public void Initialize(EnemyManager core)
    {
        this._manager = core;
        this._movmentSpeed = core.Stats.moveSpeed;
        this._rb= GetComponent<Rigidbody2D>();
        this._animator = GetComponent<Animator>();      

        this._pathStack= new Stack<PathWaypoint>();
        
        this._pathVisualizer= GetComponent<EnemyPathVisualizer>();

    }

    public void RotateToFace(Vector2 targetPosition)
    {
        Vector2 currentPosition = transform.position;
        Vector2 directionToTarget = targetPosition - currentPosition;

        FacingDirection = directionToTarget.normalized;

        UpdateAnimation(FacingDirection,false);
    }
    public bool FindSafeSpot()
    {
        StopMovment();

        if (GameBlackboard.Instance != null && GameBlackboard.Instance.GlobalPathFinder.ReservationTable != null)
        {
            GameBlackboard.Instance.GlobalPathFinder.ReservationTable.ClearReservationsForAgent(this._manager.InstanceID);
        }

        List<Vector2> ally_positions = GameBlackboard.Instance.GetAlliesPositions(_manager);
        Stack<PathWaypoint> path = GameBlackboard.Instance.GlobalPathFinder.FindPathToOptimalSafePosition(
            _rb.position,
            GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition).position,
            ally_positions,
            _manager.ObstacleLayer,
            this._manager.Stats.viewRadius,
            this._manager.InstanceID,
            this._manager.Stats.moveSpeed
        );
        if (path != null && path.Count > 0)
        {
            this._pathStack = path;
            this._currentWaypoint = path.Pop();
            this.HasTarget = true;
            return true;
        }
        return false;
    }
    public bool SetTarget(Vector2 target)
    {
        StopMovment();

        if (!GameManager.Instance.Map.IsTileEmpty(target))
        {
            return false;
        }
        if (GameBlackboard.Instance.GlobalPathFinder.ReservationTable != null)
        {
            GameBlackboard.Instance.GlobalPathFinder.ReservationTable.ClearReservationsForAgent(this._manager.InstanceID);
        }
        Stack<PathWaypoint> path = GameBlackboard.Instance.GlobalPathFinder.FindPath(
            _rb.position, 
            target,_manager.Stats.moveSpeed,
            this._manager.InstanceID,
            new NormalStrategy());

        if (path != null && path.Count > 0)
        {
            this.TargetPosition = target;
            this.HasTarget = true;
            this._pathStack = path;
            this._currentWaypoint = path.Pop();
            return true;
        }
        return false;
    }
    public bool SetStealthTarget(Vector2 target, Vector2 playerPos)
    {
        StopMovment();

        if (!GameManager.Instance.Map.IsTileEmpty(target)) return false;

        if (GameBlackboard.Instance.GlobalPathFinder.ReservationTable != null)
        {
            GameBlackboard.Instance.GlobalPathFinder.ReservationTable.ClearReservationsForAgent(this._manager.InstanceID);
        }

        Vector2 playerForward =(Vector2) GameBlackboard.Instance.ReadLastRecordedEvent(EnvironmentKey.PlayerDirectionChange).Value;


        Stack<PathWaypoint> path = GameBlackboard.Instance.GlobalPathFinder.FindPath(
            _rb.position,
            target,
            _manager.Stats.moveSpeed,
            this._manager.InstanceID,
            new FlankingStrategy(),
            playerPos,
            playerForward
        );
        if (path != null && path.Count > 0)
        {
            this.TargetPosition = target;
            this.HasTarget = true;
            this._pathStack = path;
            this._currentWaypoint = path.Pop();
            return true;
        }
        return false;

    }
    public bool FindRandomTileInDirection(Vector2 fromPos,Vector2 direction, float degreesRight, float degreesLeft,float minTiles,float maxTiles, out Vector2 resultPos, bool considerWalls = false)
    {
        StopMovment();

        if (GameBlackboard.Instance.GlobalPathFinder.ReservationTable != null)
        {
            GameBlackboard.Instance.GlobalPathFinder.ReservationTable.ClearReservationsForAgent(this._manager.InstanceID);
        }

        float tileSize = GameManager.Instance.tileSize;
        int maxAttempts = 10; // Prevent infinite loops if trapped

        Vector2 baseDir = direction.normalized;

        for (int i = 0; i < maxAttempts; i++)
        {
            // Pick a random angle within your cone.
            float randomAngleOffset = UnityEngine.Random.Range(-degreesRight, degreesLeft);

            //Rotate the base direction by the random angle
            Vector3 rotatedDir = Quaternion.Euler(0, 0, randomAngleOffset) * baseDir;
            Vector2 finalDirection = new Vector2(rotatedDir.x, rotatedDir.y);

            //Pick a random distance
            float randomTileDist = UnityEngine.Random.Range(minTiles, maxTiles);
            float realWorldDist = randomTileDist * tileSize;

            //Calculate the exact world coordinate
            Vector2 testPos = fromPos + (finalDirection * realWorldDist);

            // Check if this specific spot is a walkable floor tile 
            if (considerWalls)
            {
                if (Physics2D.Raycast(fromPos,finalDirection,realWorldDist,_manager.ObstacleLayer))
                {
                    resultPos = Vector2.zero;
                    return false;
                }
            }
            if (GameManager.Instance.Map.IsTileEmpty(testPos))
            {
                resultPos = testPos; 
                return true;        
            }
        }

        // If we tried 10 times and failed
        resultPos = fromPos; // Just return where they are currently standing
        return false;
    }
    public bool IsMovmentFinished()
    {
        return !this.HasTarget;
    }
    public void StopMovment()
    {
        this.HasTarget= false;
        this._pathStack.Clear();
        this.TargetPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        if ( GameBlackboard.Instance.GlobalPathFinder.ReservationTable != null)
        {
            GameBlackboard.Instance.GlobalPathFinder.ReservationTable.ClearReservationsForAgent(this._manager.InstanceID);

            //reserve the current tile the enemy stands on for a 5 seconds window
            Vector2Int currentGridPos = GameManager.Instance.Map.GetGlobalIndexFromWorldPosition(_rb.position);
            float currentTime = Time.time;

            GameBlackboard.Instance.GlobalPathFinder.ReservationTable.ReserveNode(currentGridPos, currentTime, currentTime + 15.0f, this._manager.InstanceID);
        }

        UpdateAnimation(FacingDirection, false);
    }
    private void FixedUpdate()
    {
        _rb.linearVelocity = Vector2.zero;

        _pathVisualizer.DrawPath(transform.position, _pathStack);
        if (!this.HasTarget)
        {
            return;
        }

        Vector2 currentPos = _rb.position;
        float distanceToWaypoint = Vector2.Distance(currentPos, _currentWaypoint.Position);

        if (distanceToWaypoint > 0.1f)
        {
            //  cbeck where the path want us to go
            Vector2 pathDirection = (_currentWaypoint.Position - currentPos).normalized;

            // soft avoidance of allies
            Vector2 separationForce = Vector2.zero;
            float separationRadius = 1.0f; // Soft radius for gentle pushing
            int neighborsInRadius = 0;

            foreach (EnemyManager ally in GameBlackboard.Instance.ActiveEnemies)
            {
                if (ally == this._manager) continue;

                Vector2 allyPos = ally.transform.position;
                Vector2 diff = currentPos - allyPos;
                float dist = diff.magnitude;

                // Only care about allies inside our  radius
                if (dist > 0.01f && dist < separationRadius)
                {
                    // Inversely Proportional Force: Stronger when very close, weak at the edge
                    float pushStrength = (separationRadius - dist) / separationRadius;

                    // Add the push force away from this specific ally
                    separationForce += diff.normalized * pushStrength;
                    neighborsInRadius++;
                }
            }

            // Average out the separation force if surrounded by multiple allies
            if (neighborsInRadius > 0)
            {
                separationForce /= neighborsInRadius;
            }

            // Combine where we want to go with where we dont want to go
            float separationWeight = 1.5f; // Higher = avoid allies more aggressively
            Vector2 desiredDirection = (pathDirection + (separationForce * separationWeight)).normalized;

          
            FacingDirection = desiredDirection;

            // move
            float currentSpeed = this._movmentSpeed * SpeedModifier;
            Vector2 travelDistance = FacingDirection * currentSpeed * Time.fixedDeltaTime;

            _rb.MovePosition(currentPos + travelDistance);
            UpdateAnimation(FacingDirection, true);
        }
        // if we reached the tile,we check if we should wait or go to the next one
        else
        {
            if (_pathStack.Count > 0)
            {
                PathWaypoint nextWaypoint = _pathStack.Peek();

                float distanceToNext = Vector2.Distance(currentPos, nextWaypoint.Position);
                float currentSpeed = this._movmentSpeed * SpeedModifier;
                float traverseTime = distanceToNext / currentSpeed;

                // When do we need to leave our current tile
                float scheduledDepartureTime = nextWaypoint.ScheduledArrivalTime - traverseTime;

                //check if we are ready to leave the tile yet
                // Add a tiny buffer to prevent floating point stutter
                if (Time.time < scheduledDepartureTime - 0.1f)
                {
                    UpdateAnimation(FacingDirection, false);
                    return;
                }

                _currentWaypoint = _pathStack.Pop();
            }
            else
            {
                HasTarget = false;
                UpdateAnimation(FacingDirection, false);
            }
        }
    }
    public void UpdateAnimation(Vector2 dir, bool walking)
    {
        if (walking)
        {
            FacingDirection = dir;
            _animator.SetFloat("speed", 1);
        }
        else
        {
            _animator.SetFloat("speed", 0);

        }
        _animator.SetFloat("Horizontal", dir.x);
        _animator.SetFloat("Vertical", dir.y);
    }
   
}
