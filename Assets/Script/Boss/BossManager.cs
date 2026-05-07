using System.Collections;
using Assets.Algorithm.BlackBoard;
using Assets.Data;
using Assets.EntityLogic;
using Assets.Script.Game_Manager;
using Assets.Script.Projectile;
using UnityEngine;

public class BossManager : EnemyManager
{
    [Header("Special attacks prefabs")]
    public GameObject ShockwavePrefab;
    public GameObject MeteorPrefab;

    [HideInInspector]
    public float TimePlayerInMeleeRange=0f;
    private bool _isPlayerInMeleeRange = false;

    public PlayerManager Player;// the boss always knows where the player is.

    private Coroutine _meteorCoroutine;

    [Header("Icon Sprites")]
    public Sprite _meteorIcon;
    public Sprite _slamIcon;
    public override void InitializeEnemy()
    {
        InstanceID = this.name; // CACHED ONCE!


        this.Vision = GetComponent<EnemyVision>();
        this.Movment = GetComponent<EnemyMovement>();
        this.Combat = GetComponent<EnemyCombat>();

        this.Movment.Initialize(this);
        this.Vision.Initialize(this);
        this.Combat.Initialize(this);

        GetComponent<EnemyHealthController>().enabled = true;

        _brain = new BossLogic((BossStats)Stats, this);

        // Subscribe to the death event
        _brain.OnDeath += () =>
        {
            GetComponent<WinScript>().OnPlayerWon();
            Destroy(gameObject);
        };

        PlayerManager player = FindAnyObjectByType<PlayerManager>();
        if (player != null)
        {
            this.Player = player;
        }
        else
        {
            Debug.Log("player transform not found");
        }

    }
    public new BossLogic GetBrain()
    {
        return ((BossLogic)_brain);
    }
    public override void Update()
    {
        GameBlackboard.Instance.PlayerDetected(Player);
        GameBlackboard.Instance.WriteData(EnvironmentKey.PlayerHealth,Player.GetBrain().CurrentHealth);
        if (_isPlayerInMeleeRange)
        {
            TimePlayerInMeleeRange += Time.deltaTime;
        }
        // Tick the boss brain every frame
        if (_brain != null)
        {
            _brain.Tick(Time.deltaTime);
        }
    }

    public void ExecuteGroundSlam()
    {
        if (ShockwavePrefab != null)
        {
            // Spawn the shockwave at the Boss position
            GameObject waveObj = Instantiate(ShockwavePrefab, transform.position, Quaternion.identity);

            // Get the script and activate the math!
            Shockwave waveScript = waveObj.GetComponent<Shockwave>();
            if (waveScript != null)
            {
                BossStats stats = (BossStats)Stats;
                waveScript.Initialize(stats.BodySlamDamage, stats.SlamStunDuration, stats.SlamMaxRadius, stats.SlamExpandDuration);
            }
            Debug.Log("Boss uses its body slam attack!");
            GetBrain().PerformSlamAttack();


        }
        else
        {
            Debug.LogWarning("Boss is trying to Slam, but the Shockwave Prefab is missing in the Inspector!");
        }
        // e.g., Triggering an animation, spawning a massive AoE, dashing, etc.
    }
    public void ExecuteMeteorStrike()
    {
        Debug.Log("Boss begins casting Meteor Strike!");

        // Optional: Trigger your boss casting animation here!
        // anim.SetTrigger("CastSpell");
        if (_meteorCoroutine != null) StopCoroutine(_meteorCoroutine);
        _meteorCoroutine = StartCoroutine(SpawnMeteorsRoutine());
    }

    private IEnumerator SpawnMeteorsRoutine()
    {
        // We divide the 3.0 second action duration by the number of meteors 
        // to get a perfect, steady rhythm of falling meteors.
        BossStats stats=(BossStats)Stats;
        float delayBetweenSpawns = 3.0f / stats.MeteorsToSpawn;
        GetBrain().PerformMeteorStrike();

        for (int i = 0; i < stats.MeteorsToSpawn; i++)
        {
            SpawnSingleMeteor();
            yield return new WaitForSeconds(delayBetweenSpawns);
        }
    }
    public void StopMeteorStrike()
    {
        if (_meteorCoroutine != null)
        {
            StopCoroutine(_meteorCoroutine);
            _meteorCoroutine = null; 
        }
    }
    private void SpawnSingleMeteor()
    {
        if (MeteorPrefab == null) return;
        BossStats stats = (BossStats)Stats;

        Transform playerTransform = GameBlackboard.Instance.ReadData<Transform>(EnvironmentKey.PlayerPosition);
        Vector2 targetCenter = playerTransform != null ? (Vector2)playerTransform.position : (Vector2)transform.position;

        // Pick a completely random spot within a circle around the player
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * stats.MeteorSpawnRadius;
        Vector2 spawnPosition = targetCenter + randomOffset;

        // Spawn it and initialize it
        GameObject meteorObj = Instantiate(MeteorPrefab, spawnPosition, Quaternion.identity);
        Meteor meteorScript = meteorObj.GetComponent<Meteor>();

        if (meteorScript != null)
        {
            meteorScript.Initialize(stats.MeteorStrikeDamage, stats.MeteorRadius, stats.MeteorTelegraphTime);
        }
    }
    public override void ShowAction(RoleType newAction)
    {
        iconDisplay.enabled = true;
        if (CurrentRole == newAction)
        {
            return;
        }
        CurrentRole = newAction;
        // Swap the picture based on the action chosen
        switch (newAction)
        {
            case RoleType.GroundSlamming: iconDisplay.sprite = _slamIcon; break;
            case RoleType.MeteorStriking: iconDisplay.sprite = _meteorIcon; break;
            default:
                base.ShowAction(newAction);
                break;
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInMeleeRange = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TimePlayerInMeleeRange = 0f;
            _isPlayerInMeleeRange = false;
        }
    }

}
