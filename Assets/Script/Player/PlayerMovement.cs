using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float SpeedModifier { get; set; } = 1.0f;

    private PlayerManager _playerManager;

    private Rigidbody2D rb;
    private Vector2 currentInput;
    private Animator animator;

    [Header("Facing Direction")]
    [SerializeField]public Vector2 FacingDirection { get; private set; } = Vector2.down;

    public void Initialize(PlayerManager core)
    {
        this._playerManager = core;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

    }
    public void SetInputVector(Vector2 input)
    {
        currentInput = input;
        if(currentInput != Vector2.zero)
            FacingDirection = currentInput.normalized;

    }
    void FixedUpdate()
    {
        if (_playerManager.CurrentState != EntityState.Stunned)
        {
            move();
        }
    }
    private void move()
    {

        rb.linearVelocity = currentInput * _playerManager.Stats.moveSpeed*SpeedModifier;// speed of the player is his speed times his direction

        // set values for the animator transitions
        animator.SetFloat("Horizontal", currentInput.x);
        animator.SetFloat("Vertical", currentInput.y);

        animator.SetFloat("LastHorizontal", FacingDirection.x);
        animator.SetFloat("LastVertical", FacingDirection.y);

    }
}