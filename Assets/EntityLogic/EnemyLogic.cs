using System;
using System.Runtime.Serialization;
using Assets.Algorithm.BehaviorTree.Selectors;
using Assets.Algorithm.MainAlgorithm;
using Assets.Data;
using Assets.EntityLogic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class EnemyLogic : EntityLogic
{
    private EnemyStats stats;

    protected Node brain_root;
    private float _tick_time;

    public float TimeSinceLastHit;
    public float CurrentPlayerExposure { get; set; } = 0f;

    public EnemyLogic(EnemyStats stats, EnemyManager currentEnemy) : base(stats)
    {
        brain_root = new RootSelector(currentEnemy);
    }
    public override float TakeDamage(float rawDamage)
    {
        TimeSinceLastHit = 0;
        float RemainingHealth= base.TakeDamage(rawDamage);
        return RemainingHealth;
    }
    public Node GetRootNode()
    {
        return brain_root;
    }
    public override void Tick(float delta)
    {
        //if we arent in the middle of a run, prepare the node for entry
        if (brain_root.CurrentState != NodeState.Running)
        {
            brain_root.OnEnter();
        }


        NodeState current_state = brain_root.Evaluate();


        TimeSinceLastHit += delta;
        base.Tick(delta);

    }
    public void RoleKick()
    {
        brain_root.OnExit();//force an exit to the current action and re-evaluate
    }




}
