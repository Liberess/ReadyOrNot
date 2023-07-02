using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class PlayerShooter : MonoBehaviourPun
{
    public enum AimState
    {
        Idle = 0,
        HipFire
    }

    public AimState aimState { get; private set; }

    private string sceneName;
    private PhotonView pv;
    public PhotonView PV { get => pv; }

    [Header("View Camera Setting")]
    //[SerializeField] private TPSCamera tpsCam;
    [SerializeField] private Camera fpsCam;
    [SerializeField] private Vector3 offset;
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject head;
    private Vector3 originWeaponRotate;
    private Vector3 originFpsCamRotate;
    private Vector3 originFpsCamPos;
    [SerializeField] private Vector3 currentWeaponRotate = new Vector3(0.153f, 112.27f, 88.79f);
    [SerializeField] private Vector3 currentFpsCamRotate = new Vector3(-4.156f, 5.912f, 0f);
    [SerializeField] private Vector3 currentFpsCamPos = new Vector3(-0.015f, 0.89f, -0.25f);

    [Header("Weapon Setting")]
    public Gun gun;
    [SerializeField] private int gunID;
    public LayerMask excludeTarget;
    [SerializeField] private GameObject[] weapons = new GameObject[9];

    private PlayerInput playerInput;
    private PlayerHealth playerHealth;
    private Animator playerAnim;
    public Camera playerCam;

    private float waitingTimeForReleasingAim = 2.5f;   // HipFire상태에서 Idle 상태로 자동 전환되는 대기 시간
    private float lastFireInputTime = 0f;

    private Vector3 aimPos;
    private bool lineUp => !(Mathf.Abs(
        playerCam.transform.eulerAngles.y - transform.eulerAngles.y) > 1f);
    private bool hasEnoughDistance => !Physics.Linecast(
        transform.position + Vector3.up * gun.firePos[gunID].position.y,
        gun.firePos[gunID].position, ~excludeTarget);

    private void Awake()
    {
        if (excludeTarget != (excludeTarget | (1 << gameObject.layer)))
            excludeTarget |= 1 << gameObject.layer; // Player 레이어를 쏘지 않도록

        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
        gunID = DataManager.Instance.userData.equipGunNum;

        if(photonView.IsMine)
            playerCam = Camera.main;

        playerInput = GetComponent<PlayerInput>();
        playerHealth = GetComponent<PlayerHealth>();
        playerAnim = GetComponent<Animator>();

        originFpsCamPos = weapons[gunID].transform.localPosition;
        originWeaponRotate = weapons[gunID].transform.localEulerAngles;

        bl_UCrosshair.Instance.Change(1); // 기본 점
        bl_UCrosshair.Instance.Reset();

        for(int i = 0; i < weapons.Length; i++)
        {
            if (i == gunID)
                weapons[i].SetActive(true);
            else
                weapons[i].SetActive(false);
        }
    }

    private void OnEnable()
    {
        aimState = AimState.Idle;
        gun.gameObject.SetActive(true);
        gun.Setup(this);
    }

    private void OnDisable()
    {
        aimState = AimState.Idle;
        gun.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!pv.IsMine)
            return;

        if (playerHealth.dead)
            return;

        if (CheckActiveOptionPanel())
            return;

        // 플레이어 위,아래 회전
        var angle = playerCam.transform.eulerAngles.x;
        if (angle > 270f)
            angle -= 360f;

        angle = (angle / 180f * -1f + 0.5f) / 1.2f;

        playerAnim.SetFloat("Angle", angle);

        if (!playerInput.Fire && Time.time >= lastFireInputTime + waitingTimeForReleasingAim)
            aimState = AimState.Idle;

        if (Input.GetKeyDown(KeyCode.V))
            ChangeView();

        if (fpsCam.gameObject.activeSelf)
            fpsCam.transform.eulerAngles = playerCam.transform.eulerAngles + offset;

        UpdateAimTarget();
        UpdateUI();
    }

    private void FixedUpdate()
    {
        if (!pv.IsMine)
            return;

        if (playerHealth.dead)
            return;

        if (CheckActiveOptionPanel())
            return;

        if (playerInput.Fire)
        {
            lastFireInputTime = Time.time;
            Shoot();
        }
        else if (playerInput.Reload)
        {
            Reload();
        }
    }

    public void DevAmmoUp() => gun.magAmmo += 50;

    public void DevDamageUp() => gun.damage += 10f;

    private bool CheckActiveOptionPanel()
    {
        if (sceneName == "Campaign")
        {
            if (CampaignManager.Instance != null && CampaignManager.Instance.optionPanel.activeSelf)
                return true;
        }
        else if (sceneName == "Survival")
        {
            if (GameManager.Instance != null && GameManager.Instance.optionPanel.activeSelf)
                return true;
        }
        else
        {
            if (PVPManager.Instance != null && PVPManager.Instance.optionPanel.activeSelf)
                return true;
        }

        return false;
    }

    #region View
    public void ChangeView()
    {
        if (fpsCam.gameObject.activeSelf)
            ToTpsView();
        else
            ToFpsView();
    }

    public void ToFpsView()
    {
        gun.viewState = Gun.ViewState.FPS;
        head.SetActive(false);
        body.SetActive(false);

        fpsCam.transform.localPosition = currentFpsCamPos;
        fpsCam.transform.localRotation = Quaternion.Euler(currentFpsCamRotate);
        weapons[gunID].transform.localRotation = Quaternion.Euler(currentWeaponRotate);

        fpsCam.gameObject.SetActive(true);
    }

    public void ToTpsView()
    {
        gun.viewState = Gun.ViewState.TPS;
        head.SetActive(true);
        body.SetActive(true);

        fpsCam.transform.localPosition = originFpsCamPos;
        fpsCam.transform.localRotation = Quaternion.Euler(originFpsCamRotate);
        weapons[gunID].transform.localRotation = Quaternion.Euler(originWeaponRotate);

        fpsCam.gameObject.SetActive(false);
    }
    #endregion

    private void UpdateUI()
    {
        if (gun == null)
            return;

        if(UIManager.Instance != null)
            UIManager.Instance.SetAmmoTxt(gun.magAmmo, gun.ammoRemain);
        else if (PVPUIManager.Instance != null)
            PVPUIManager.Instance.SetAmmoTxt(gun.magAmmo, gun.ammoRemain);
        else if (CampaignUIManager.Instance != null)
            CampaignUIManager.Instance.SetAmmoTxt(gun.magAmmo, gun.ammoRemain);

        Shoulder();
    }

    private void Shoulder()
    {
        if (Input.GetButtonDown(playerInput.shoulderButtonName))
        {
            bl_UCrosshair.Instance.OnAim(true);
            bl_UCrosshair.Instance.Change(DataManager.Instance.userData.crosshairNum);
        }

        if (Input.GetButtonUp(playerInput.shoulderButtonName))
        {
            bl_UCrosshair.Instance.OnAim(false);
            bl_UCrosshair.Instance.Change(1);
        }
    }

    public void Shoot()
    {
        if (aimState == AimState.Idle)
        {
            if (lineUp)
                aimState = AimState.HipFire;
        }
        else if (aimState == AimState.HipFire)
        {
            if (gun.Fire(aimPos))
            {
                bl_UCrosshair.Instance.OnFire();
                playerAnim.SetTrigger("Shoot");
                if (weapons[gunID].GetComponent<Animator>() != null)
                    weapons[gunID].GetComponent<Animator>().SetTrigger("doShot");
            }
        }
    }

    public void Reload()
    {
        if (gun.Reload())
            playerAnim.SetTrigger("Reload");
    }

    private void UpdateAimTarget()
    {
        RaycastHit hit;
        var ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out hit, gun.fireDistance, ~excludeTarget))
        {
            aimPos = hit.point;

            if (Physics.Linecast(gun.firePos[gunID].position, hit.point, out hit, ~excludeTarget))
                aimPos = hit.point;
        }
        else
        {
            aimPos = playerCam.transform.position + playerCam.transform.forward * gun.fireDistance;
        }
    }
}