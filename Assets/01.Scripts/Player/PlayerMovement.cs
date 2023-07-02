using System.Collections;
using UnityEngine;
using Photon.Pun;

public enum PlayerState
{
    Idle = 0,
    Walk,
    Run
}

public class PlayerMovement : MonoBehaviourPunCallbacks, IPunObservable
{
    private PhotonView PV;

    private PlayerInput playerInput;
    private PlayerHealth playerHealth;
    private PlayerStamina playerStamina;
    private PlayerShooter playerShooter;

    private Animator anim;
    private CharacterController characterCtrl;

    private Camera followCam;

    private Vector3 currPos;
    private Quaternion currRot;

    [Header("Movement Speed")]
    public float moveSpeed = 6f;
    public float normalSpeed = 6f;
    public float runSpeed = 8f;
    public float walkSpeed = 4f;
    public float jumpVelocity = 5f;
    [Range(0.01f, 1f)] public float airCtrlPercent = 0.01f;

    [Header("Movement Change Smooth")]
    public float speedSmoothTime = 0.1f;
    public float turnSmoothTime = 0.1f;

    private float speedSmoothVelocity;
    private float turnSmoothVelocity;

    private float currentVelocityY;

    public float currentSpeed
        => new Vector2(characterCtrl.velocity.x, characterCtrl.velocity.z).magnitude;

    public bool canRoll { get; private set; }
    public bool canJump { get; private set; }
    public bool canHit { get; private set; }

    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        playerInput = GetComponent<PlayerInput>();
        playerHealth = GetComponent<PlayerHealth>();
        playerStamina = GetComponent<PlayerStamina>();
        playerShooter = GetComponent<PlayerShooter>();

        anim = GetComponent<Animator>();
        characterCtrl = GetComponent<CharacterController>();
    }

    private void Start()
    {
        canHit = true;
        canRoll = true;
        canJump = true;

        if (!PV.IsMine)
            return;

        followCam = Camera.main;

        moveSpeed = DataManager.Instance.userData.
            statUpLevels[(int)StatType.Speed] * 0.5f + 3;
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        if (playerHealth.dead)
            return;

        UpdateAnimation(playerInput.MoveInput);

        if (playerStamina.stamina >= 10f)
        {
            if (playerInput.Roll && !playerInput.Jump && canRoll && !anim.GetBool("isJump"))
                Roll();

            if (playerInput.Jump && !anim.GetBool("isRoll"))
                Jump();
        }
    }

    private void FixedUpdate()
    {
        if (playerHealth.dead)
            return;

        if (PV.IsMine)
        {
            Rotate();
            Move(playerInput.MoveInput);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, currPos, Time.deltaTime * 10.0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, currRot, Time.deltaTime * 10.0f);
        }
    }

    private void Roll()
    {
        canHit = false;
        canRoll = false;
        moveSpeed = runSpeed;
        anim.SetBool("isRoll", true);
        playerStamina.UseStamina(10f);
        StartCoroutine(RollOff());
    }

    private IEnumerator RollOff()
    {
        yield return new WaitForSeconds(0.5f);

        canHit = true;

        yield return new WaitForSeconds(0.3f);

        moveSpeed = normalSpeed;
        anim.SetBool("isRoll", false);

        yield return new WaitForSeconds(3f);
        canRoll = true;
    }

    private void RunAndWalk()
    {
        if (!anim.GetBool("isJump") && !anim.GetBool("isRoll"))
        {
            if (playerInput.Run && !playerInput.Walk && playerStamina.stamina >= 1f && playerStamina.useStamina)
            {
                moveSpeed = runSpeed;
                anim.SetFloat("AnimSpeed", 1.5f);
                playerStamina.UseStamina(0.5f);
            }
            else if ((!playerInput.Run && !playerInput.Walk) || !playerInput.Walk && !playerStamina.useStamina)
            {
                moveSpeed = normalSpeed;
                anim.SetFloat("AnimSpeed", 1f);
            }

            if (playerInput.Walk && !playerInput.Run)
            {
                moveSpeed = walkSpeed;
                anim.SetFloat("AnimSpeed", 0.5f);
            }
            else if (!playerInput.Walk && !playerInput.Run)
            {
                moveSpeed = normalSpeed;
                anim.SetFloat("AnimSpeed", 1f);
            }
        }
    }

    private void Move(Vector2 moveInput)
    {
        RunAndWalk();

        var tartgetSpeed = moveSpeed * moveInput.magnitude;
        var moveDir
            = Vector3.Normalize(transform.forward * moveInput.y + transform.right * moveInput.x);

        var smoothTime
            = (characterCtrl.isGrounded) ? speedSmoothTime : speedSmoothTime / airCtrlPercent;

        tartgetSpeed
            = Mathf.SmoothDamp(currentSpeed, tartgetSpeed, ref speedSmoothVelocity, smoothTime);

        currentVelocityY += Time.deltaTime * Physics.gravity.y;

        var velocity = moveDir * tartgetSpeed + Vector3.up * currentVelocityY;

        characterCtrl.Move(velocity * Time.deltaTime);

        if (characterCtrl.isGrounded)
        {
            anim.SetBool("isJump", false);
            currentVelocityY = 0f;
        }

        SetStateImg(moveInput);
    }

    private void SetStateImg(Vector2 moveInput)
    {
        PlayerState state = PlayerState.Idle;

        if (moveSpeed == runSpeed)
            state = PlayerState.Run;
        else
            state = PlayerState.Walk;

        if(moveInput.x == 0f && moveInput.y == 0f)
            state = PlayerState.Idle;

        if (UIManager.Instance != null)
            UIManager.Instance.SetStateImg(state);
        else if (PVPUIManager.Instance != null)
            PVPUIManager.Instance.SetStateImg(state);
        else if (CampaignUIManager.Instance != null)
            CampaignUIManager.Instance.SetStateImg(state);
    }

    private void Rotate()
    {
        var targetRotation = followCam.transform.eulerAngles.y;

        targetRotation
            = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);

        transform.eulerAngles = Vector3.up * targetRotation;
    }

    private void Jump()
    {
        if (!characterCtrl.isGrounded || !canJump)
            return;

        canJump = false;
        currentVelocityY = jumpVelocity;
        playerStamina.UseStamina(10f);

        anim.SetBool("isJump", true);
        StartCoroutine(JumpCoroutine());
    }

    private IEnumerator JumpCoroutine()
    {
        yield return new WaitForSeconds(1.0f);

        canJump = true;
    }

    private void UpdateAnimation(Vector2 moveInput)
    {
        var animSpeedPercent = currentSpeed / moveSpeed;

        if (!anim.GetBool("isRoll"))
        {
            anim.SetFloat("Vertical Move", moveInput.y * animSpeedPercent, 0.05f, Time.deltaTime);
            anim.SetFloat("Horizontal Move", moveInput.x * animSpeedPercent, 0.05f, Time.deltaTime);
        }
        else
        {
            if (anim.GetFloat("Vertical Move") != 0.0f && moveInput.y != 0.0f)
                anim.SetFloat("Horizontal Move", 0.0f);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();
        }
    }
}