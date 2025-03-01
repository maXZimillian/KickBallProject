using System.Collections;
using UnityEngine;

public class GoalkeeperPlayerControl : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxJumpDistance = 2.7f;
    [SerializeField] private Rigidbody player;
    [SerializeField] private Vector3 colliderSize = new Vector3(1.5f, 2.7f, 1);
    [SerializeField] private Vector3 diveColliderSize = new Vector3(2.7f, 1.6f, 1);
    [SerializeField] private Vector3 blockolliderSize = new Vector3(2.6f, 2.7f, 1);
    [SerializeField] private float actionDuration = 1f;
    private bool isPerformingAction = false;
    private FixedJoystick joystick;
    private TouchHandler th;
    private Animator animator;
    private BoxCollider col;
    private GameController gameController;
    public string currentAnimationTrigger = "";
    
    public TouchHandler touchHandler
    {
        get { return th; }
        set
        {
            th = value;
            AssignTouchHandler();
        }
    }
    
    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
        animator = GetComponent<Animator>();
        col = GetComponent<BoxCollider>();
        ChangeColliderSize(colliderSize);
        col.center = new Vector3(0, 1.3f, 0);
        if (player == null) player = GetComponent<Rigidbody>();
    }

    private void AssignTouchHandler()
    {
        joystick = FindObjectOfType<FixedJoystick>();
    }

    private void ChangeColliderSize(Vector3 size)
    {
        col.size = size;
    }

    private void Update()
    {
        if (gameController.playerRole == PlayerTypes.goalkeeper)
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        if (joystick == null)
            AssignTouchHandler();
        if (!isPerformingAction)
        {
            float moveX = -joystick.Horizontal;
            float moveZ = -joystick.Vertical;
            Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

            if (moveDirection.magnitude >= 0.1f)
            {
                transform.position += moveDirection * moveSpeed * Time.deltaTime;
                UpdateAnimationTrigger(GetWalkTrigger(moveX, moveZ));
            }
            else
            {
                UpdateAnimationTrigger("BackToIdle");
            }
        }
        else
        {

        }
    }

    private string GetWalkTrigger(float moveX, float moveZ)
    {
        if (Mathf.Abs(moveX) > Mathf.Abs(moveZ))
        {
            return moveX > 0 ? "WalkRight" : "WalkLeft";
        }
        else
        {
            return moveZ > 0 ? "WalkForward" : "WalkBack";
        }
    }

    public void UpdateAnimationTrigger(string newTrigger)
    {
        if (currentAnimationTrigger != newTrigger)
        {
            currentAnimationTrigger = newTrigger;
            animator.SetTrigger(currentAnimationTrigger);
        }
    }

    public void JumpHigh()
    {
        if (isPerformingAction) return;
        isPerformingAction = true;
        Vector3 jumpDirection = new Vector3(-joystick.Horizontal * maxJumpDistance * 0.3f, 2, 0).normalized;
        player.AddForce(jumpDirection * 7f, ForceMode.VelocityChange);
        UpdateAnimationTrigger("Jump");
        StartCoroutine(ResetAction());
    }

    public void JumpMedium()
{
    if (isPerformingAction) return;
    isPerformingAction = true;

    float inputX = joystick.Horizontal;
    if (Mathf.Abs(inputX) < 0.2f) 
        inputX = Mathf.Sign(inputX) * 0.5f; // Минимальный порог

    Vector3 jumpDirection = new Vector3(-inputX * maxJumpDistance * 0.5f, 1.1f, 0); // Без нормализации
    player.AddForce(jumpDirection * 5f, ForceMode.VelocityChange);
    UpdateAnimationTrigger(-inputX > 0 ? "RightDive" : "LeftDive");
    ChangeColliderSize(diveColliderSize);
    StartCoroutine(ResetAction());
}

public void BlockLow()
{
    if (isPerformingAction) return;
    isPerformingAction = true;

    float inputX = joystick.Horizontal;
    if (Mathf.Abs(inputX) < 0.2f) 
        inputX = Mathf.Sign(inputX) * 0.5f; // Минимальный порог

    Vector3 diveDirection = new Vector3(-inputX * maxJumpDistance, 0.5f, 0); // Без нормализации
    player.AddForce(diveDirection * 4f, ForceMode.VelocityChange);
    UpdateAnimationTrigger(-inputX > 0 ? "RightBlock" : "LeftBlock");
    ChangeColliderSize(blockolliderSize);
    StartCoroutine(ResetAction());
}

    private IEnumerator ResetAction()
    {
        yield return new WaitForSeconds(actionDuration);
        ChangeColliderSize(colliderSize);
        isPerformingAction = false;
    }
}
