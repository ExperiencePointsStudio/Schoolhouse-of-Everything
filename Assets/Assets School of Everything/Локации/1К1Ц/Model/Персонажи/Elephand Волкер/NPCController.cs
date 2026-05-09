using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCController : MonoBehaviour
{
    [Header("Links")]
    public Transform visual;
    public Transform headPoint;
    Animator animator;

    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float rotationSpeed = 8f;
    public float walkTime = 3f;
    public float idleTime = 2f;
    public float gravity = -9.81f;

    CharacterController cc;

    Vector3 moveDirection;      // направление, КУДА хотим идти
    float stateTimer;
    bool isWalking;
    bool inDialogue;

    Transform lookTarget;
    Vector3 visualLocalPos;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        animator = visual.GetComponentInChildren<Animator>();
    }

    void Start()
    {
        visualLocalPos = visual.localPosition;
        EnterIdle();
    }

    void Update()
    {
        if (inDialogue)
        {
            FaceTarget();
            return;
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            if (isWalking) EnterIdle();
            else EnterWalk();
        }

        if (isWalking)
            DoMove();
        if (animator != null)
            animator.SetBool("IsWalking", isWalking);
    }

    void LateUpdate()
    {
        if (visual != null)
            visual.localPosition = visualLocalPos;
    }

    void DoMove()
    {
        // Поворот корня в сторону цели
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        // Движение ТОЛЬКО вперёд
        Vector3 motion = transform.forward * moveSpeed;
        motion.y += gravity * Time.deltaTime;

        cc.Move(motion * Time.deltaTime);
    }

    void EnterIdle()
    {
        isWalking = false;
        stateTimer = idleTime;
        moveDirection = Vector3.zero;
    }

    void EnterWalk()
    {
        isWalking = true;
        stateTimer = walkTime;

        Vector2 r = Random.insideUnitCircle.normalized;
        moveDirection = new Vector3(r.x, 0f, r.y);
    }

    void FaceTarget()
    {
        if (lookTarget == null) return;

        Vector3 dir = lookTarget.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

    public void OnPlayerEnter(Transform player)
    {
        inDialogue = true;
        lookTarget = player;
        moveDirection = Vector3.zero;
    }

    public void OnPlayerExit()
    {
        inDialogue = false;
        lookTarget = null;
        EnterIdle();
    }
}
