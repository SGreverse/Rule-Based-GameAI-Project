using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.HashDataStructers;
using Assets.Algorithm.Map;
using Assets.Algorithm.PriorityQueue;
using Assets.Data.StatScriptables;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Assets.Algorithm.PathFinding
{
    public class PathFinder
    {
        int SEARCHLIMIT = 200; // NEVER check more than 200 tiles per enemy
        float MAXWAITTIME = 3f;
        //to hold onto not yet visited nodes in the specific search
        private GameHashMap<Vector2Int, PathNode> _pathfindingData;

        // maintain object that could be reused in different searches and give less job for the GC.
        private Stack<PathNode> _nodePool;

        public ReservationTable ReservationTable;
        private MapfConfiguration _config;
        public GameMap Map => GameManager.Instance.Map;
        public PathFinder(MapfConfiguration config)
        {
            this._pathfindingData = new GameHashMap<Vector2Int, PathNode>();
            this._nodePool = new Stack<PathNode>();
            this.ReservationTable = new ReservationTable(config);
            this._config = config;
        }
        /// <summary>
        /// fill the pool with every node used from the last search
        /// </summary>
        private void PoorInsidePool()
        {
            foreach (var kvp in _pathfindingData)
            {
                _nodePool.Push(kvp.Value);
            }
            _pathfindingData.Clear();
        }

        /// <summary>
        /// grabs an existing Path node or creates a new one.
        /// using a node pool we can avoid garbage collection every time by reusing nodes from previous path searches
        /// </summary>
        private PathNode GetNodeFromPool(MapNode mapNodeReference)
        {
            PathNode node = _nodePool.Count > 0 ? _nodePool.Pop() : new PathNode(mapNodeReference);

            node.NodeReference = mapNodeReference;
            node.GCost = float.MaxValue;
            node.HCost = 0;
            node.ParentNode = null;
            return node;
        }

        public Stack<PathWaypoint> FindPath(Vector2 startWorldPos, Vector2 targetWorldPos, float agentSpeed, string agentID, IPathingStrategy pathingStrategy, Vector2 playerPos = default, Vector2 playerForward = default)
        {
            Stack<PathWaypoint> finalPath = null;

            //put each node from the last search into the node pool
            PoorInsidePool();



            //find the start index and the end index
            Vector2Int startIndex = this.Map.GetGlobalIndexFromWorldPosition(startWorldPos);
            Vector2Int targetIndex = this.Map.GetGlobalIndexFromWorldPosition(targetWorldPos);


            if (startIndex == targetIndex)
            {
                finalPath = new Stack<PathWaypoint>();
            }
            else
            {
                MapNode targetNode = this.Map.GetNodeDataByGlobalIndex(targetIndex);
                MapNode startMapNode = this.Map.GetNodeDataByGlobalIndex(startIndex);

                // target and start nodes validation
                if (targetNode != null && targetNode.IsWalkable && startMapNode != null)
                {
                    PriorityQueue<PathNode> OpenSet = new PriorityQueue<PathNode>(HeapType.Min);
                    GameHashSet<Vector2Int> closedList = new GameHashSet<Vector2Int>(200);

                    PathNode startPathNode = GetNodeFromPool(startMapNode);
                    startPathNode.GCost = 0;
                    startPathNode.HCost = pathingStrategy.CalculateHCost(startIndex, targetIndex, agentSpeed);
                    startPathNode.ParentNode = null;
                    startPathNode.ArrivalTime = Time.time;

                    OpenSet.Enqueue(startPathNode);
                    this._pathfindingData[startIndex] = startPathNode;

                    int currentSearchCount = 0;
                    bool pathFound = false; 

                    while (OpenSet.Count() > 0 && currentSearchCount <= SEARCHLIMIT && !pathFound)
                    {
                        currentSearchCount++;

                        PathNode currentPathNode = OpenSet.Dequeue();
                        Vector2Int currentIndex = currentPathNode.NodeReference.GridPosition;

                        if (currentIndex == targetIndex)
                        {
                            finalPath = RetracePath(currentPathNode, agentID);
                            pathFound = true;
                        }
                        else
                        {
                            closedList.Add(currentIndex);

                            // evaluate every neighbor of the the current node
                            EvaluateNeighbors(currentPathNode, targetIndex, agentSpeed, closedList, OpenSet, pathingStrategy, playerPos, playerForward);
                        }
                    }
                }
            }

            return finalPath;
        }
        private void EvaluateNeighbors(PathNode currentPathNode, Vector2Int targetIndex, float agentSpeed, GameHashSet<Vector2Int> closedList, PriorityQueue<PathNode> openSet, IPathingStrategy pathingStrategy, Vector2 playerPos, Vector2 playerForward)
        {
            Vector2Int currentIndex = currentPathNode.NodeReference.GridPosition;
            // get all valid neighbors of the current node
            List<MapNode> neighbors = Map.GetNeighbors(currentIndex);

            foreach (MapNode neighbor in neighbors)
            {
                if (!closedList.Contains(neighbor.GridPosition))
                {
                    float moveDistance = Vector2.Distance(currentIndex, neighbor.GridPosition);

                    float nodeWeight = neighbor.WalkWeight;

                    // Calculate Traversal Time (using the formula time=distance(which is the movedistance times the weight) over speed) 
                    float traverseTime = ((moveDistance * nodeWeight) / agentSpeed);

                    //arrival time to the neighbor is the arrival time from the current tile+ the time it takes to walk to the neighbor.
                    float tentativeArrivalTime = currentPathNode.ArrivalTime + traverseTime;

                    // The enemy reserves the tile for the duration it takes to walk across it
                    // departure time is the time in which he will leave the tile
                    // because we dont actually know when we will leave the tile, we use an approiximation by adding again the traverse time.
                    float tentativeDepartureTime = tentativeArrivalTime + traverseTime;

                    float waitTime = 0f;

                    bool isNeighborValid = true;

                    if (!ReservationTable.IsNodeFree(neighbor.GridPosition, tentativeArrivalTime, tentativeDepartureTime))
                    {
                        waitTime = ReservationTable.CalculateWaitTime(neighbor.GridPosition, tentativeArrivalTime, traverseTime, _config.TemporalPadding);

                        if (waitTime > MAXWAITTIME || !ReservationTable.IsNodeFree(currentIndex, currentPathNode.ArrivalTime, currentPathNode.ArrivalTime + waitTime))
                        {
                            isNeighborValid = false;
                        }
                        else
                        {
                            tentativeArrivalTime += waitTime;
                            tentativeDepartureTime += waitTime;
                        }
                    }

                    if (isNeighborValid)
                    {
                        //using strategy pattern to inject the type of cost calculation
                        float extraStrategyCost = pathingStrategy.GetExtraCost(this, neighbor, playerPos, playerForward, agentSpeed);
                        float tentativeGCost = currentPathNode.GCost + traverseTime + waitTime + extraStrategyCost;

                        if (currentPathNode.ParentNode != null && (currentIndex - currentPathNode.ParentNode.NodeReference.GridPosition) != (neighbor.GridPosition - currentIndex))
                        {
                            tentativeGCost += 1f; //we add a Zigzag penalty to force the AI to use a more human-like straight path
                        }
                        
                        //path finding data contains all nodes that have been created
                        bool isAlreadyInOpenSet = _pathfindingData.ContainsKey(neighbor.GridPosition);

                        if (!isAlreadyInOpenSet)
                        {
                            PathNode newNeighborNode = GetNodeFromPool(neighbor);
                            newNeighborNode.HCost = pathingStrategy.CalculateHCost(neighbor.GridPosition, targetIndex, agentSpeed);
                            _pathfindingData[neighbor.GridPosition] = newNeighborNode;
                        }

                        PathNode neighborData = _pathfindingData[neighbor.GridPosition];

                        if (tentativeGCost < neighborData.GCost)
                        {
                            neighborData.GCost = tentativeGCost;
                            neighborData.ParentNode = currentPathNode;
                            neighborData.ArrivalTime = tentativeArrivalTime;

                            if (isAlreadyInOpenSet)
                            {
                                openSet.UpdatePriority(neighborData);
                            }
                            else
                            {
                                openSet.Enqueue(neighborData);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// creates an influence map where walking towards the player is dangerous but walking behind him is safe
        /// </summary>
        public float CalculateFlankGCost(MapNode neighbor, Vector2 playerPos, Vector2 playerForward)
        {
            Vector2 neighborWorldPos = this.Map.GetWorldPositionFromGlobalIndex(neighbor.GridPosition);
            Vector2 dirFromPlayerTonode = (neighborWorldPos - playerPos).normalized;
            float distToPlayer = Vector2.Distance(neighborWorldPos, playerPos);

            // 1 = directly in front ,0 = exactly to the side, -1 = directly behind
            float dotToPlayerForward = Vector2.Dot(playerForward, dirFromPlayerTonode);

            //the distance from which the enemy starts trying to sneak
            float sneakRadius = 6.0f;

            // represention of the maximum "Virtual Seconds" added if they stand right in front of the player
            float maxTimePenalty = 8.0f;

            if (distToPlayer < sneakRadius)
            {
                // The exact directional shape logic you liked
                float proximityDanger = Mathf.Clamp01(dotToPlayerForward + 0.5f);

                // normalized distance to the player. 0 if touching, 1 if at the maximum distance
                float distanceIntensity = (sneakRadius - distToPlayer) / sneakRadius;

                // Output the final Time Penalty
                return maxTimePenalty * distanceIntensity * proximityDanger;
            }

            return 0f;
        }
        public Stack<PathWaypoint> FindPathToOptimalSafePosition(Vector2 StartingWorldPos, Vector2 playerWorldPos, List<Vector2> allyPositions, LayerMask obstacleLayer, float searchRadius, string agentID, float AgentSpeed)
        {
            Stack<PathWaypoint> finalPath = null;

            PoorInsidePool();

            Vector2Int startIndex = this.Map.GetGlobalIndexFromWorldPosition(StartingWorldPos);
            MapNode startMapNode = this.Map.GetNodeDataByGlobalIndex(startIndex);

            if (startMapNode != null)
            {
                PriorityQueue<PathNode> OpenSet = new PriorityQueue<PathNode>(HeapType.Min);
                GameHashSet<Vector2Int> closedList = new GameHashSet<Vector2Int>();

                PathNode startPathNode = GetNodeFromPool(startMapNode);
                startPathNode.GCost = 0;
                startPathNode.ArrivalTime = Time.time;

                OpenSet.Enqueue(startPathNode);
                _pathfindingData[startIndex] = startPathNode;

                PathNode bestDestinationNode = startPathNode;
                float highestDestinationScore = float.MinValue;

                //check if the node is out of the search bounds 
                float sqrSearchRadius = searchRadius * searchRadius;

                // Instantiate the specific strategy for this search type
                IPathingStrategy dangerStrategy = new SafePositionPathingStrategy(allyPositions, obstacleLayer, searchRadius);

                while (OpenSet.Count() > 0)
                {
                    PathNode currentPathNode = OpenSet.Dequeue();
                    Vector2Int currentIndex = currentPathNode.NodeReference.GridPosition;

                    closedList.Add(currentIndex);

                    Vector2 currentWorldPos = this.Map.GetWorldPositionFromGlobalIndex(currentIndex);
                    float destinationScore = CalculateDestinationScore(currentWorldPos, playerWorldPos, allyPositions, obstacleLayer);

                    //we calculate how fitted the node is to be the destination node
                    if (destinationScore > highestDestinationScore)
                    {
                        highestDestinationScore = destinationScore;
                        bestDestinationNode = currentPathNode;
                    }

                    float sqrDistToStart = (currentIndex - startIndex).sqrMagnitude;

                    if (sqrDistToStart <= sqrSearchRadius)
                    {
                        // We pass Vector2Int.zero as a dummy targetIndex, and Vector2.zero as dummy playerForward
                        // because the SafePositionPathingStrategy ignores them entirely
                        EvaluateNeighbors(currentPathNode, Vector2Int.zero, AgentSpeed, closedList, OpenSet, dangerStrategy, playerWorldPos, Vector2.zero);
                    }
                }
                // once we finish evaluating all the nodes, we pick the path to the best one
                finalPath = RetracePath(bestDestinationNode, agentID);
            }

            return finalPath;
        }


        public float CalculateDangerScore(Vector2 testPos, Vector2 playerPos, List<Vector2> allyPositions, LayerMask obstacleLayer, float searchRadius, float agentSpeed)
        {
            float timePenalty = 0f;
            float distancePenalty = 0f;
            // dist from group
            if (allyPositions.Count > 0)
            {
                float avgAllyDist = 0f;
                foreach (Vector2 ally in allyPositions)
                {
                    avgAllyDist += Vector2.Distance(testPos, ally);
                }
                avgAllyDist /= allyPositions.Count;

                distancePenalty += avgAllyDist;//the further the bigger the penalty
            }

            // dist to human player
            Vector2 dirToPlayer = playerPos - testPos;
            float distToPlayer = dirToPlayer.magnitude;

            RaycastHit2D hit = Physics2D.Raycast(testPos, dirToPlayer.normalized, distToPlayer, obstacleLayer);

            // If there is NO wall blocking the view, apply the player virtual seconds penalty
            // If there IS a wall, this block is skipped, completely canceling the player danger
            if (hit.collider == null)
            {
                float exposureDanger = Mathf.Max(0, searchRadius - distToPlayer);
                distancePenalty += exposureDanger;
            }

            timePenalty = distancePenalty / agentSpeed;
            return timePenalty;
        }

        private float CalculateDestinationScore(Vector2 testPos, Vector2 playerPos, List<Vector2> allyPositions, LayerMask obstacleLayer)
        {
            float score = 0f;

            Vector2 dirToPlayer = playerPos - testPos;
            float distToPlayer = dirToPlayer.magnitude;

            // Distance Bonus (Further from player = Better)
            score += distToPlayer;

            // Cohesion Penalty (Too far from allies = Worse)
            if (allyPositions.Count > 0)
            {
                float avgAllyDist = 0f;
                foreach (Vector2 ally in allyPositions) avgAllyDist += Vector2.Distance(testPos, ally);
                avgAllyDist /= allyPositions.Count;

                score -= (avgAllyDist * 0.5f);//for tuning how important the ally distance to the score
            }

            // Line of Sight Bonus (critical for fleeing)
            RaycastHit2D hit = Physics2D.Raycast(testPos, dirToPlayer.normalized, distToPlayer, obstacleLayer);
            if (hit.collider != null)
            {
                //score bonus if we hinde behind a wall
                score += 20.0f;
            }

            return score;
        }
        private Stack<PathWaypoint> RetracePath(PathNode endNode, string agentID)
        {
            Stack<PathWaypoint> path = new Stack<PathWaypoint>();
            PathNode currentNode = endNode;
            float exitTimeForCurrentNode = endNode.ArrivalTime + _config.TimeWindowLimit;

            while (currentNode != null)
            {
                // Create the Waypoint
                PathWaypoint waypoint = new PathWaypoint
                {
                    Position = this.Map.GetWorldPositionFromGlobalIndex(currentNode.NodeReference.GridPosition),
                    ScheduledArrivalTime = currentNode.ArrivalTime
                };
                path.Push(waypoint);

                if (this.ReservationTable != null)
                {
                    this.ReservationTable.ReserveNode(currentNode.NodeReference.GridPosition, currentNode.ArrivalTime, exitTimeForCurrentNode, agentID);
                }

                exitTimeForCurrentNode = currentNode.ArrivalTime;
                currentNode = currentNode.ParentNode;
            }
            path.Pop(); // Remove the start node
            return path;
        }


    }
}
