using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    private CharacterController characterCtrl;

    private Camera followCam;

    [Header("Direction Input")]
    public string moveHorizontalAxisName = "Horizontal";
    public string moveVerticalAxisName = "Vertical";

    [Header("Action Input")]
    public string fireButtonName = "Fire1";
    public string shoulderButtonName = "Fire2";
    public string runButtonName = "Run";
    public string walkButtonName = "Walk";
    public string jumpButtonName = "Jump";
    public string rollButtonName = "Roll";
    public string reloadButtonName = "Reload";

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

    public Vector2 MoveInput { get; private set; }
    public bool Run { get; private set; }
    public bool Walk { get; private set; }
    public bool Jump { get; private set; }
    public bool canJump { get; private set; }

    private void Awake()
    {
        characterCtrl = GetComponent<CharacterController>();
    }

    private void Start()
    {
        canJump = true;
        followCam = Camera.main;
    }

    private void Update()
    {
        Run = Input.GetButton(runButtonName);
        Walk = Input.GetButton(walkButtonName);
        Jump = Input.GetButtonDown(jumpButtonName);

        MoveInput = new Vector2(Input.GetAxis(moveHorizontalAxisName), Input.GetAxis(moveVerticalAxisName));

        if (MoveInput.sqrMagnitude > 1)
            MoveInput = MoveInput.normalized;

        if (Jump)
            Jumping();
    }

    private void FixedUpdate()
    {
        Rotate();
        Move(MoveInput);
    }

    private void Rotate()
    {
        var targetRotation = followCam.transform.eulerAngles.y;

        targetRotation
            = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);

        transform.eulerAngles = Vector3.up * targetRotation;
    }

    private void Move(Vector2 moveInput)
    {
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
            currentVelocityY = 0f;
        }
    }

    private void Jumping()
    {
        if (!characterCtrl.isGrounded || !canJump)
            return;

        canJump = false;
        currentVelocityY = jumpVelocity;

        StartCoroutine(JumpCoroutine());
    }

    private IEnumerator JumpCoroutine()
    {
        yield return new WaitForSeconds(1.0f);

        canJump = true;
    }
}