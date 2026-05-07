using System;
using Assets.Algorithm.BlackBoard;
using Assets.Data;
using Assets.SavingSystem;
using Assets.Script;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour,IDamagable
{
    //[SerializeField] private string _persistentID = "";
    public string InstanceID { get; protected set; }

    protected EnemyLogic _brain;

    [Header("Configuration")]
    [SerializeField] public EnemyStats Stats;
    [SerializeField] private TextMeshProUGUI EnemyName; 
    private EntityState CurrentState;
    public RoleType CurrentRole;
    [HideInInspector]
    public EnemyMovement Movment;
    [HideInInspector]
    public EnemyVision Vision;
    [HideInInspector]
    public EnemyCombat Combat;

    [Header("Masks")]
    public LayerMask PlayerLayer;
    public LayerMask ObstacleLayer;


    private Animator _animator;

    private bool _isInitialized = false;

    private float _brainTickTimer = 0f;
    private float _brainTickRate = 0.15f;

    [Header("UI Reference")]
    [Tooltip("Drag the Image component from the World Space Canvas here")]
    public Image iconDisplay;

    [Header("Icon Sprites")]
    public Sprite chargeIcon;
    public Sprite flankIcon;
    public Sprite defendIcon;
    public Sprite fleeIcon;
    public Sprite healIcon;
    public Sprite shootIcon;
    public Sprite patrolIcon;
    public Sprite reloadIcon;
    void Awake()
    {
        InitializeEnemy();

        SuspendEnemy();

        _isInitialized = true;
    }
    public virtual void Update()
    {
        if (this._brain != null)
        {
            _brainTickTimer += Time.deltaTime;

            // Only run the heavy Behavior Tree logic when the timer fills up
            if (_brainTickTimer >= _brainTickRate)
            {
                this._brain.Tick(_brainTickTimer); // Pass the accumulated time
                _brainTickTimer = 0f;
            }
        }
    }
    // Public so e can call it when loading a game save
    public virtual void InitializeEnemy()
    {
        if (_isInitialized) return;
        InstanceID = this.name;

        EnemyName.text = Stats.enemyType;

        this._brain = new EnemyLogic(Stats,this);

        this._brain.OnDeath += Die;

        this._animator = GetComponent<Animator>();

        this.Vision = GetComponent<EnemyVision>();
        this.Movment = GetComponent<EnemyMovement>();
        this.Combat = GetComponent<EnemyCombat>();
        this.Movment.Initialize(this);
        this.Vision.Initialize(this);
        this.Combat.Initialize(this);

        GetComponent<EnemyHealthController>().enabled = true;

        // Add a random offset so all enemies dont tick on the exact same frame
        _brainTickTimer = UnityEngine.Random.Range(0f, _brainTickRate);

    }
    public float TakeDamage(float damage,IDamagable DamagingEntity)
    {
        if (DamagingEntity is PlayerManager player)
        {
            GameBlackboard.Instance.PlayerDetected(player);
            GameBlackboard.Instance.RecordEvent(EnvironmentKey.PlayerAmountOfAttacks, damage);
        }
        return this._brain.TakeDamage(damage);
    }

    public bool Heal()
    {
        return _brain.Heal();
    }
    public EnemyLogic GetBrain()
    {
        return this._brain;
    }
    public void SuspendEnemy()
    {
        if (_animator != null)
        {
            _animator.enabled = false;
        }
        if (_isInitialized) {
            Movment.StopMovment();
        }
        _brain.RoleKick();// we force the enemy to abandon their role since once suspended its irrelevant. when the enemy resumes he will pick a new role
        this.Vision.enabled = false;
        this.enabled = false;

    }
    public void ResumeEnemy()
    {
        this.Vision.enabled = true;
        this.enabled = true;
        if (_animator != null)
        {
            _animator.enabled = true;
        }
    }
    public void ReloadEnemy(EnemyData enemyData)
    {
        if (this._brain == null || !_isInitialized)
        {
            InitializeEnemy();
        }
        this.transform.position = enemyData.Position;
        this._brain.CurrentHealth= enemyData.Health;
        this._brain.ProjectileAmount= enemyData.ProjectileAmount;
        this._brain.PotionAmount = enemyData.PotionAmuont;
    }
    void Die()
    {
        Debug.Log("Enemy Defeated");
        GameBlackboard.Instance.KickEnemy(this);// we remov the enemy from both his role and the active enemy list
        GameManager.Instance.RegisterDeadEnemy(this.InstanceID);
        Destroy(gameObject);
    }

    public void SetState(EntityState newState) => CurrentState = newState;

    public virtual void ShowAction(RoleType newAction)
    {
        iconDisplay.enabled = true;
        CurrentRole = newAction;
        // Swap the picture based on the action chosen
        switch (newAction)
        {
            case RoleType.Charging: iconDisplay.sprite = chargeIcon; break;
            case RoleType.Flanking: iconDisplay.sprite = flankIcon; break;
            case RoleType.Defending: iconDisplay.sprite = defendIcon; break;
            case RoleType.Fleeing: iconDisplay.sprite = fleeIcon; break;
            case RoleType.Healing: iconDisplay.sprite = healIcon; break;
            case RoleType.Shooting: iconDisplay.sprite = shootIcon; break;
            case RoleType.Patroling: iconDisplay.sprite = patrolIcon; break;
            case RoleType.Reloading: iconDisplay.sprite = reloadIcon; break;
        }
    }

}
