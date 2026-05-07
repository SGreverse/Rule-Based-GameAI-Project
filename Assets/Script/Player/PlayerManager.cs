using System.Collections;
using Assets.Algorithm.BlackBoard;
using Assets.Data;
using Assets.EntityLogic;
using Assets.SavingSystem;
using Assets.Script;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public enum EntityState { Free, Attacking, Defending, Stunned, Aiming }
public class PlayerManager : MonoBehaviour,IDamagable
{
    public EntityState CurrentState { get; private set; } = EntityState.Free;

    //  Dependencies
    public PlayerMovement Movement { get; private set; }
    public PlayerCombat Combat { get; private set; }

    //Input References 
    private PlayerInput _input;
    private InputAction _moveAction;
    private InputAction _attackAction;
    private InputAction _defendAction;
    private InputAction _healAction;
    private InputAction _shootAction; 

    [Header("Configuration")]
    [SerializeField] public PlayerStats Stats;
    [SerializeField] private ItemData starterSword;
    [SerializeField] private ItemData starterBow;
    [SerializeField] private ItemData starterArmor;


    [HideInInspector]
    public float CurrentAimTime { get; private set; } = 0f;

    // Brain\Logic Manager

    private PlayerLogic _brain;
    void Awake()
    {
        Movement = GetComponent<PlayerMovement>();
        Combat = GetComponent<PlayerCombat>();
        _input = GetComponent<PlayerInput>();

        Movement.Initialize(this);
        Combat.Initialize(this);

        _moveAction = _input.actions["Move"];
        _attackAction = _input.actions["Attack"];
        _defendAction = _input.actions["Defend"];
        _healAction = _input.actions["Heal"];
        _shootAction= _input.actions["Shoot"];
        _brain = new PlayerLogic(Stats,starterSword,starterBow,starterArmor);
        _brain.OnDeath += Die;
        _brain.ShieldBreakEvent += Combat.LowerShield;
    }

    void Update()
    {
        _brain.Tick(Time.deltaTime);
        //Handle Defending
        if (CurrentState == EntityState.Defending)
        {
            Movement.SetInputVector(Vector2.zero);
            if (!_defendAction.IsPressed()) Combat.LowerShield();
            return;
        }
        // Handle Aiming 
        else if (CurrentState == EntityState.Aiming)
        {
            Movement.SetInputVector(Vector2.zero); // Stop moving while aiming

            // Fill the charge bar
            CurrentAimTime += Time.deltaTime;
            float MaxAimTime = _brain.GetBowChargeTime();
            CurrentAimTime = Mathf.Clamp(CurrentAimTime, 0, MaxAimTime);

            // If the player lets go of the key...
            if (!_shootAction.IsPressed())
            {
                // Only shoot if the bar is completely full
                if (CurrentAimTime >= MaxAimTime)
                {
                    Combat.ShootArrow();
                }

                // Reset and go back to free state
                CurrentAimTime = 0f;
                SetState(EntityState.Free);
            }
            return;
        }
        // 3. Handle Stunned/Attacking lockouts
        else if (CurrentState != EntityState.Free)
        {
            Movement.SetInputVector(Vector2.zero);
            return;
        }

        // Read Movement
        Vector2 input = _moveAction.ReadValue<Vector2>();
        Movement.SetInputVector(input);


        if (_defendAction.IsPressed() && !_brain.IsShieldBroken)
        {
            Combat.RaiseShield();
        }
        else if (_attackAction.triggered)
        {
            Combat.ExecuteAttack();
        }
        else if (_healAction.triggered)
        {
            Combat.Heal();
        }
        else if (_shootAction.IsPressed() && _brain.ProjectileAmount > 0)
        {
            SetState(EntityState.Aiming);
            CurrentAimTime = 0f;
        }
    }

    public float TakeDamage(float damage, IDamagable damagingEntity)
    {
       return this._brain.TakeDamage(damage);

    }
    void Die()
    {
        Debug.Log("Player Defeated");
        GameManager.Instance.ShowDeathScreen();
        GameBlackboard.Instance.GameOver();

        this.enabled = false;
    }
    public PlayerLogic GetBrain()
    {
        return _brain;
    }
    public void ReloadPlayer(PlayerData playerData)
    {
        this.transform.position = playerData.Position;
        this._brain.CurrentHealth=playerData.Health;
        this._brain.ProjectileAmount=playerData.ProjectileAmount;
        this._brain.PotionAmount = playerData.PotionAmount;
        for (int i = 0; i < playerData.KeysCollected; i++)
            GameManager.Instance.AddKey();
        this._brain.Equipment.EquipBetterItem( GameManager.Instance.GetItemByID(playerData.SwordID));
        this._brain.Equipment.EquipBetterItem(GameManager.Instance.GetItemByID(playerData.BowID));
        this._brain.Equipment.EquipBetterItem(GameManager.Instance.GetItemByID(playerData.ArmorID));
        this.enabled = true;
    }
    public void GetStunned(float stunDuration)
    {
        StartCoroutine(StunRoutine(stunDuration));
    }
    private IEnumerator StunRoutine(float duration)
    {
        SetState(EntityState.Stunned);
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Optionally change sprite color so the player KNOWS they are stunned
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.color = Color.yellow;

        // Wait for the stun duration
        yield return new WaitForSeconds(duration);

        // Release the player back to normal
        if (sr != null) sr.color = Color.white;

        Debug.Log("Player is no longer stunned.");
        SetState(EntityState.Free);
    }

    public void SetState(EntityState newState) => CurrentState = newState;
}
