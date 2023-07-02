using UnityEngine;
using Photon.Pun;

public class PlayerInput : MonoBehaviour
{
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

    public Vector2 MoveInput { get; private set; }
    public bool Fire { get; private set; }
    public bool Shoulder { get; private set; }
    public bool Run { get; private set; }
    public bool Walk { get; private set; }
    public bool Jump { get; private set; }
   public bool Roll { get; private set; } 
    public bool Reload { get; private set; }

    private PhotonView pv;
    public PhotonView PV { get => pv; }

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (!pv.IsMine)
            return;

        if(GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            MoveInput = Vector2.zero;
            Fire = false;
            Shoulder = false;
            Run = false;
            Jump = false;
            Roll = false;
            Reload = false;
            return;
        }

        MoveInput = new Vector2(Input.GetAxis(moveHorizontalAxisName), Input.GetAxis(moveVerticalAxisName));

        if (MoveInput.sqrMagnitude > 1)
            MoveInput = MoveInput.normalized;

        Fire = Input.GetButton(fireButtonName);
        Shoulder = Input.GetButton(shoulderButtonName);
        Run = Input.GetButton(runButtonName);
        Walk = Input.GetButton(walkButtonName);
        Jump = Input.GetButtonDown(jumpButtonName);
        Roll = Input.GetButtonDown(rollButtonName);
        Reload = Input.GetButtonDown(reloadButtonName);
    }
}