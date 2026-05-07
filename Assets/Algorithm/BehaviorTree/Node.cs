using System;
using System.Collections.Generic;
using Assets.Algorithm.MainAlgorithm;
using Assets.Algorithm.Utility;
using UnityEngine;

public enum NodeState
{
    Success,
    Failure,
    Running,
    None
}
public abstract class Node : IComparable<Node>
{
    private const float INERTIA_BONUS = 1.3f;

    public NodeState CurrentState;

    //a refrence to the enemy the node belongs to
    protected EnemyManager CurrentEnemy;

    //all three are public for debugging purposes
    public List<UtilityFactor> UtilityFactors = new List<UtilityFactor>(); 
    public ResponseCurve PriorityCurve;
    public Func<EnemyManager, float> PriorityFetcher;
    public float CurrentUtility{get;protected set;}

    protected List<ConditionFunc> _preConditions=new List<ConditionFunc>();
    protected Node(EnemyManager CurrentEnemy)
    {
        this.CurrentEnemy = CurrentEnemy;
        this.CurrentState= NodeState.None;
        this.CurrentUtility = 0;
    }
    public virtual bool OnEnter()
    {
        foreach(ConditionFunc condition in _preConditions)
        {
            if (!condition.Invoke(CurrentEnemy))
            {
                return false;
            }
        }
        this.CurrentState = NodeState.Running;
        return true;
    }
    public abstract NodeState Evaluate();
    public virtual void OnExit()
    {
        this.CurrentState = NodeState.None;
    }
    public virtual void CalculateUtility()
    {
        if (UtilityFactors.Count == 0) return;

        float finalUtility = 0;

        
        foreach (UtilityFactor factor in UtilityFactors)
        {
            //fetch the parameter
            float parameterValue = factor.ParameterFetcher.Invoke(this.CurrentEnemy);
            //plot the fetched parameter in his response curve
            finalUtility += factor.Curve.Plot(parameterValue) * factor.Weight;
        }

        //fetch and plot the priority parameter
        if (PriorityCurve != null && PriorityFetcher != null)
        {
            float priorityValue = PriorityFetcher.Invoke(this.CurrentEnemy);
            finalUtility *= PriorityCurve.Plot(priorityValue);
        }


        finalUtility *= (this.CurrentState == NodeState.Running) ? INERTIA_BONUS : 1f;//Apply inertia

        this.CurrentUtility = finalUtility;
    }
    public int CompareTo(Node other)
    {
        return this.CurrentUtility.CompareTo(other.CurrentUtility);
    }
}
